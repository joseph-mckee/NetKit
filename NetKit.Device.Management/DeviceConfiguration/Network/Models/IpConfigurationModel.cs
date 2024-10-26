namespace NetKit.Device.Management.DeviceConfiguration.Network.Models;

public class IpConfigurationModel
{
    public string[] IpAddresses { get; init; } = [];
    public string[] SubnetMasks { get; init; } = [];
    public string[] Gateways { get; init; } = [];
    public int[] GatewayMetrics { get; init; } = [];
    public string[] DnsServers { get; init; } = [];
}