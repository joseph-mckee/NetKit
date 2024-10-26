using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using NetKit.Lib.Arp;
using NetKit.Lib.IP;
using NetKit.Lib.Ping;
using NetKit.Lib.Tcp;
using NetKit.UI.Models;
using NetworkToolkitModern.Lib.Vendor;

namespace NetKit.UI.ViewModels.PageViewModels;

public partial class ScanPageViewModel : ViewModelBase, IPageViewModel
{
    private readonly Stopwatch _stopwatch = new();
    private readonly DispatcherTimer _timer = new();
    private readonly VendorLookup _vendorLookup = new();
    private CancellationTokenSource? _cancellationTokenSource;
    [ObservableProperty] private string _elapsed = "00:00:00";

    [ObservableProperty] private ObservableCollection<ScannedHostModel> _filteredScannedHosts =
    [
        new ScannedHostModel
        {
            Hostname = "guardian.techkee.home",
            IpAddress = "172.16.12.1",
            MacAddress = "7C:5A:1C:D9:4E:CC",
            Vendor = "Sophos"
        }
    ];

    [ObservableProperty] private int _goal;
    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private bool _isStopped = true;
    [ObservableProperty] private int _progress;
    [ObservableProperty] private string _progressText = "Scanned: 0/0";
    [ObservableProperty] private string _rangeInput = "192.168.1.1-192.168.1.254";
    [ObservableProperty] private string? _scanFilterText = string.Empty;
    [ObservableProperty] private ObservableCollection<ScannedHostModel> _scannedHosts = [];

    private readonly ConcurrentQueue<Action> _uiUpdateQueue = new();
    private readonly DispatcherTimer _uiUpdateTimer;


    public ScanPageViewModel()
    {
        ScannedHosts = [];
        Progress = 0;
        Goal = 0;

        _uiUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _uiUpdateTimer.Tick += ProcessUiUpdates;
        _uiUpdateTimer.Start();

        ScanLocalCommand();
    }

    private void ProcessUiUpdates(object? sender, EventArgs e)
    {
        while (_uiUpdateQueue.TryDequeue(out var uiUpdate))
        {
            uiUpdate();
        }
    }

    partial void OnGoalChanged(int value)
    {
        ProgressText = $"Scanned: {Progress} / {Goal}";
    }

    partial void OnScanFilterTextChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            var hostsToAdd = ScannedHosts.Except(FilteredScannedHosts);
            foreach (var hostModel in hostsToAdd)
            {
                FilteredScannedHosts.Add(hostModel);
            }

            return;
        }

        var matchingHosts = ScannedHosts.Where(x =>
            x.Hostname.ToString().ToLower().Contains(value.ToLower()) ||
            x.IpAddress.ToString().ToLower().Contains(value.ToLower()) ||
            x.MacAddress.ToString().ToLower().Contains(value.ToLower()) ||
            x.Vendor.ToString().ToLower().Contains(value.ToLower())).ToList();

        var hostsToFilter = ScannedHosts.Except(matchingHosts).ToList();
        var hostsToShow = matchingHosts.Except(FilteredScannedHosts);

        foreach (var hostModel in hostsToFilter)
        {
            FilteredScannedHosts.Remove(hostModel);
        }

        foreach (var hostModel in hostsToShow)
        {
            FilteredScannedHosts.Add(hostModel);
        }

        // FilteredScannedHosts = new ObservableCollection<ScannedHostModel>(ScannedHosts.Where(x =>
        //     x.Hostname.ToString().ToLower().Contains(value.ToLower()) ||
        //     x.IpAddress.ToString().ToLower().Contains(value.ToLower()) ||
        //     x.MacAddress.ToString().ToLower().Contains(value.ToLower()) ||
        //     x.Vendor.ToString().ToLower().Contains(value.ToLower())).ToList());
    }

    public void Reset()
    {
        _stopwatch.Stop();
        _stopwatch.Reset();
        IsScanning = false;
        IsStopped = true;
        ProgressText = "Scanned: 0/0";
        ScannedHosts.Clear();
        FilteredScannedHosts.Clear();
        Progress = 0;
        Goal = 0;
        Elapsed = "00:00:00";
    }

    [RelayCommand]
    public void Stop()
    {
        _stopwatch.Stop();
        _cancellationTokenSource?.Cancel();
    }

    [RelayCommand]
    public async Task StartScan()
    {
        Reset();
        IsScanning = true;
        IsStopped = false;
        _timer.Interval = TimeSpan.FromMilliseconds(100);
        _timer.Tick += Timer_Tick;
        _stopwatch.Start();
        _timer.Start();
        _cancellationTokenSource = new CancellationTokenSource();
        var range = RangeInput.Split('-');
        var scanRange = RangeFinder.GetAddressRange(range[0], range[1]);
        await Dispatcher.UIThread.InvokeAsync(() =>
            Goal = RangeFinder.GetNumberOfAddressesInRange(range[0], range[1]));
        var cancellationToken = _cancellationTokenSource.Token;
        cancellationToken.ThrowIfCancellationRequested();
        var semaphore = new SemaphoreSlim(1000);
        try
        {
            var tasks = new List<Task>();

            foreach (var host in scanRange)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(cancellationToken); // Throttle concurrency

                var task = Task.Run(async () =>
                {
                    try
                    {
                        await ScanHost(host, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // tasks.RemoveAll(_ => true);
                        // tasks.Clear();
                    }
                    finally
                    {
                        semaphore.Release(); // Release on task completion
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            await Task.WhenAll(tasks); // Wait for all tasks to complete
        }
        catch (OperationCanceledException)
        {
            IsScanning = false;
            _stopwatch.Stop();
            _timer.Stop();
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                await _cancellationTokenSource.CancelAsync();
                Progress = Goal;
            }

            IsScanning = false;
            IsStopped = true;
            _stopwatch.Stop();
            _timer.Stop();
        }
    }

    private async Task ScanHost(IPAddress address, CancellationToken token)
    {
        // token = CancellationToken.None;
        token.ThrowIfCancellationRequested();
        try
        {
            var result = await TryScan(address, 1000, token);
            if (result.Item1)
            {
                await AddHostAsync(address, result.Item2, token);
            }
            else
            {
                result = await TryScan(address, 8000, token);
                if (result.Item1) await AddHostAsync(address, result.Item2, token);
            }
        }
        catch (OperationCanceledException)
        {
            // Debug.WriteLine($"Scanning host: {address} operation cancelled.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error scanning {address}: {ex}");
            throw;
        }
        finally
        {
            // Always update the progress if the operation was not canceled.
            if (!token.IsCancellationRequested)
                _uiUpdateQueue.Enqueue(() =>
                {
                    Progress++;
                    ProgressText = $"Scanned: {Progress}/{Goal}";
                });
        }
    }


    private async Task<(bool, string)> TryScan(IPAddress address, int timeout, CancellationToken token)
    {
        var tcpScanTask = Tcp.ScanHostAsync(address, timeout, token);
        var pingScanTask = PingEx.ScanHostAsync(address, timeout, token);
        var scanTask = await Task.WhenAny(tcpScanTask, pingScanTask);
        var arpTask = Arp.ArpScanAsync(address, timeout, token);
        if (scanTask == tcpScanTask && scanTask.Result) return (true, "TCP");
        if (scanTask == pingScanTask && scanTask.Result) return (true, "Ping");
        var tryArp = Arp.GetArpCache().Select(entry => entry.IpAddress).Contains(address.ToString());
        return tryArp ? (await arpTask, "ARP") : (false, "null");
    }


    private void Timer_Tick(object? sender, EventArgs e)
    {
        Elapsed = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
    }


    private async Task AddHostAsync(IPAddress hostAddress, string scanMethod, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        try
        {
            var name = await NameResolution.ResolveHostnameAsync(hostAddress, token);
            var macAddress = Arp.GetArpEntry(hostAddress)?.MacAddress ?? "Unknown";
            if (macAddress == "Unknown") // Look to see if the host is the current machine.
                foreach (var netInf in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var interfaceAddress = netInf.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
                    // Only proceed if the host address matches that of an address assigned to the local machine.
                    if (!Equals(interfaceAddress, hostAddress)) continue;
                    var rawMacAddress = netInf.GetPhysicalAddress().ToString();
                    var formattedMacAddress = new StringBuilder(rawMacAddress);
                    for (var i = 2; i < formattedMacAddress.Length; i += 3) formattedMacAddress.Insert(i, ":");
                    macAddress = formattedMacAddress.ToString();
                    name = Dns.GetHostName();
                }

            var vendor = _vendorLookup.GetVendorName(macAddress);
            token.ThrowIfCancellationRequested();
            if (macAddress == "Unknown")
            {
                await Task.Delay(100, token);
                macAddress = Arp.GetArpEntry(hostAddress)?.MacAddress ?? "Unknown";
            }

            var scannedHost = new ScannedHostModel()
            {
                IpAddress = hostAddress.ToString(),
                MacAddress = macAddress,
                Vendor = vendor,
                Hostname = name,
                ScanMethod = scanMethod
            };

            _uiUpdateQueue.Enqueue(() =>
            {
                ScannedHosts.Add(scannedHost);
                OnScanFilterTextChanged(ScanFilterText);
            });
        }
        catch (OperationCanceledException)
        {
            // Debug.WriteLine($"Adding host: {hostAddress} operation cancelled.");
        }
    }

    [RelayCommand]
    public void ScanLocalCommand()
    {
        var bestInterface = Route.GetBestInterface();
        var localIp = bestInterface.GetIPProperties().UnicastAddresses
            .First(ip => ip.Address.AddressFamily.Equals(AddressFamily.InterNetwork)).Address;
        var localSubnet = bestInterface.GetIPProperties().UnicastAddresses.First(ip => ip.Address.Equals(localIp))
            .IPv4Mask;
        var netInf = new NetInfo(localIp, localSubnet);
        var startAddress = IpMath.BitsToIp(IpMath.IpToBits(netInf.NetworkAddress) + 1);
        var endAddress = IpMath.BitsToIp(IpMath.IpToBits(netInf.BroadcastAddress) - 1);
        RangeInput = $"{startAddress}-{endAddress}";
        // var netInfo = new NetInfo(localIp, localSubnet);
        // _localNetwork = netInfo.GetAddressRangeFromNetwork().ToList();
    }

    public string Label { get; } = "Scan Tool";

    public string IconData { get; } =
        "M19.778,4.222A11,11,0,1,1,12,1a1,1,0,0,1,1,1v8.277a2,2,0,1,1-2,0V7.621a4.49,4.49,0,1,0,4.182,1.2A1,1,0,0,1,16.6,7.4,6.505,6.505,0,1,1,11,5.585V3.055a9,9,0,1,0,7.364,2.581,1,1,0,1,1,1.414-1.414Z";
}