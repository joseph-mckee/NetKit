using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using NetKit.UI.Comparers;
using NetKit.UI.ViewModels;
using NetKit.UI.ViewModels.PageViewModels;

namespace NetKit.UI.Views.PageViews;

public partial class ScanPageView : UserControl
{
    public ScanPageView()
    {
        InitializeComponent();
        var dataGrid = this.FindControl<DataGrid>("HostGrid");
        var ipColumn = dataGrid?.Columns
            .FirstOrDefault(c => c.Header.ToString() == "IP Address");
        if (ipColumn != null) ipColumn.CustomSortComparer = new IpAddressComparer();
    }

    private void CopyIp_OnClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        clipboard?.SetTextAsync(GetCellText(sender));
    }

    private void CopyMac_OnClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        clipboard?.SetTextAsync(GetCellText(sender));
    }

    private void OpenIp_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var url = $"https://{GetCellText(sender)}";
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        catch (Exception ex)
        {
            // Handle any exceptions here
            Console.WriteLine($"Error opening URL: {ex.Message}");
        }
    }

    private static string GetCellText(object? menuItem)
    {
        var cell = menuItem as MenuItem;
        var content = cell.Parent.Parent.Parent as DataGridCell;
        var text = content.Content as TextBlock;
        return text.Text;
    }

    private async void Ping_OnClick(object? sender, RoutedEventArgs e)
    {
        var text = GetCellText(sender);
        var mainView = App.Current.Services.GetRequiredService<MainWindowViewModel>();
        var pingView = App.Current.Services.GetRequiredService<PingPageViewModel>();
        pingView.Host = text;
        mainView.SelectedMenuItem = pingView;
        await pingView.StartPing();
    }

    private async void Traceroute_OnClick(object? sender, RoutedEventArgs e)
    {
        var text = GetCellText(sender);
        var mainView = App.Current.Services.GetRequiredService<MainWindowViewModel>();
        var traceRouteView = App.Current.Services.GetRequiredService<TraceRoutePageViewModel>();
        traceRouteView.Host = text;
        mainView.SelectedMenuItem = traceRouteView;
        await traceRouteView.TraceRoute();
    }

    private async void Export_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Results",
            DefaultExtension = "csv",
            SuggestedFileName = "results.csv"
        });
        if (file is null) return;
        await using var stream = await file.OpenWriteAsync();
        await using var streamWriter = new StreamWriter(stream);
        await using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        csv.WriteRecordsAsync(App.Current.Services.GetRequiredService<ScanPageViewModel>().FilteredScannedHosts);
    }


    private void CopyHost_OnClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        clipboard?.SetTextAsync(GetCellText(sender));
    }

    private void CopyVendor_OnClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        clipboard?.SetTextAsync(GetCellText(sender));
    }
}