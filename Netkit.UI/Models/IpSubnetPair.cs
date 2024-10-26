using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NetKit.UI.Models;

public partial class IpSubnetPair : ObservableObject
{
    [ObservableProperty] private BindableString _ipAddress = new(string.Empty);
    [ObservableProperty] private BindableString _subnetMask = new(string.Empty);

    public IpSubnetPair(string ip, string subnet)
    {
        IpAddress = new BindableString(ip);
        SubnetMask = new BindableString(subnet);
    }

    public IpSubnetPair() : this(string.Empty, string.Empty)
    {
    }
}