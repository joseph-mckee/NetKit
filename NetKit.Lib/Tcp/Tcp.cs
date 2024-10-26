using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NetKit.Lib.Tcp;

public static class Tcp
{
    public static async Task<bool> ScanHostAsync(IPAddress target, int timeout, CancellationToken token, int port = 80)
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
            // Debug.WriteLine($"TCP Response: {e.SocketErrorCode} for {target}");
            // Depending on your application logic, you may treat these specific socket errors as non-fatal.
            return e.SocketErrorCode switch
            {
                SocketError.ConnectionRefused or SocketError.ConnectionReset or SocketError.HostDown
                    or SocketError.HostUnreachable => !token.IsCancellationRequested,
                _ => false
            };
        }
        catch (OperationCanceledException)
        {
            // Cancellation has been requested; handle accordingly.
            return false;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unhandled Exception while performing TCP scan on {target}: {e.Message}");
            throw;
        }
    }
}