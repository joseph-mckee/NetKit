using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DynamicData.Kernel;
using NetKit.Device.Management.DeviceConfiguration.Network;
using NetKit.Device.Management.DeviceConfiguration.Network.Models;
using NetKit.UI.Models;
using IpConfigurationProfileViewModel = NetKit.UI.ViewModels.IpConfigurationProfileViewModel;
using ViewModels_IpConfigurationProfileViewModel = NetKit.UI.ViewModels.IpConfigurationProfileViewModel;

namespace NetKit.UI.Services;

public class IpConfigurationService(uint index)
{
    private readonly NetworkAdapterConfiguration _networkAdapterConfiguration = new(index);

    public void ApplyProfile(ViewModels_IpConfigurationProfileViewModel profileViewModel)
    {
        var ipConfig = ConvertProfileToModel(profileViewModel);

        _networkAdapterConfiguration.EnableStatic(ipConfig.IpAddresses, ipConfig.SubnetMasks);
        _networkAdapterConfiguration.SetGateways(ipConfig.Gateways, ipConfig.GatewayMetrics);
        _networkAdapterConfiguration.SetDnsServerSearchOrder(ipConfig.DnsServers);
    }

    public void SetDhcp()
    {
        _networkAdapterConfiguration.EnableDhcp();
        _networkAdapterConfiguration.SetDnsServerSearchOrder();
    }

    private static IpConfigurationModel ConvertProfileToModel(ViewModels_IpConfigurationProfileViewModel profileViewModel)
    {
        return new IpConfigurationModel
        {
            IpAddresses = profileViewModel.IpSubnetPairs.Select(x => x.IpAddress.Value).AsArray(),
            SubnetMasks = profileViewModel.IpSubnetPairs.Select(x => x.SubnetMask.Value).AsArray(),
            Gateways = profileViewModel.GatewayMetricPairs.Select(x => x.GatewayAddress.Value).AsArray(),
            GatewayMetrics = profileViewModel.GatewayMetricPairs.Select(x => int.Parse(x.GatewayMetric.Value)).AsArray(),
            DnsServers = profileViewModel.DnsServers.Select(x => x.Value).AsArray()
        };
    }
}