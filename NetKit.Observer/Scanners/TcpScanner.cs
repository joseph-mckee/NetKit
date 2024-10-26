using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NetKit.Observer.Scanners;

public static class TcpScanner
{
    public static async Task<bool> ScanAsync(IPAddress target, int timeout, CancellationToken token, int port = 80)
    {
        token.ThrowIfCancellationRequested();
        using var tcpClient = new TcpClient();
        tcpClient.SendTimeout = timeout;
        try
        {
            await tcpClient.ConnectAsync(target.ToString(), port, token);
            // If the operation is canceled, ConnectAsync will throw OperationCanceledException.
            return tcpClient.Connected;
        }
        catch (SocketException e)
        {
            // Depending on your application logic, you may treat these specific socket errors as non-fatal.
            switch (e.SocketErrorCode)
            {
                case SocketError.ConnectionRefused:
                case SocketError.ConnectionReset:
                case SocketError.HostDown:
                case SocketError.HostUnreachable:
                    return !token.IsCancellationRequested;
                default:
                    return false;
            }
        }
        catch (Exception)
        {
            // Cancellation has been requested; handle accordingly.
            return false;
        }
    }
}