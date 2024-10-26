using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using NetKit.Lib.Ping;
using NetKit.UI.Models;

namespace NetKit.UI.ViewModels.PageViewModels;

public partial class TraceRoutePageViewModel : ViewModelBase, IPageViewModel
{
    public string Label { get; } = "Traceroute Tool";

    public string IconData { get; } =
        "M13 0a3 3 0 00-1.65 5.506 7.338 7.338 0 01-.78 1.493c-.22.32-.472.635-.8 1.025a1.509 1.509 0 00-.832.085 12.722 12.722 0 00-1.773-1.124c-.66-.34-1.366-.616-2.215-.871a1.5 1.5 0 10-2.708 1.204c-.9 1.935-1.236 3.607-1.409 5.838a1.5 1.5 0 101.497.095c.162-2.07.464-3.55 1.25-5.253.381-.02.725-.183.979-.435.763.23 1.367.471 1.919.756a11.13 11.13 0 011.536.973 1.5 1.5 0 102.899-.296c.348-.415.64-.779.894-1.148.375-.548.665-1.103.964-1.857A3 3 0 1013 0zm-1.5 3a1.5 1.5 0 113 0 1.5 1.5 0 01-3 0z";
    
    private CancellationTokenSource? _cancellationTokenSource;
    [ObservableProperty] private int? _delay = 200;
    [ObservableProperty] private bool _doResolve;
    [ObservableProperty] private int? _hops = 30;
    [ObservableProperty] private string _host = "8.8.8.8";
    [ObservableProperty] private bool _isStarted;
    [ObservableProperty] private bool _isStopped = true;
    [ObservableProperty] private ObservableCollection<InterfaceModel> _networkInterfaces = new();
    [ObservableProperty] private int _selectedIndex;
    [ObservableProperty] private InterfaceModel? _selectedInterface;
    [ObservableProperty] private int? _timeout = 1000;

    [ObservableProperty] private ObservableCollection<TracerouteReplyModel>? _tracerouteReplyModels;

    public TraceRoutePageViewModel()
    {
        Reset();
    }

    partial void OnSelectedIndexChanged(int value)
    {
        if (value < 0) return;
        SelectedInterface = NetworkInterfaces[value];
    }

    private async void ResolveDnsInBackground(int index, CancellationToken token)
    {
        try
        {
            var hostNameOrAddress = TracerouteReplyModels?[index].IpAddress;
            if (hostNameOrAddress == null) return;
            var entry = await Dns.GetHostEntryAsync(hostNameOrAddress, token);
            token.ThrowIfCancellationRequested();
            Dispatcher.UIThread.Invoke(() =>
            {
                if (TracerouteReplyModels != null)
                    TracerouteReplyModels[index] = new TracerouteReplyModel
                    {
                        Index = TracerouteReplyModels[index].Index,
                        IpAddress = hostNameOrAddress,
                        RoundTripTime = TracerouteReplyModels[index].RoundTripTime,
                        Status = TracerouteReplyModels[index].Status,
                        HostName = entry.HostName
                    };
            });
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Cancelled DNS resolve.");
        }
        catch (Exception ex)
        {
            if (token.IsCancellationRequested) return;
            Dispatcher.UIThread.Invoke(() =>
            {
                if (TracerouteReplyModels != null)
                    TracerouteReplyModels[index] = new TracerouteReplyModel
                    {
                        Index = TracerouteReplyModels[index].Index,
                        IpAddress = TracerouteReplyModels[index].IpAddress,
                        RoundTripTime = TracerouteReplyModels[index].RoundTripTime,
                        Status = TracerouteReplyModels[index].Status,
                        HostName = "Unknown"
                    };
            });
            Debug.WriteLine(ex.Message);
        }
    }

    private async Task<bool> IsInputValid()
    {
        if (!IPAddress.TryParse(Host, out _))
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(Host);
                if (addresses.Length > 0) Host = addresses[0].ToString();
            }
            catch
            {
                return false;
            }
        if (Timeout is null) return false;
        if (Delay is null) return false;
        if (Hops is null) return false;

        return true;
    }
    
    public async Task TraceRoute()
    {
        Reset();
        IsStarted = true;
        IsStopped = false;
        if (!IPAddress.TryParse(Host, out _))
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(Host);
                if (addresses.Length > 0) Host = addresses[0].ToString();
            }
            catch
            {
                Debug.WriteLine("Failed to Resolve DNS");
                return;
            }

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        try
        {
            for (var i = 1; i < Hops; i++)
            {
                token.ThrowIfCancellationRequested();
                PingOptions pingOptions = new()
                {
                    Ttl = i,
                    DontFragment = true
                };
                var buffer = new byte[32];
                if (Host is not null)
                {
                    var source = IPAddress.Parse(SelectedInterface?.IpAddress ?? throw new InvalidOperationException());
                    var dest = IPAddress.Parse(Host);
                    var reply = await Task.Run(() => PingEx.Send(source, dest, Timeout.Value, buffer, pingOptions),
                        token);
                    token.ThrowIfCancellationRequested();
                    if (reply.Status is IPStatus.Success or IPStatus.TtlExpired)
                    {
                        if (DoResolve)
                            try
                            {
                                TracerouteReplyModels?.Add(new TracerouteReplyModel
                                {
                                    Index = i,
                                    IpAddress = reply.IpAddress.ToString(),
                                    RoundTripTime = reply.RoundTripTime.ToString(),
                                    Status = reply.Status.ToString()
                                });
                                ResolveDnsInBackground(i - 1, token);
                            }
                            catch (SocketException e)
                            {
                                Debug.WriteLine(e.Message);
                            }
                        else
                            TracerouteReplyModels?.Add(new TracerouteReplyModel
                            {
                                Index = i,
                                IpAddress = reply.IpAddress.ToString(),
                                RoundTripTime = reply.RoundTripTime.ToString(),
                                Status = reply.Status.ToString(),
                                HostName = "Unknown"
                            });

                        if (reply.Status == IPStatus.Success)
                            break;
                    }
                    else
                    {
                        TracerouteReplyModels?.Add(new TracerouteReplyModel
                        {
                            Index = i,
                            IpAddress = reply.IpAddress.ToString(),
                            RoundTripTime = "N/A",
                            Status = reply.Status.ToString(),
                            HostName = "N/A"
                        });
                    }
                }

                // Handle empty field
                await Task.Delay(Delay.Value, token);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Traceroute operation cancelled.");
        }
        finally
        {
            IsStarted = false;
            IsStopped = true;
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        IsStarted = false;
        IsStopped = true;
    }

    public void Reset()
    {
        Stop();
        var selected = SelectedIndex;
        NetworkInterfaces.Clear();
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var networkInterface in networkInterfaces)
        {
            try
            {
                networkInterface.GetIPProperties().GetIPv4Properties();
            }
            catch
            {
                continue;
            }

            if (networkInterface.OperationalStatus == OperationalStatus.Up)
                NetworkInterfaces.Add(new InterfaceModel(networkInterface));
        }

        NetworkInterfaces = new ObservableCollection<InterfaceModel>(NetworkInterfaces.OrderBy(o => o.Metric));
        SelectedIndex = selected;
        TracerouteReplyModels = new ObservableCollection<TracerouteReplyModel>();
    }
}