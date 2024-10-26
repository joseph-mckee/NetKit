using System.Net.Sockets;
using NetKit.Device.Management.DeviceConfiguration.Network;
using static System.Text.Encoding;

namespace NetKit.Observer.Workers;

public class ConfigurationWorker : BackgroundService
{
    private readonly ILogger<ConfigurationWorker> _logger;

    public ConfigurationWorker(ILogger<ConfigurationWorker> logger)
    {
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var networkInterfaceConfiguration = new NetworkAdapterConfiguration(2);
            var ipAddresses = new[] { "192.168.1.100" };
            var subnetMasks = new[] { "255.255.255.0" };
            var gateways = new[] { "192.168.1.1" };
            var dnsServers = new[] { "8.8.8.8", "8.8.4.4" };
            // networkInterfaceConfiguration.EnableDhcp();
            // networkInterfaceConfiguration.EnableStatic(ipAddresses, subnetMasks);

            DnsClientCache.Clear();
            var records = DnsClientCache.GetRecords();
            
            _logger.LogInformation("End of loop");
            await Task.Delay(1000, stoppingToken);
        }
    }
}