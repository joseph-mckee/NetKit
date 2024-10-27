using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrypticWizard.RandomWordGenerator;
using DynamicData;
using NetKit.Device.Management.DeviceConfiguration.Network;
using NetKit.UI.Models;
using NetKit.UI.Services;
using NetKit.UI.ViewModels.PageViewModels;

namespace NetKit.UI.ViewModels;

public partial class IpConfigurationProfileViewModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isIpRemovable = true;
    [ObservableProperty] private bool _isGatewayRemovable = true;
    [ObservableProperty] private bool _isDnsServerRemovable = true;
    [ObservableProperty] private bool _isEditing;

    [ObservableProperty] private ObservableCollection<IpSubnetPair> _ipSubnetPairs = [new IpSubnetPair()];

    [ObservableProperty]
    private ObservableCollection<GatewayMetricPair> _gatewayMetricPairs = [new GatewayMetricPair()];

    [ObservableProperty] private ObservableCollection<BindableString> _dnsServers = [new BindableString()];

    [ObservableProperty] private IpConfigurationPageViewModel _parentContext;
    
    public IpConfigurationProfileViewModel() : this(new IpConfigurationPageViewModel())
    {
    }

    public IpConfigurationProfileViewModel(IpConfigurationPageViewModel parentContext)
    {
        _parentContext = parentContext;
        _name = GenerateName();
        ModerateButtons();
    }

    private static string GenerateName()
    {
        var wordGenerator = new WordGenerator();
        var firstWord = GetCapitalized(wordGenerator.GetWord());
        var secondWord = GetCapitalized(wordGenerator.GetWord());
        return $"{firstWord} {secondWord}";
    }

    private static string GetCapitalized(string word)
    {
        return word[0].ToString().ToUpper() + word[1..];
    }

    private void ModerateButtons()
    {
        IsIpRemovable = IpSubnetPairs.Count > 1;
        IsGatewayRemovable = GatewayMetricPairs.Count > 1;
        IsDnsServerRemovable = DnsServers.Count > 1;
    }

    private void DisableEditingInterface()
    {
        IsIpRemovable = false;
        IsGatewayRemovable = false;
        IsDnsServerRemovable = false;
    }

    private void EnableEditingInterface()
    {
        IsIpRemovable = true;
        IsGatewayRemovable = true;
        IsDnsServerRemovable = true;
        ModerateButtons();
    }

    private void RemoveEmptyFields()
    {
        var emptyIps = IpSubnetPairs.Where(x =>
            string.IsNullOrEmpty(x.IpAddress.Value) || string.IsNullOrEmpty(x.SubnetMask.Value)).ToList();
        IpSubnetPairs.Remove(emptyIps);
        var emptyGateways = GatewayMetricPairs.Where(x =>
            string.IsNullOrEmpty(x.GatewayAddress.Value) || string.IsNullOrEmpty(x.GatewayMetric.Value)).ToList();
        GatewayMetricPairs.Remove(emptyGateways);
        var emptyDnsServers = DnsServers.Where(x => string.IsNullOrEmpty(x.Value)).ToList();
        DnsServers.Remove(emptyDnsServers);
    }

    [RelayCommand]
    public void EditCommand()
    {
        IsEditing = true;
        EnableEditingInterface();
    }

    [RelayCommand]
    public void SaveCommand()
    {
        IsEditing = false;
        RemoveEmptyFields();
        DisableEditingInterface();
    }

    [RelayCommand]
    public void AddIpAddressCommand()
    {
        IpSubnetPairs.Add(new IpSubnetPair());
        ModerateButtons();
    }

    [RelayCommand]
    public void RemoveIpSubnetPairCommand(IpSubnetPair pair)
    {
        IpSubnetPairs.Remove(pair);
        ModerateButtons();
    }

    [RelayCommand]
    public void AddGatewayCommand()
    {
        GatewayMetricPairs.Add(new GatewayMetricPair());
        ModerateButtons();
    }

    [RelayCommand]
    public void RemoveGatewayCommand(GatewayMetricPair pair)
    {
        GatewayMetricPairs.Remove(pair);
        ModerateButtons();
    }

    [RelayCommand]
    public void AddDnsServerCommand()
    {
        DnsServers.Add(new BindableString());
        ModerateButtons();
    }

    [RelayCommand]
    public void RemoveDnsServerCommand(BindableString server)
    {
        DnsServers.Remove(server);
        ModerateButtons();
    }
}