using System.Security.Cryptography;
using System.Text;

namespace NetKit.Lib.Dns.Models;

public class DnsRecord
{
    public void GenerateId()
    {
        var concatenatedString = InstanceId + Caption + Description + ElementName + Entry + Name +
                                 Type + DataLength + Section + Data + Status;
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(concatenatedString));
        var sb = new StringBuilder();
        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }
        Id = sb.ToString();
    }
    
    public string InstanceId { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ElementName { get; set; } = string.Empty;
    public string Entry { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    // public string? TimeToLive { get; set; } = string.Empty;
    public string DataLength { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}