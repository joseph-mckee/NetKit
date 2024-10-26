﻿using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetKit.UI.Models;
using NetKit.UI.Services;

namespace NetKit.UI.ViewModels.PageViewModels;

public partial class IpConfigurationPageViewModel : ViewModelBase, IPageViewModel
{
    public string Label => "IP Config";

    public string IconData =>
        "M18.25,3 C19.7687831,3 21,4.23121694 21,5.75 L21,18.25 C21,19.7687831 19.7687831,21 18.25,21 L5.75,21 C4.23121694,21 3,19.7687831 3,18.25 L3,5.75 C3,4.23121694 4.23121694,3 5.75,3 L18.25,3 Z M18.25,4.5 L5.75,4.5 C5.05964406,4.5 4.5,5.05964406 4.5,5.75 L4.5,18.25 C4.5,18.9403559 5.05964406,19.5 5.75,19.5 L18.25,19.5 C18.9403559,19.5 19.5,18.9403559 19.5,18.25 L19.5,5.75 C19.5,5.05964406 18.9403559,4.5 18.25,4.5 Z M9.75189626,12.5 C10.7183946,12.5 11.5018963,13.2835017 11.5018963,14.25 L11.5018963,16.25 C11.5018963,17.2164983 10.7183946,18 9.75189626,18 L7.75189626,18 C6.78539794,18 6.00189626,17.2164983 6.00189626,16.25 L6.00189626,14.25 C6.00189626,13.2835017 6.78539794,12.5 7.75189626,12.5 L9.75189626,12.5 Z M16.2493679,12.5 C17.2158662,12.5 17.9993679,13.2835017 17.9993679,14.25 L17.9993679,16.25 C17.9993679,17.2164983 17.2158662,18 16.2493679,18 L14.2493679,18 C13.2828696,18 12.4993679,17.2164983 12.4993679,16.25 L12.4993679,14.25 C12.4993679,13.2835017 13.2828696,12.5 14.2493679,12.5 L16.2493679,12.5 Z M9.75189626,14 L7.75189626,14 C7.61382507,14 7.50189626,14.1119288 7.50189626,14.25 L7.50189626,16.25 C7.50189626,16.3880712 7.61382507,16.5 7.75189626,16.5 L9.75189626,16.5 C9.88996744,16.5 10.0018963,16.3880712 10.0018963,16.25 L10.0018963,14.25 C10.0018963,14.1119288 9.88996744,14 9.75189626,14 Z M16.2493679,14 L14.2493679,14 C14.1112967,14 13.9993679,14.1119288 13.9993679,14.25 L13.9993679,16.25 C13.9993679,16.3880712 14.1112967,16.5 14.2493679,16.5 L16.2493679,16.5 C16.3874391,16.5 16.4993679,16.3880712 16.4993679,16.25 L16.4993679,14.25 C16.4993679,14.1119288 16.3874391,14 16.2493679,14 Z M9.75063209,6 C10.7171304,6 11.5006321,6.78350169 11.5006321,7.75 L11.5006321,9.75 C11.5006321,10.7164983 10.7171304,11.5 9.75063209,11.5 L7.75063209,11.5 C6.78413377,11.5 6.00063209,10.7164983 6.00063209,9.75 L6.00063209,7.75 C6.00063209,6.78350169 6.78413377,6 7.75063209,6 L9.75063209,6 Z M16.2481037,6 C17.2146021,6 17.9981037,6.78350169 17.9981037,7.75 L17.9981037,9.75 C17.9981037,10.7164983 17.2146021,11.5 16.2481037,11.5 L14.2481037,11.5 C13.2816054,11.5 12.4981037,10.7164983 12.4981037,9.75 L12.4981037,7.75 C12.4981037,6.78350169 13.2816054,6 14.2481037,6 L16.2481037,6 Z M9.75063209,7.5 L7.75063209,7.5 C7.6125609,7.5 7.50063209,7.61192881 7.50063209,7.75 L7.50063209,9.75 C7.50063209,9.88807119 7.6125609,10 7.75063209,10 L9.75063209,10 C9.88870327,10 10.0006321,9.88807119 10.0006321,9.75 L10.0006321,7.75 C10.0006321,7.61192881 9.88870327,7.5 9.75063209,7.5 Z M16.2481037,7.5 L14.2481037,7.5 C14.1100326,7.5 13.9981037,7.61192881 13.9981037,7.75 L13.9981037,9.75 C13.9981037,9.88807119 14.1100326,10 14.2481037,10 L16.2481037,10 C16.3861749,10 16.4981037,9.88807119 16.4981037,9.75 L16.4981037,7.75 C16.4981037,7.61192881 16.3861749,7.5 16.2481037,7.5 Z";

    [ObservableProperty] private ObservableCollection<IpConfigurationProfileViewModel> _ipConfigurationProfiles =
    [
    ];

    [ObservableProperty] private ObservableCollection<InterfaceConfigurationViewModel> _interfaceConfigurations = [];

    private readonly IpConfigurationService _ipConfigurationService = new();

    [ObservableProperty] private IpConfigViewModel _ipConfigViewModel;

    public IpConfigurationPageViewModel() : this(new IpConfigViewModel())
    {
        
    }
    
    public IpConfigurationPageViewModel(IpConfigViewModel ipConfigViewModel)
    {
        _ipConfigViewModel = ipConfigViewModel;
        IpConfigurationProfiles =
        [
            new IpConfigurationProfileViewModel(this, _ipConfigurationService)
            {
                IpSubnetPairs = [new IpSubnetPair("172.16.12.100", "255.255.255.0")],
                GatewayMetricPairs = [new GatewayMetricPair("172.16.12.1", "1")],
                DnsServers = [new BindableString("172.16.12.1")],
                Name = "Default Profile"
            },
            new IpConfigurationProfileViewModel(this, _ipConfigurationService)
            {
                IpSubnetPairs = [new IpSubnetPair("172.16.12.200", "255.255.255.0")],
                GatewayMetricPairs = [new GatewayMetricPair("172.16.12.1", "1")],
                DnsServers = [new BindableString("172.16.12.1")],
                Name = "Other Profile"
            }
        ];
        InterfaceConfigurations = 
        [
            new InterfaceConfigurationViewModel(this, _ipConfigurationService)
        ];
    }

    [RelayCommand]
    public void ReloadInterfaces()
    {
        
    }

    [RelayCommand]
    public void ApplyDhcpCommand()
    {
        var ipConfigurationService = new IpConfigurationService();
        ipConfigurationService.SetDhcp();
    }

    [RelayCommand]
    public void AddProfileCommand()
    {
        IpConfigurationProfiles.Add(new IpConfigurationProfileViewModel(this, _ipConfigurationService));
    }

    [RelayCommand]
    public void RemoveProfileCommand(IpConfigurationProfileViewModel profileViewModel)
    {
        IpConfigurationProfiles.Remove(profileViewModel);
    }
}