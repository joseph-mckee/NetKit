using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetKit.Lib.Ping;
using NetKit.UI.Models;

namespace NetKit.UI.ViewModels.PageViewModels;

public partial class PingPageViewModel : ViewModelBase, IPageViewModel
{
    public string Label { get; } = "Ping Tool";
    public string IconData { get; } = "M12,1.99789 C17.524,1.99789 22.0021,6.47599 22.0021,12 C22.0021,17.524 17.524,22.0021 12,22.0021 C6.47602,22.0021 1.99792,17.524 1.99792,12 C1.99792,6.47599 6.47602,1.99789 12,1.99789 Z M12,3.49789 C7.30445,3.49789 3.49792,7.30441 3.49792,12 C3.49792,16.6956 7.30445,20.5021 12,20.5021 C16.6956,20.5021 20.5021,16.6956 20.5021,12 C20.5021,7.30441 16.6956,3.49789 12,3.49789 Z M12,6 C15.3137,6 18,8.68629 18,12 C18,15.3137 15.3137,18 12,18 C8.68629,18 6,15.3137 6,12 C6,8.68629 8.68629,6 12,6 Z M12,7.5 C9.51472,7.5 7.5,9.51472 7.5,12 C7.5,14.4853 9.51472,16.5 12,16.5 C14.4853,16.5 16.5,14.4853 16.5,12 C16.5,9.51472 14.4853,7.5 12,7.5 Z M12,10 C13.1046,10 14,10.8954 14,12 C14,13.1046 13.1046,14 12,14 C10.8954,14 10,13.1046 10,12 C10,10.8954 10.8954,10 12,10 Z";
    
    [ObservableProperty] private int? _attempts = 4;
    [ObservableProperty] private int? _buffer = 32;
    private CancellationTokenSource? _cancellationTokenSource;
    [ObservableProperty] private int? _delay = 200;
    [ObservableProperty] private int _failedPings;
    [ObservableProperty] private bool _fragmentable;
    [ObservableProperty] private int? _hops = 30;
    [ObservableProperty] private string _host = "8.8.8.8";
    [ObservableProperty] private string _hostname = string.Empty;
    [ObservableProperty] private bool _isContinuous;
    [ObservableProperty] private bool _isIndeterminate;
    [ObservableProperty] private bool _isPinging;
    [ObservableProperty] private bool _isStopped;
    [ObservableProperty] private ObservableCollection<InterfaceModel> _networkInterfaces = [];
    [ObservableProperty] private string _packetLoss = "0%";
    [ObservableProperty] private ObservableCollection<PingReplyModel>? _pingReplies;
    [ObservableProperty] private int _progress;

    [ObservableProperty] private ulong _replyTimes;
    [ObservableProperty] private string _roundTripTime = string.Empty;
    [ObservableProperty] private int _selectedIndex;
    [ObservableProperty] private InterfaceModel? _selectedInterface;
    [ObservableProperty] private int _successfulPings;
    [ObservableProperty] private int? _timeout = 1000;
    
    public PingPageViewModel()
    {
        Reset();
    }

    public event EventHandler? ScrollToNewItemRequested;

    protected virtual void OnScrollToNewItemRequested()
    {
        ScrollToNewItemRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public async Task StartPing()
    {
        Reset();
        IsStopped = false;
        IsPinging = true;
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        await Task.Run(() => PreparePing(token), token);
        if (token.IsCancellationRequested) return;
        IsPinging = false;
    }

    private async Task PreparePing(CancellationToken cancellationToken)
    {
        if (Hops is null) return;
        PingOptions pingOptions = new()
        {
            Ttl = Hops.Value,
            DontFragment = !Fragmentable
        };
        if (Buffer is null) return;
        var buffer = new byte[Buffer.Value];
        try
        {
            ResolveDnsInBackground(Host, cancellationToken);
        }
        catch (Exception)
        {
            Debug.WriteLine("DNS Query Cancelled.");
            throw;
        }

        if (IsContinuous)
        {
            IsIndeterminate = true;
            while (true)
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await SendPing(Progress, buffer, pingOptions, cancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    Debug.WriteLine(ex.Message);
                    break;
                }
        }
        else
        {
            for (var i = 0; i < Attempts; i++)
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await SendPing(i, buffer, pingOptions, cancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    Debug.WriteLine(ex.Message);
                    break;
                }
        }

        IsStopped = true;
    }

    private async Task SendPing(int index, byte[]? buffer, PingOptions? pingOptions,
        CancellationToken cancellationToken)
    {
        try
        {
            SelectedInterface ??= NetworkInterfaces[0];
            if (SelectedInterface.IpAddress is null) return;
            var source = IPAddress.Parse(SelectedInterface.IpAddress);
            var dest = IPAddress.Parse(Host);
            if (Timeout is null) return;
            var reply = await Task.Run(() => PingEx.Send(source, dest, Timeout.Value, buffer, pingOptions),
                cancellationToken);
            Dispatcher.UIThread.Invoke(() =>
            {
                if (reply.Status == IPStatus.Success) SuccessfulPings++;
                else FailedPings++;
                PingReplies?.Add(new PingReplyModel(reply, index + 1));
                OnScrollToNewItemRequested();
                ReplyTimes += reply.RoundTripTime;
                var averageRtt = ((float)ReplyTimes / PingReplies!.Count).ToString("N");
                RoundTripTime = $"{averageRtt}ms";
                Progress++;
            });
        }
        catch (PingException)
        {
            StopPinging();
        }

        try
        {
            if (Delay is null) return;
            if (Progress < Attempts || IsContinuous) await Task.Delay(Delay.Value, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
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
        if (Attempts is null) return false;
        if (Timeout is null) return false;
        if (Delay is null) return false;
        if (Hops is null) return false;
        if (Buffer is null) return false;

        return true;
    }

    private async void ResolveDnsInBackground(string address, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var entry = await Dns.GetHostEntryAsync(address, cancellationToken);
            Dispatcher.UIThread.Invoke(() => { Hostname = entry.HostName; });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public void StopPinging()
    {
        _cancellationTokenSource?.Cancel();
        IsPinging = false;
        IsStopped = true;
        IsIndeterminate = false;
        // Progress = int.Parse(Attempts);
    }

    public void Reset()
    {
        StopPinging();
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
        IsStopped = true;
        PingReplies = new ObservableCollection<PingReplyModel>();
        SuccessfulPings = 0;
        FailedPings = 0;
        PacketLoss = "0%";
        ReplyTimes = 0;
        RoundTripTime = string.Empty;
        Hostname = string.Empty;
        PingReplies.Clear();
        Progress = 0;
    }
}