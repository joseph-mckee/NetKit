using System.Management;
using NetKit.Device.Management.DeviceConfiguration.Network.Enums;
using NetKit.Device.Management.DeviceConfiguration.Network.Models;

namespace NetKit.Device.Management.DeviceConfiguration.Network;

public static class DnsClientCache
{
    private static ManagementClass GetDnsClass()
    {
        var scope = new ManagementScope("\\Root\\StandardCimv2");
        var path = new ManagementPath("MSFT_DNSClientCache");
        var options = new ObjectGetOptions();
        return new ManagementClass(scope, path, options);
    }

    public static IEnumerable<DnsRecord> GetRecords()
    {
        var entries = GetDnsClass().GetInstances();
        foreach (var entry in entries)
        {
            yield return new DnsRecord
            {
                InstanceId = entry.Properties["InstanceId"]?.Value?.ToString() ?? string.Empty,
                Caption = entry.Properties["Caption"]?.Value?.ToString() ?? string.Empty,
                Description = entry.Properties["Description"]?.Value?.ToString() ?? string.Empty,
                ElementName = entry.Properties["ElementName"]?.Value?.ToString() ?? string.Empty,
                Entry = entry.Properties["Entry"]?.Value?.ToString() ?? string.Empty,
                Name = entry.Properties["Name"]?.Value?.ToString() ?? string.Empty,
                Type = (DnsType)int.Parse(entry.Properties["Type"].Value.ToString() ?? string.Empty),
                TimeToLive = entry.Properties["TimeToLive"]?.Value?.ToString() ?? string.Empty,
                DataLength = entry.Properties["DataLength"]?.Value?.ToString() ?? string.Empty,
                Section = (DnsSection)int.Parse(entry.Properties["Section"].Value.ToString() ?? string.Empty),
                Data = entry.Properties["Data"]?.Value?.ToString() ?? string.Empty,
                Status = (DnsStatus)int.Parse(entry.Properties["Status"].Value.ToString() ?? string.Empty)
            };
        }
    }

    public static void Clear()
    {
        var dnsClass = GetDnsClass();
        var parameters = dnsClass.GetMethodParameters("Clear");
        dnsClass.InvokeMethod("Clear", parameters, null!);
    }
}