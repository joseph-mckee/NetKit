namespace NetKit.Device.Management.DeviceConfiguration.Network.Enums;

public enum DnsType
{
    A = 1,
    NS = 2,
    CNAME = 5,
    SOA = 6,
    PTR = 12,
    MX = 15,
    AAAA = 28,
    SRV = 33
}

public enum DnsStatus
{
    Success = 0,
    NotExist = 9003,
    NoRecords = 9501,
    RecordDoesNotExist = 9701
}

public enum DnsSection
{
    Answer = 1,
    Authority = 2,
    Additional = 3
}