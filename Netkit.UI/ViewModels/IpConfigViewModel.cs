using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetKit.Lib.Arp;
using NetKit.Lib.IP;
using NetKit.UI.Models;
using NetKit.UI.Services;
using NetKit.UI.ViewModels.PageViewModels;
using NetworkToolkitModern.Lib.Vendor;

namespace NetKit.UI.ViewModels;

public partial class IpConfigViewModel : ViewModelBase
{
    private readonly Dictionary<string, string> _vendorCache = new();
    private readonly VendorLookup _vendorLookup = new();
    [ObservableProperty] private ObservableCollection<UnicastIPAddressInformation> _addresses = [];
    [ObservableProperty] private IPAddressInformationCollection? _anycastAddresses;
    [ObservableProperty] private ObservableCollection<ArpEntry> _arpEntries = [];
    [ObservableProperty] private string? _arpTableFilterText = string.Empty;
    [ObservableProperty] private bool _automaticPrivateAddressingActive;
    [ObservableProperty] private bool _automaticPrivateAddressingEnabled;
    [ObservableProperty] private string? _bytesReceived;
    [ObservableProperty] private string? _bytesSent;
    private string _cachedFilterText = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private IPAddressCollection? _dhcpAddresses;
    [ObservableProperty] private bool _dhcpEnabled;
    [ObservableProperty] private ObservableCollection<IPAddress> _dnsAddresses = [];
    [ObservableProperty] private bool _dnsEnabled;
    [ObservableProperty] private string? _dnsSuffix;
    [ObservableProperty] private bool _doRefresh = true;
    [ObservableProperty] private bool _dynamicDnsEnabled;
    [ObservableProperty] private ObservableCollection<ArpEntry> _filteredArpEntries = [];
    [ObservableProperty] private bool _forwardingEnabled;
    [ObservableProperty] private ObservableCollection<GatewayIPAddressInformation> _gatewayAddresses = [];


    [ObservableProperty] private bool _hasArp;
    [ObservableProperty] private bool _hasRoutes;
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _incomingPacketsDiscarded;
    [ObservableProperty] private string? _incomingPacketsWithErrors;
    [ObservableProperty] private string? _incomingUnknownProtocolPackets;

    [ObservableProperty] private int _index;
    [ObservableProperty] private ObservableCollection<InterfaceModel> _interfaceModels = new();
    [ObservableProperty] private bool _isAddressInfoExpanded = false;
    [ObservableProperty] private bool _isArpTableExpanded = false;
    [ObservableProperty] private bool _isFocused;

    [ObservableProperty] private bool _isInterfaceInfoExpanded = false;
    [ObservableProperty] private bool _isRouteTableExpanded = false;
    [ObservableProperty] private bool _isStatisticsExpanded = false;
    [ObservableProperty] private int _mtu;
    [ObservableProperty] private ObservableCollection<MulticastIPAddressInformation> _multicastAddresses = [];
    [ObservableProperty] private bool _multicastSupport;

    // private int _metric;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _nonUnicastPacketsReceived;
    [ObservableProperty] private string? _nonUnicastPacketsSent;
    [ObservableProperty] private string? _outgoingPacketsDiscarded;
    [ObservableProperty] private string? _outgoingPacketsWithErrors;
    [ObservableProperty] private string? _outputQueueLength;
    [ObservableProperty] private string? _physicalAddress;
    [ObservableProperty] private bool _receiveOnly;
    [ObservableProperty] private int? _refreshInterval;

    private Timer? _refreshTimer;
    [ObservableProperty] private ObservableCollection<RouteRowModel> _routeRowModels = new();
    [ObservableProperty] private int _selectedInterface;
    [ObservableProperty] private string? _speed;
    [ObservableProperty] private string? _status;
    [ObservableProperty] private ObservableCollection<IPAddress>? _subnetMask;
    [ObservableProperty] private string? _type;
    [ObservableProperty] private string? _unicastPacketsReceived;
    [ObservableProperty] private string? _unicastPacketsSent;
    [ObservableProperty] private bool _usesWins;
    [ObservableProperty] private IPAddressCollection? _winsServerAddresses;

    [ObservableProperty] private ObservableCollection<IpConfigurationProfileViewModel> _profiles;
    [ObservableProperty] private IpConfigurationProfileViewModel _selectedProfile;

    public IpConfigViewModel() : this(new IpConfigurationPageViewModel())
    {
    }

    public IpConfigViewModel(IpConfigurationPageViewModel ipConfigurationPageViewModel)
    {
        _profiles = ipConfigurationPageViewModel.IpConfigurationProfiles;
        _selectedProfile = _profiles[0];
        Update();
        RefreshInterval = 1;
        FilteredArpEntries = ArpEntries;
    }

    partial void OnArpEntriesChanged(ObservableCollection<ArpEntry> value)
    {
        if (string.IsNullOrEmpty(ArpTableFilterText))
        {
            FilteredArpEntries = value;
            return;
        }

        if (ArpTableFilterText != _cachedFilterText) OnArpTableFilterTextChanged(ArpTableFilterText);
    }

    partial void OnArpTableFilterTextChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            FilteredArpEntries = ArpEntries;
            return;
        }

        _cachedFilterText = value;

        // Assigns entry models containing filter text to the UI bound collection.
        FilteredArpEntries = new ObservableCollection<ArpEntry>(ArpEntries
            .Where(x =>
                x.Vendor.ToLower().Contains(value.ToLower()) ||
                x.IpAddress.ToLower().Contains(value.ToLower()) ||
                x.MacAddress.ToLower().Contains(value.ToLower())).ToList());
    }

    partial void OnDoRefreshChanged(bool value)
    {
        if (RefreshInterval is null) return;
        if (!value) _refreshTimer?.Dispose();
        else _refreshTimer = new Timer(Refresh, null, 0, RefreshInterval.Value * 1000);
    }

    partial void OnRefreshIntervalChanged(int? value)
    {
        if (value is null) return;
        _refreshTimer?.Dispose();
        _refreshTimer = new Timer(Refresh, null, 0, value.Value * 1000);
    }

    partial void OnSelectedInterfaceChanged(int value)
    {
        if (value < 0) return;
        Update();
    }

    [RelayCommand]
    public void ExpandAll()
    {
        IsInterfaceInfoExpanded = true;
        IsAddressInfoExpanded = true;
        IsStatisticsExpanded = true;
        IsRouteTableExpanded = true;
        IsArpTableExpanded = true;
    }

    [RelayCommand]
    public void CollapseAll()
    {
        IsInterfaceInfoExpanded = false;
        IsAddressInfoExpanded = false;
        IsStatisticsExpanded = false;
        IsRouteTableExpanded = false;
        IsArpTableExpanded = false;
    }

    /// <summary>
    ///     Updates the UI with new information from the given network interface.
    /// </summary>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private void UpdateInterfaceDisplay(NetworkInterface networkInterface)
    {
        // _metric = Route.GetMetric(networkInterface); May remove.

        // Most of this function is just reassigning UI bound properties to updated values from the passed network interface.
        // It tries to do this only when necessary to allow more interactivity between refreshes.

        if (Name != networkInterface.Name) Name = networkInterface.Name;
        if (Type != networkInterface.NetworkInterfaceType.ToString())
            Type = networkInterface.NetworkInterfaceType.ToString();
        if (Description != networkInterface.Description) Description = networkInterface.Description;
        if (Id != networkInterface.Id) Id = networkInterface.Id;
        if (Speed != $"{networkInterface.Speed / 1000000} Mbps") Speed = $"{networkInterface.Speed / 1000000} Mbps";
        if (Status != networkInterface.OperationalStatus.ToString())
            Status = networkInterface.OperationalStatus.ToString();
        if (ReceiveOnly != networkInterface.IsReceiveOnly) ReceiveOnly = networkInterface.IsReceiveOnly;

        // Converts the MAC address to the *proper* formatting before assignment.
        var rawMacAddress = networkInterface.GetPhysicalAddress().ToString();
        var formattedMacAddress = new StringBuilder(rawMacAddress);
        for (var i = 2; i < formattedMacAddress.Length; i += 3) formattedMacAddress.Insert(i, ":");
        if (PhysicalAddress != formattedMacAddress.ToString()) PhysicalAddress = formattedMacAddress.ToString();

        var ipProperties = networkInterface.GetIPProperties();
        Addresses = new ObservableCollection<UnicastIPAddressInformation>(
            ipProperties.UnicastAddresses.Where(y => y.Address.AddressFamily == AddressFamily.InterNetwork));
        DnsAddresses =
            new ObservableCollection<IPAddress>(
                ipProperties.DnsAddresses.Where(x => x.AddressFamily == AddressFamily.InterNetwork));
        if (DnsSuffix != ipProperties.DnsSuffix) DnsSuffix = ipProperties.DnsSuffix;
        GatewayAddresses = new ObservableCollection<GatewayIPAddressInformation>(
            ipProperties.GatewayAddresses.Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork));
        MulticastAddresses = new ObservableCollection<MulticastIPAddressInformation>(
            ipProperties.MulticastAddresses.Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork));
        DhcpAddresses = ipProperties.DhcpServerAddresses;
        if (DynamicDnsEnabled != ipProperties.IsDynamicDnsEnabled) DynamicDnsEnabled = ipProperties.IsDynamicDnsEnabled;
        SubnetMask = new ObservableCollection<IPAddress>(Addresses.Select(x => x.IPv4Mask));

        // Some interfaces will only have IPv4 or IPv6 and not both.
        // This will handle either case.
        try
        {
            // Attempts to retrieve from IPv4 properties.
            var iPv4Properties = networkInterface.GetIPProperties().GetIPv4Properties();
            if (Index != iPv4Properties.Index) Index = iPv4Properties.Index;
            if (Mtu != iPv4Properties.Mtu) Mtu = iPv4Properties.Mtu;
            if (UsesWins != iPv4Properties.UsesWins) UsesWins = iPv4Properties.UsesWins;
            if (DhcpEnabled != iPv4Properties.IsDhcpEnabled) DhcpEnabled = iPv4Properties.IsDhcpEnabled;
            if (ForwardingEnabled != iPv4Properties.IsForwardingEnabled)
                ForwardingEnabled = iPv4Properties.IsForwardingEnabled;
            if (AutomaticPrivateAddressingActive != iPv4Properties.IsAutomaticPrivateAddressingActive)
                AutomaticPrivateAddressingActive = iPv4Properties.IsAutomaticPrivateAddressingActive;
            if (AutomaticPrivateAddressingEnabled != iPv4Properties.IsAutomaticPrivateAddressingEnabled)
                AutomaticPrivateAddressingEnabled = iPv4Properties.IsAutomaticPrivateAddressingEnabled;
        }
        catch (NetworkInformationException e)
        {
            // Falls back only if certain that IPv4 is not supported.
            // Much fewer details are relevant or available.
            if (e.ErrorCode != 10043) throw;
            var iPv6Properties = networkInterface.GetIPProperties().GetIPv6Properties();
            if (Index != iPv6Properties.Index) Index = iPv6Properties.Index;
            if (Mtu != iPv6Properties.Mtu) Mtu = iPv6Properties.Mtu;
        }

        // Interface Statistics:
        var statistics = networkInterface.GetIPStatistics();
        BytesSent = statistics.BytesSent.ToString("N0");
        BytesReceived = statistics.BytesReceived.ToString("N0");
        UnicastPacketsSent = statistics.UnicastPacketsSent.ToString("N0");
        UnicastPacketsReceived = statistics.UnicastPacketsReceived.ToString("N0");
        NonUnicastPacketsSent = statistics.NonUnicastPacketsSent.ToString("N0");
        NonUnicastPacketsReceived = statistics.NonUnicastPacketsReceived.ToString("N0");
        OutgoingPacketsDiscarded = statistics.OutgoingPacketsDiscarded.ToString("N0");
        IncomingPacketsDiscarded = statistics.IncomingPacketsDiscarded.ToString("N0");
        OutgoingPacketsWithErrors = statistics.OutgoingPacketsWithErrors.ToString("N0");
        IncomingPacketsWithErrors = statistics.IncomingPacketsWithErrors.ToString("N0");
        OutputQueueLength = statistics.OutputQueueLength.ToString("N0");
        IncomingUnknownProtocolPackets = statistics.IncomingUnknownProtocolPackets.ToString("N0");


        RefreshArp();

        // Retrieves route table entries for the current interface.
        var routes = Route.GetRoutes().Where(x => x.IfIndex == Index).ToList();

        // Decides whether to even display a route table.
        HasRoutes = routes.Count != 0;

        // Initializes the UI collection before adding route entries.
        Dispatcher.UIThread.Invoke(() =>
        {
            RouteRowModels = new ObservableCollection<RouteRowModel>();
            foreach (var route in routes) RouteRowModels.Add(new RouteRowModel(route));
        });
    }

    private void RefreshArp()
    {
        var currentArpEntries = ArpEntries.ToList();
        var latestArpEntries = Arp.GetArpCache().Where(x => x.Index == Index).ToList();

        HasArp = latestArpEntries.Count != 0;

        Dispatcher.UIThread.Invoke(() =>
        {
            foreach (var arpEntry in currentArpEntries)
            {
                if (latestArpEntries.FirstOrDefault(x => x.MacAddress == arpEntry.MacAddress) is not null) continue;
                ArpEntries.Remove(arpEntry);
            }

            foreach (var arpEntry in latestArpEntries)
            {
                if (!_vendorCache.ContainsKey(arpEntry.MacAddress))
                    _vendorCache.Add(arpEntry.MacAddress, _vendorLookup.GetVendorName(arpEntry.MacAddress));
                var vendor = _vendorCache[arpEntry.MacAddress];
                arpEntry.Vendor = string.IsNullOrEmpty(vendor) ? "Unknown" : vendor;
                if (currentArpEntries.FirstOrDefault(x => x.MacAddress == arpEntry.MacAddress) is not null) continue;
                ArpEntries.Add(arpEntry);
            }

            OnArpTableFilterTextChanged(ArpTableFilterText);
        });
    }

    private void Refresh(object? state)
    {
        Task.Run(Update);
    }

    /// <summary>
    ///     Everything that needs to happen when a refresh is called to update the UI.
    /// </summary>
    private void Update()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
            .OrderBy(o => o.GetIPStatistics().BytesReceived).Reverse().ToList();
        var comparedInterfaces = new ObservableCollection<InterfaceModel>();
        foreach (var networkInterface in networkInterfaces)
            comparedInterfaces.Add(new InterfaceModel(networkInterface));

        if (InterfaceModels.Count < comparedInterfaces.Count)
            foreach (var model in comparedInterfaces)
            {
                var difference = InterfaceModels.FirstOrDefault(x => x.Index == model.Index);
                if (difference is null) InterfaceModels.Add(model);
            }

        if (InterfaceModels.Count > comparedInterfaces.Count)
            for (var index = 0; index < InterfaceModels.Count; index++)
            {
                var model = InterfaceModels[index];
                var difference = comparedInterfaces.FirstOrDefault(x => x.Index == model.Index);
                if (difference is null) InterfaceModels.Remove(model);
            }

        try
        {
            var currentInterface =
                networkInterfaces.FirstOrDefault(x => x.Name == InterfaceModels[SelectedInterface].Name);
            if (currentInterface is null) return;

            UpdateInterfaceDisplay(currentInterface);
        }
        catch (ArgumentOutOfRangeException)
        {
            SelectedInterface = 0;
        }
    }

    [RelayCommand]
    public void FlushArpCommand(uint index)
    {
        Arp.FlushArpTable(index);
        RefreshArp();
    }

    [RelayCommand]
    public void RefreshCommand()
    {
        Update();
    }

    [RelayCommand]
    public void ApplyProfileCommand()
    {
        var ipService = new IpConfigurationService((uint)Index);
        ipService.ApplyProfile(SelectedProfile);
    }

    [RelayCommand]
    public void SetDhcpCommand()
    {
        var ipService = new IpConfigurationService((uint)Index);
        ipService.SetDhcp();
    }

    [RelayCommand]
    public void ResetInterfaceCommand()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var iperfDirectory = Path.Combine(baseDirectory, "iperf\\iperf3.exe");
        var startInfo = new ProcessStartInfo
        {
            FileName = iperfDirectory, // Change to "iperf" if using iperf2
            Arguments = "-c ping.online.net -p 5208  --json",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process();
        process.StartInfo = startInfo;
        try
        {
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse the JSON output as needed
            Debug.WriteLine(output);
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., iperf not installed)
            Debug.WriteLine($"Error running iperf: {ex.Message}");
        }
    }
}