using System.Diagnostics;
using System.Net;
using NetKit.Observer.Scanners;
using NetKit.Observer.Utilities;

namespace NetKit.Observer.Workers;

public class ScanWorker : BackgroundService
{
    private readonly ILogger<ScanWorker> _logger;

    // Services for listening to various protocols

    public ScanWorker(ILogger<ScanWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
            // Check for configuration changes from server

            // Figure out hosts to scan
            var addressesToScan = new List<IPAddress>();
            var networks = LocalNetwork.GetLocalIpv4Networks();
            networks.Remove(IPAddress.Parse("172.20.48.1"));
            foreach (var network in networks)
            {
                var info = new NetworkInformation(network.Key, network.Value);
                addressesToScan.AddRange(info.GetAddressRangeFromNetwork());
            }

            _logger.LogInformation($"Found {addressesToScan.Count} addresses to scan.");

            // Perform all scan operations
            // IEnumerable<IPAddress> scannedHosts = Scanner.ScanAll(addressesToScan);
            var livingHosts = new List<IPAddress>();

            var timer = new Stopwatch();

            var semaphore = new SemaphoreSlim(254);

            var arpTable = Arp.GetArpCache();

            var tasks = addressesToScan.Select(ipAddress => Task.Run(async () =>
                {
                    await semaphore.WaitAsync(stoppingToken);
                    try
                    {
                        var scanTasks = new[]
                        {
                            TcpScanner.ScanAsync(ipAddress, 1000, stoppingToken),
                            PingScanner.ScanHostAsync(ipAddress, 1000, stoppingToken)
                        };
                        var finishedTask = await Task.WhenAny(scanTasks);
                        var scanType = string.Empty;
                        if (finishedTask == scanTasks[0]) scanType = "TCP";
                        else if (finishedTask == scanTasks[1]) scanType = "Ping";
                        var result = finishedTask.Result;
                        if (!result) result = await PingScanner.ScanHostAsync(ipAddress, 1000, stoppingToken);
                        if (!result && arpTable.Select(entry => entry.IpAddress)!
                                .Contains<string>(ipAddress.ToString()))
                        {
                            scanType = "ARP";
                            result = await ArpScanner.ScanAsync(ipAddress, stoppingToken);
                        }

                        if (result)
                        {
                            livingHosts.Add(ipAddress);
                            if (result) _logger.LogInformation($"Host found at {ipAddress} using {scanType} scan.");
                        }

                        stoppingToken.ThrowIfCancellationRequested();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, stoppingToken))
                .ToList();

            _logger.LogInformation("Beginning scan.");
            timer.Start();
            await Task.WhenAll(tasks);
            timer.Stop();
            _logger.LogInformation($"Found {livingHosts.Count} hosts in scan.");
            _logger.LogInformation($"Scan took {timer.Elapsed.Seconds} seconds.");

            // Retrieve data about scanned hosts
            // SNMP, DNS, Vendor, etc.

            // Organize data from scans
            // Serialize host models into JSON

            // Push data to EF server
            // Send serialized host models via HTTPS

            // Repeat


            await Task.Delay(1000, stoppingToken);
        }
    }
}