namespace NetKit.Lib.Arp;

public class ArpEntry
{
    public string IpAddress { get; init; } = string.Empty;
    public string MacAddress { get; init; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public int Index { get; init; }
}