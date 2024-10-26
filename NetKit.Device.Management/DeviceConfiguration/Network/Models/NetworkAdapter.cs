namespace NetKit.Device.Management.DeviceConfiguration.Network.Models;

public class NetworkAdapter
{
    public int Availability { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public int StatusInfo { get; set; }
    public int DeviceId { get; set; }
    public string? Caption { get; set; }
    public string? Description { get; set; }
    public DateTime? InstallDate { get; set; }
    public int ConfigManagerErrorCode { get; set; }
    public bool ConfigManagerUserConfig { get; set; }
    public string? CreationClassName { get; set; }
    public bool ErrorCleared { get; set; }
    public string? ErrorDescription { get; set; }
    public int LastErrorCode { get; set; }
    public string? PnpDeviceId { get; set; }
    public int[]? PowerManagementCapabilities { get; set; }
    public bool PowerManagementSupported { get; set; }
    public string? SystemCreationClassName { get; set; }
    public string? SystemName { get; set; }
    public bool AutoSense { get; set; }
    public long MaxSpeed { get; set; }
    public string[]? NetworkAddresses { get; set; }
    public string? PermanentAddress { get; set; }
    public long Speed { get; set; }
    public string? AdapterType { get; set; }
    public int AdapterTypeId { get; set; }
    public string? Guid { get; set; }
    public int Index { get; set; }
    public bool Installed { get; set; }
    public int InterfaceIndex { get; set; }
    public string? MacAddress { get; set; }
    public string? Manufacturer { get; set; }
    public int MaxNumberControlled { get; set; }
    public string? NetConnectionId { get; set; }
    public int NetConnectionStatus { get; set; }
    public bool NetEnabled { get; set; }
    public bool PhysicalAdapter { get; set; }
    public string? ProductName { get; set; }
    public string? ServiceName { get; set; }
    public DateTime? TimeOfLastReset { get; set; }
    public string? PsComputerName { get; set; }
    public string? CimClass { get; set; }
    public string[]? CimInstanceProperties { get; set; }
    public string? CimSystemProperties { get; set; }
}