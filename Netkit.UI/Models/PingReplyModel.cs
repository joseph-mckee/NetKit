using System.Net.NetworkInformation;
using NetKit.Lib.Ping;

namespace NetKit.UI.Models;

public class PingReplyModel(PingReplyEx reply, int index)
{
    public int Index { get; } = index;
    public string? IpAddress { get; } = reply.IpAddress.ToString();
    public long RoundtripTime { get; } = reply.RoundTripTime;

    public IPStatus Status { get; } = reply.Status;
}