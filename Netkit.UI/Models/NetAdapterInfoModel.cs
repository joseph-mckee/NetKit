using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NetKit.UI.Models;

public partial class NetAdapterInfoModel : ObservableObject
{
    // Basic Info
    [ObservableProperty] private int _interfaceIndex;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _netConnectionId = string.Empty;
    [ObservableProperty] private string _physicalAddress = string.Empty;
    
    // Advanced Info
    [ObservableProperty] private int _mtu;
    [ObservableProperty] private int _tcpWindowSize;
    [ObservableProperty] private int _keepAliveTime;
    [ObservableProperty] private int _keepAliveInterval;
    [ObservableProperty] private bool _deadGwDetectEnabled;
    
    // Operational Status
    [ObservableProperty] private string _status = string.Empty;
    [ObservableProperty] private string _availability = string.Empty;
    [ObservableProperty] private string _netConnectionStatus = string.Empty;
    
    // Hardware Information
    [ObservableProperty] private string _adapterType = string.Empty;
    [ObservableProperty] private string _adapterSpeed = string.Empty;
    [ObservableProperty] private string _pnpDeviceId = string.Empty;
    [ObservableProperty] private string _driverName = string.Empty;
    
    // Statistics
    [ObservableProperty] private string _bytesSent = string.Empty;
    [ObservableProperty] private string _bytesReceived = string.Empty;
    [ObservableProperty] private string _packetsSent = string.Empty;
    [ObservableProperty] private string _packetsReceived = string.Empty;
    [ObservableProperty] private string _packetsOutboundErrors = string.Empty;
    [ObservableProperty] private string _packetReceivedErrors = string.Empty;
    
    // IPSEC
    [ObservableProperty] private bool _ipSecEnabled;
    [ObservableProperty] private bool _ipFilterSecurityEnabled;
    
    // IP Properties
    [ObservableProperty] private bool _ipEnabled;
    [ObservableProperty] private ObservableCollection<BindableString> _ipAddresses = [];
    [ObservableProperty] private ObservableCollection<BindableString> _subnetMasks = [];
    [ObservableProperty] private ObservableCollection<BindableString> _gateways = [];
    [ObservableProperty] private ObservableCollection<BindableString> _gatewayMetrics = [];
    [ObservableProperty] private ObservableCollection<BindableString> _multicastAddresses = [];
    [ObservableProperty] private bool _isApipaEnabled;
    [ObservableProperty] private bool _isApipaActive;
    
    // DHCP Properties
    [ObservableProperty] private bool _isDhcpEnabled;
    [ObservableProperty] private string _dhcpServerAddress = string.Empty;
    [ObservableProperty] private DateTime _dhcpLeaseObtained;
    [ObservableProperty] private DateTime _dhcpLeaseExpired;
    
    // DNS Properties
    [ObservableProperty] private string _dnsDomain = string.Empty;
    [ObservableProperty] private ObservableCollection<BindableString> _dnsDomainSuffixSearchOrder = [];
    [ObservableProperty] private string _dnsHostname = string.Empty;
    [ObservableProperty] private ObservableCollection<BindableString> _dnsServerSearchOrder = [];
    [ObservableProperty] private bool _domainDnsRegistrationEnabled;
    [ObservableProperty] private bool _fullDnsRegistrationEnabled;
}