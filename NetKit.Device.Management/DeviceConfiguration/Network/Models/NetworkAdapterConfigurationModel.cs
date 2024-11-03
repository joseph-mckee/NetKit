using System.Diagnostics.CodeAnalysis;
using System.Management;

namespace NetKit.Device.Management.DeviceConfiguration.Network.Models;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class NetworkAdapterConfigurationModel(ManagementObject configuration)
{
    // public string DhcpLeaseExpires { get; set; } = configuration["DHCPLeaseExpires"] as string ?? string.Empty;
    // public uint Index { get; set; } = (uint)configuration["Index"];
    // public string Description { get; set; } = configuration["Description"] as string ?? string.Empty;
    // public bool DhcpEnabled { get; set; } = (bool)configuration["DHCPEnabled"];
    // public string DhcpLeaseObtained { get; set; } = configuration["DHCPLeaseObtained"] as string ?? string.Empty;
    // public string DhcpServer { get; set; } = configuration["DHCPServer"] as string ?? string.Empty;
    // public string DnsDomain { get; set; } = configuration["DNSDomain"] as string ?? string.Empty;
    // public string[] DnsDomainSuffixSearchOrder { get; set; } = configuration["DNSDomainSuffixSearchOrder"] as string[] ?? [];
    // public bool DnsEnabledForWinsResolution { get; set; } = (bool)configuration["DNSEnabledForWINSResolution"];
    // public string DnsHostName { get; set; } = configuration["DNSHostName"] as string ?? string.Empty;
    // public string[] DnsServerSearchOrder { get; set; } = configuration["DNSServerSearchOrder"] as string[] ?? [];
    // public bool DomainDnsRegistrationEnabled { get; set; } = (bool)configuration["DomainDNSRegistrationEnabled"];
    // public bool FullDnsRegistrationEnabled { get; set; }
    // public List<string>? IpAddress { get; set; }
    // public uint IpConnectionMetric { get; set; }
    public bool IpEnabled { get; set; } = (bool)configuration["IPEnabled"];
    // public bool IpFilterSecurityEnabled { get; set; }
    // public bool WinsEnableLmHostsLookup { get; set; }
    // public string? WinsHostLookupFile { get; set; }
    // public string? WinsPrimaryServer { get; set; }
    // public string? WinsScopeId { get; set; }
    // public string? WinsSecondaryServer { get; set; }
    // public string Caption { get; set; } = configuration["Caption"] as string ?? string.Empty;
    // public string SettingId { get; set; } = configuration["SettingID"] as string ?? string.Empty;
    // public bool ArpAlwaysSourceRoute { get; set; } = (bool)configuration["ArpAlwaysSourceRoute"];
    // public bool ArpUseEtherSnap { get; set; } = (bool)configuration["ArpUseEtherSNAP"];
    // public string DatabasePath { get; set; } = configuration["DatabasePath"] as string ?? string.Empty;
    // public bool DeadGwDetectEnabled { get; set; } = (bool)configuration["DeadGWDetectEnabled"];
    // public string[] DefaultIpGateway { get; set; } = configuration["DefaultIPGateway"] as string[] ?? [];
    // public byte DefaultTos { get; set; } = (byte)configuration["DefaultTOS"];
    // public byte DefaultTtl { get; set; } = (byte)configuration["DefaultTTL"];
    // public uint ForwardBufferMemory { get; set; }
    // public List<uint>? GatewayCostMetric { get; set; }
    // public uint IgmpLevel { get; set; }
    // public uint InterfaceIndex { get; set; }
    // public bool IpPortSecurityEnabled { get; set; }
    // public List<uint>? IpSecPermitIpProtocols { get; set; }
    // public List<uint>? IpSecPermitTcpPorts { get; set; }
    // public List<uint>? IpSecPermitUdpPorts { get; set; }
    // public List<string>? IpSubnet { get; set; }
    // public bool IpUseZeroBroadcast { get; set; }
    // public string? IpxAddress { get; set; }
    // public bool IpxEnabled { get; set; }
    // public uint IpxFrameType { get; set; }
    // public uint IpxMediaType { get; set; }
    // public string? IpxNetworkNumber { get; set; }
    // public string? IpxVirtualNetNumber { get; set; }
    // public uint KeepAliveInterval { get; set; }
    // public uint KeepAliveTime { get; set; }
    // public string? MacAddress { get; set; }
    // public uint Mtu { get; set; }
    // public uint NumForwardPackets { get; set; }
    // public bool PMtuBhDetectEnabled { get; set; }
    // public bool PMtuDiscoveryEnabled { get; set; }
    // public string? ServiceName { get; set; }
    // public uint TcpIpNetbiosOptions { get; set; }
    // public uint TcpMaxConnectRetransmissions { get; set; }
    // public uint TcpMaxDataRetransmissions { get; set; }
    // public uint TcpNumConnections { get; set; }
    // public bool TcpUseRfc1122UrgentPointer { get; set; }
    // public uint TcpWindowSize { get; set; }
    // public string? PsComputerName { get; set; }
    // public string? CimClass { get; set; }
    // public List<string>? CimInstanceProperties { get; set; }
    // public string? CimSystemProperties { get; set; }
}