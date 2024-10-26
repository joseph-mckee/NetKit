using NetKit.Device.Management.DeviceConfiguration.Network.Enums;

namespace NetKit.Device.Management.DeviceConfiguration.Network.Models;

public class DnsRecord
{
    public string? InstanceId { get; set; }
    public string? Caption { get; set; }
    public string? Description { get; set; }
    public string? ElementName { get; set; }
    public string? Entry { get; set; }
    public string? Name { get; set; }
    public DnsType Type { get; set; }
    public string? TimeToLive { get; set; }
    public string? DataLength { get; set; }
    public DnsSection Section { get; set; }
    public string? Data { get; set; }
    public DnsStatus Status { get; set; }
}