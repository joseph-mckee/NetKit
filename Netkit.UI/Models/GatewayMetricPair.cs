using CommunityToolkit.Mvvm.ComponentModel;
using NetKit.UI.ViewModels;

namespace NetKit.UI.Models;

public partial class GatewayMetricPair : ObservableObject
{
    [ObservableProperty] private BindableString _gatewayAddress = new(string.Empty);
    [ObservableProperty] private BindableString _gatewayMetric = new(string.Empty);

    public GatewayMetricPair(string address, string metric)
    {
        GatewayAddress = new BindableString(address);
        GatewayMetric = new BindableString(metric);
    }

    public GatewayMetricPair() : this(string.Empty, string.Empty)
    {
    }
}