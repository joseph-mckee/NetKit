using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetKit.Observer.Utilities;

public static class LocalNetwork
{
    public static Dictionary<IPAddress, IPAddress> GetLocalIpv4Networks()
    {
        Dictionary<IPAddress, IPAddress> networks = new();

        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces().Where(networkInterface =>
            networkInterface.GetIPProperties().UnicastAddresses.FirstOrDefault(address =>
                address.Address.AddressFamily == AddressFamily.InterNetwork) != null &&
            networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
            networkInterface.OperationalStatus == OperationalStatus.Up);

        foreach (var networkInterface in networkInterfaces)
        {
            var address = networkInterface.GetIPProperties().UnicastAddresses.First(address =>
                address.Address.AddressFamily == AddressFamily.InterNetwork);
            networks[address.Address] = address.IPv4Mask;
        }

        return networks;
    }
}