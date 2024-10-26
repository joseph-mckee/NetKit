namespace NetKit.UI.Models;

public class ScannedHostModel
{
    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string ScanMethod { get; set; } = string.Empty;
}