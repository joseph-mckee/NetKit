using System.Diagnostics.CodeAnalysis;
using System.Management;

namespace NetKit.Device.Management.DeviceConfiguration.Network;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class NetworkAdapterConfiguration(uint interfaceIndex) : IDisposable
{
    private readonly ManagementObject _interfaceObject = LoadInterface(interfaceIndex)
                                                         ?? throw new ArgumentException(
                                                             $"No network adapter found with InterfaceIndex {interfaceIndex}.");

    private bool _disposed;
    public uint InterfaceIndex { get; } = interfaceIndex;

    private static ManagementObject? LoadInterface(uint interfaceIndex)
    {
        using var managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
        var managementObjectCollection = managementClass.GetInstances();

        foreach (var o in managementObjectCollection)
        {
            var managementObject = (ManagementObject)o;
            try
            {
                if (managementObject["InterfaceIndex"] is uint currentInterfaceIndex &&
                    currentInterfaceIndex == interfaceIndex)
                {
                    return managementObject;
                }
            }
            finally
            {
                // Dispose of the management object if it's not the one we're returning
                managementObject.Dispose();
            }
        }

        return null;
    }

    public static List<NetworkAdapterConfiguration> GetAll()
    {
        var configurations = new List<NetworkAdapterConfiguration>();
        using var managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
        using var managementObjectCollection = managementClass.GetInstances();

        foreach (var o in managementObjectCollection)
        {
            var managementObject = (ManagementObject)o;
            configurations.Add(new NetworkAdapterConfiguration((uint)managementObject["InterfaceIndex"]));
        }

        return configurations;
    }

    public void DisableIpSec()
    {
        InvokeMethod("DisableIPSec");
    }

    public void EnableDhcp()
    {
        InvokeMethod("EnableDHCP");
    }

    // Not working on Home or Pro versions of Windows. May only be for Windows Server.
    public void EnableDns(string? dnsHostname = null, string? dnsDomain = null, string[]? dnsServerSearchOrder = null,
        string[]? dnsDomainSuffixSearchOrder = null)
    {
        const string methodName = "EnableDNS";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        if (dnsHostname != null) parameters["DNSHostName"] = dnsHostname;
        if (dnsDomain != null) parameters["DNSDomain"] = dnsDomain;
        if (dnsServerSearchOrder != null) parameters["DNSServerSearchOrder"] = dnsServerSearchOrder;
        if (dnsDomainSuffixSearchOrder != null) parameters["DNSDomainSuffixSearchOrder"] = dnsDomainSuffixSearchOrder;

        InvokeMethod(methodName, parameters);
    }

    public void EnableIpFilterSec(bool enabled)
    {
        const string methodName = "EnableIPFilterSec";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["IPFilterSecurityEnabled"] = enabled;

        InvokeMethod(methodName, parameters);
    }

    public void EnableIpSec(string[] ipSecPermitTcpPorts, string[] ipSecPermitUdpPorts, string[] ipSecPermitIpProtocols)
    {
        const string methodName = "EnableIPSec";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["IPSecPermitTCPPorts"] = ipSecPermitTcpPorts;
        parameters["IPSecPermitUDPPorts"] = ipSecPermitUdpPorts;
        parameters["IPSecPermitIPPProtocols"] = ipSecPermitIpProtocols;

        InvokeMethod(methodName, parameters);
    }

    public void EnableStatic(string[] ipAddresses, string[] subnetMasks)
    {
        const string methodName = "EnableStatic";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["IPAddress"] = ipAddresses;
        parameters["SubnetMask"] = subnetMasks;

        InvokeMethod(methodName, parameters);
    }

    // Basically deprecated.
    public void EnableWins(bool dnsEnabledForWinsResolution, bool winsEnableLmHostsLookup,
        string? winsHostLookupFile = null, string? winsScopeId = null)
    {
        const string methodName = "EnableWINS";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["DNSEnabledForWINSResolution"] = dnsEnabledForWinsResolution;
        parameters["WINSEnableLMHostsLookup"] = winsEnableLmHostsLookup;
        if (winsHostLookupFile is not null) parameters["WINSHostLookupFile"] = winsHostLookupFile;
        if (winsScopeId is not null) parameters["WINSScopeID"] = winsScopeId;

        InvokeMethod(methodName, parameters);
    }

    public void ReleaseDhcpLease()
    {
        InvokeMethod("ReleaseDHCPLease");
    }

    public void ReleaseDhcpLeaseAll()
    {
        InvokeMethod("ReleaseDHCPLeaseAll");
    }

    public void RenewDhcpLease()
    {
        InvokeMethod("RenewDHCPLease");
    }

    public void RenewDhcpLeaseAll()
    {
        InvokeMethod("RenewDHCPLeaseAll");
    }

    public void SetArpAlwaysSourceRoute(bool arpAlwaysSourceRoute)
    {
        const string methodName = "SetArpAlwaysSourceRoute";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["ArpAlwaysSourceRoute"] = arpAlwaysSourceRoute;

        InvokeMethod(methodName, parameters);
    }

    public void SetArpUseEtherSnap(bool arpUseEtherSnap)
    {
        const string methodName = "SetArpUseEtherSNAP";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["ArpUseEtherSNAP"] = arpUseEtherSnap;

        InvokeMethod(methodName, parameters);
    }

    public void SetDatabasePath(string databasePath)
    {
        const string methodName = "SetDatabasePath";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["DatabasePath"] = databasePath;

        InvokeMethod(methodName, parameters);
    }

    public void SetDeadGwDetect(bool deadGwDetectEnabled)
    {
        const string methodName = "SetDeadGWDetect";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["DeadGWDetectEnabled"] = deadGwDetectEnabled;

        InvokeMethod(methodName, parameters);
    }

    public void SetDefaultTos(sbyte defaultTos)
    {
        const string methodName = "SetDefaultTOS";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["DefaultTOS"] = defaultTos;

        InvokeMethod(methodName, parameters);
    }

    public void SetDefaultTtl(sbyte defaultTtl)
    {
        const string methodName = "SetDefaultTTL";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["DefaultTTL"] = defaultTtl;

        InvokeMethod(methodName, parameters);
    }

    public void SetDnsDomain(string dnsDomain)
    {
        const string methodName = "SetDNSDomain";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["DNSDomain"] = dnsDomain;

        InvokeMethod(methodName, parameters);
    }

    public void SetDnsServerSearchOrder(string[]? dnsServers = null)
    {
        const string methodName = "SetDnsServerSearchOrder";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        if (dnsServers is not null) parameters["DNSServerSearchOrder"] = dnsServers;

        InvokeMethod(methodName, parameters);
    }

    public void SetDnsSuffixSearchOrder(string[] dnsDomainSuffixSearchOrder)
    {
        const string methodName = "SetDNSSuffixSearchOrder";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["DNSDomainSuffixSearchOrder"] = dnsDomainSuffixSearchOrder;

        InvokeMethod(methodName, parameters);
    }

    public void SetDynamicDnsRegistration(bool fullDnsRegistrationEnabled, bool domainDnsRegistrationEnabled = false)
    {
        const string methodName = "SetDynamicDNSRegistration";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["FullDNSRegistrationEnabled"] = fullDnsRegistrationEnabled;
        parameters["DomainDNSRegistrationEnabled"] = domainDnsRegistrationEnabled;

        InvokeMethod(methodName, parameters);
    }

    public void SetForwardBufferMemory(uint forwardBufferMemory)
    {
        const string methodName = "SetForwardBufferMemory";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["ForwardBufferMemory"] = forwardBufferMemory;

        InvokeMethod(methodName, parameters);
    }

    public void SetGateways(string[] gateways, int[]? metric = null)
    {
        const string methodName = "SetGateways";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["DefaultIPGateway"] = gateways;
        if (metric != null) parameters["GatewayCostMetric"] = metric;

        InvokeMethod(methodName, parameters);
    }

    public void SetIgmpLevel(sbyte igmpLevel)
    {
        const string methodName = "SetIGMPLevel";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["IGMPLevel"] = igmpLevel;

        InvokeMethod(methodName, parameters);
    }

    public void SetIpConnectionMetric(uint ipConnectionMetric)
    {
        const string methodName = "SetIPConnectionMetric";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["IPConnectionMetric"] = ipConnectionMetric;

        InvokeMethod(methodName, parameters);
    }

    public void SetIpUseZeroBroadcast(bool ipUseZeroBroadcast)
    {
        const string methodName = "SetIPUseZeroBroadcast";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["IPUseZeroBroadcast"] = ipUseZeroBroadcast;

        InvokeMethod(methodName, parameters);
    }

    public void SetIpxFrameTypeNetworkPairs(string[] ipxNetworkNumber, uint[] ipxFrameType)
    {
        const string methodName = "SetIPXFrameTypeNetworkPairs";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["IPXNetworkNumber"] = ipxNetworkNumber;
        parameters["IPXFrameType"] = ipxFrameType;

        InvokeMethod(methodName, parameters);
    }

    public void SetIpxVirtualNetworkNumber(uint ipxVirtualNetNumber)
    {
        const string methodName = "SetIPXVirtualNetworkNumber";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["IPXVirtualNetNumber"] = ipxVirtualNetNumber;

        InvokeMethod(methodName, parameters);
    }

    public void SetKeepAliveInterval(uint keepAliveInterval)
    {
        const string methodName = "SetKeepAliveInterval";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["KeepAliveInterval"] = keepAliveInterval;

        InvokeMethod(methodName, parameters);
    }

    public void SetKeepAliveTime(uint keepAliveTime)
    {
        const string methodName = "SetKeepAliveTime";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["KeepAliveTime"] = keepAliveTime;

        InvokeMethod(methodName, parameters);
    }

    public void SetMtu(uint mtu)
    {
        const string methodName = "SetMTU";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["MTU"] = mtu;

        InvokeMethod(methodName, parameters);
    }

    public void SetNumForwardPackets(uint numForwardPackets)
    {
        const string methodName = "SetNumForwardPackets";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["NumForwardPackets"] = numForwardPackets;

        InvokeMethod(methodName, parameters);
    }

    public void SetPmtubhDetect(bool pmtubhDetect)
    {
        const string methodName = "SetPMTUBHDetect";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["PMTUBHDetectEnabled"] = pmtubhDetect;

        InvokeMethod(methodName, parameters);
    }

    public void SetPmtuDiscovery(bool pmtuDiscovery)
    {
        const string methodName = "SetPMTUDiscovery";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["PMTUDiscoveryEnabled"] = pmtuDiscovery;

        InvokeMethod(methodName, parameters);
    }

    public void SetTcpIpNetbios(uint tcpipNetiosOptions)
    {
        const string methodName = "SetTcpipNetbios";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["TcpipNetbiosOptions"] = tcpipNetiosOptions;

        InvokeMethod(methodName, parameters);
    }

    public void SetTcpMaxConnectRetransmissions(uint maxConnectRetransmissions)
    {
        const string methodName = "SetTcpMaxConnectRetransmissions";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["TcpMaxConnectRetransmissions"] = maxConnectRetransmissions;

        InvokeMethod(methodName, parameters);
    }

    public void SetTcpMaxDataRetransmissions(uint maxDataRetransmissions)
    {
        const string methodName = "SetTcpMaxDataRetransmissions";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["TcpMaxDataRetransmissions"] = maxDataRetransmissions;

        InvokeMethod(methodName, parameters);
    }

    public void SetTcpNumConnections(uint numConnections)
    {
        const string methodName = "SetTcpNumConnections";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["TcpNumConnections"] = numConnections;

        InvokeMethod(methodName, parameters);
    }

    public void SetTcpUseRfc1122UrgentPointer(bool useRfc1122UrgentPointer)
    {
        const string methodName = "SetTcpUseRFC1122UrgentPointer";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["TcpUseRFC1122UrgentPointer"] = useRfc1122UrgentPointer;

        InvokeMethod(methodName, parameters);
    }

    public void SetTcpWindowSize(ushort tcpWindowSize)
    {
        const string methodName = "SetTcpWindowSize";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["TcpWindowSize"] = tcpWindowSize;

        InvokeMethod(methodName, parameters);
    }

    public void SetWinsServer(string primaryWinsServer, string secondaryWinsServer)
    {
        const string methodName = "SetWINSServer";

        var parameters = _interfaceObject.GetMethodParameters(methodName);
        parameters["WINSPrimaryServer"] = primaryWinsServer;
        parameters["WINSSecondaryServer"] = secondaryWinsServer;

        InvokeMethod(methodName, parameters);
    }

    private void InvokeMethod(string methodName, ManagementBaseObject? parameters = null)
    {
        EnsureNotDisposed();

        var result = _interfaceObject.InvokeMethod(methodName, parameters!, null!);
        ProcessResult(result);
    }

    private static void ProcessResult(ManagementBaseObject result)
    {
        if (result == null)
            throw new InvalidOperationException("No result returned from WMI method invocation.");

        var returnValue = (uint)result["ReturnValue"];
        if (returnValue != 0 && returnValue != 1)
        {
            throw new NetworkConfigurationException(returnValue);
        }
    }

    private void EnsureNotDisposed()
    {
        if (!_disposed) return;
        throw new ObjectDisposedException(nameof(NetworkAdapterConfiguration));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _interfaceObject.Dispose();
        _disposed = true;
    }
}

public class NetworkConfigurationException(uint errorCode) : Exception(GetErrorMessage(errorCode))
{
    private static string GetErrorMessage(uint errorCode)
    {
        return errorCode switch
        {
            64 => "Method not supported on this platform.",
            65 => "Unknown failure.",
            66 => "Invalid subnet mask.",
            67 => "An error occurred while processing an instance.",
            68 => "Invalid input parameter.",
            69 => "More than five gateways specified.",
            70 => "Invalid IP address.",
            71 => "Invalid gateway IP address.",
            72 => "An error occurred while accessing the registry.",
            73 => "Invalid domain name.",
            74 => "Invalid host name.",
            75 => "No primary or secondary WINS server defined.",
            76 => "Invalid file.",
            77 => "Invalid system path.",
            78 => "File copy failed.",
            79 => "Invalid security parameter.",
            80 => "Unable to configure TCP/IP service.",
            81 => "Unable to configure DHCP service.",
            82 => "Unable to renew DHCP lease.",
            83 => "Unable to release DHCP lease.",
            84 => "IP not enabled on adapter.",
            85 => "IPX not enabled on adapter.",
            86 => "Frame or network number bounds error.",
            87 => "Invalid frame type.",
            88 => "Invalid network number.",
            89 => "Duplicate network number.",
            90 => "Parameter out of bounds.",
            91 => "Access denied.",
            92 => "Out of memory.",
            93 => "Already exists.",
            94 => "Path, file, or object not found.",
            95 => "Unable to notify service.",
            96 => "Unable to notify DNS service.",
            97 => "Interface not configurable.",
            98 => "Not all DHCP leases could be released or renewed.",
            100 => "DHCP not enabled on the adapter.",
            101 => "System error.",
            _ => $"Unknown error invoking function. Error code: {errorCode}"
        };
    }
}