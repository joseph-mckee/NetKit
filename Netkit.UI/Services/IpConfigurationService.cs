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

public class IpConfigurationService
{
    private readonly List<NetworkAdapterConfiguration> _networkAdapterConfigurations = [];

    public IpConfigurationService()
    {
        _networkAdapterConfigurations.Add(new NetworkAdapterConfiguration(12));
        var a = NetworkAdapterConfiguration.GetAll();
    }

    public void ApplyProfile(ViewModels_IpConfigurationProfileViewModel profileViewModel)
    {
        const int interfaceIndex = 12;
        var configuration = _networkAdapterConfigurations.FirstOrDefault(x => x.InterfaceIndex == interfaceIndex);
        if (configuration is null)
        {
            configuration = new NetworkAdapterConfiguration(interfaceIndex);
            _networkAdapterConfigurations.Add(configuration);
        }

        var ipConfig = ConvertProfileToModel(profileViewModel);

        configuration.EnableStatic(ipConfig.IpAddresses, ipConfig.SubnetMasks);
        configuration.SetGateways(ipConfig.Gateways, ipConfig.GatewayMetrics);
        configuration.SetDnsServerSearchOrder(ipConfig.DnsServers);
    }

    public void SetDhcp()
    {
        const int interfaceIndex = 12;
        var configuration = _networkAdapterConfigurations.FirstOrDefault(x => x.InterfaceIndex == interfaceIndex);
        if (configuration is null)
        {
            configuration = new NetworkAdapterConfiguration(interfaceIndex);
            _networkAdapterConfigurations.Add(configuration);
        }

        configuration.EnableDhcp();
        configuration.SetDnsServerSearchOrder();
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