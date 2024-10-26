using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using NetKit.Observer.Utilities;

namespace NetKit.Observer.Scanners;

public static class ArpScanner
{
    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    private static extern int SendARP(uint destIp, uint srcIp, [Out] byte[] pMacAddr, ref uint phyAddrLen);


    public static async Task<bool> ScanAsync(IPAddress ipAddress, CancellationToken token)
    {
        try
        {
            var macAddr = new byte[6];
            var macAddrLen = (uint)macAddr.Length;
            var dest = IpMath.IpToBits(ipAddress, false);
            const uint source = 0;

            token.ThrowIfCancellationRequested();
            // Running the ARP request on a separate task
            var result = await Task.Run(() => SendARP(dest, source, macAddr, ref macAddrLen), token);
            return result == 0;
        }
        catch (Exception)
        {
            // Return false or handle as needed for an error condition.
            return false;
        }
    }
}