﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetKit.Lib.Dns;
using NetKit.Lib.Dns.Models;

namespace NetKit.UI.ViewModels.PageViewModels;

public partial class DnsPageViewModel : ViewModelBase, IPageViewModel
{
    [ObservableProperty] private ObservableCollection<DnsRecord> _dnsEntries;
    [ObservableProperty] private ObservableCollection<DnsRecord> _filteredDnsEntries = [];
    [ObservableProperty] private string _filterText = string.Empty;

    private Timer? _refreshTimer;

    partial void OnFilterTextChanged(string? value)
    {
        _refreshTimer?.Dispose();

        FilterList(value, FilteredDnsEntries.ToList());

        _refreshTimer = new Timer(Loop, null, 1000, 1000);
    }

    private void FilterList(string? value, List<DnsRecord> dnsRecords)
    {
        if (string.IsNullOrEmpty(value))
        {
            var adding = DnsEntries.Except(dnsRecords).ToList();
            var removing = dnsRecords.Except(DnsEntries).ToList();

            Dispatcher.UIThread.Invoke(() =>
            {
                foreach (var dnsRecord in adding)
                {
                    if (dnsRecords.FirstOrDefault(x => x.Id == dnsRecord.Id) is not null) continue;
                    FilteredDnsEntries.Add(dnsRecord);
                }

                foreach (var dnsRecord in removing)
                {
                    FilteredDnsEntries.Remove(dnsRecord);
                }
            });
            return;
        }

        var matchText = value.Trim().ToLower();
        var matchingEntries = DnsEntries.Where(x =>
            x.Entry.Contains(matchText, StringComparison.CurrentCultureIgnoreCase) ||
            x.Name.Contains(matchText, StringComparison.CurrentCultureIgnoreCase) ||
            x.Data.Contains(matchText, StringComparison.CurrentCultureIgnoreCase) ||
            x.Type.Contains(matchText, StringComparison.CurrentCultureIgnoreCase) ||
            x.Status.Contains(matchText, StringComparison.CurrentCultureIgnoreCase) ||
            x.Section.Contains(matchText, StringComparison.CurrentCultureIgnoreCase)).ToList();

        var entriesToRemove = dnsRecords.Except(matchingEntries).ToList();
        var entriesToAdd = matchingEntries.Except(dnsRecords).ToList();

        Dispatcher.UIThread.Invoke(() =>
        {
            foreach (var dnsRecord in entriesToRemove)
            {
                FilteredDnsEntries.Remove(dnsRecord);
            }

            foreach (var dnsRecord in entriesToAdd)
            {
                FilteredDnsEntries.Add(dnsRecord);
            }
        });
    }

    public DnsPageViewModel()
    {
        _dnsEntries = new ObservableCollection<DnsRecord>(DnsClientCache.GetRecords());
        FilterList(FilterText, FilteredDnsEntries.ToList());
        _refreshTimer = new Timer(Loop, null, 1000, 1000);
    }

    private void Loop(object? state)
    {
        Task.Run(RefreshList);
    }

    [RelayCommand]
    public void FlushCacheCommand()
    {
        DnsClientCache.Clear();
    }

    private async void RefreshList()
    {
        // DnsEntries = new(DnsClientCache.GetRecords());
        var currentEntries = DnsEntries.ToList();
        var newEntries = DnsClientCache.GetRecords().ToList();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var dnsRecord in currentEntries)
            {
                if (newEntries.FirstOrDefault(x => x.Id == dnsRecord.Id) is not null) continue;
                DnsEntries.Remove(dnsRecord);
            }

            foreach (var dnsRecord in newEntries)
            {
                if (currentEntries.FirstOrDefault(x => x.Id == dnsRecord.Id) is not null) continue;
                DnsEntries.Add(dnsRecord);
            }

            FilterList(FilterText, FilteredDnsEntries.ToList());
        });
    }

    public string Label { get; } = "DNS Table";

    public string IconData { get; } =
        "m437.02 74.98c-48.353-48.352-112.64-74.98-181.02-74.98s-132.667 26.628-181.02 74.98-74.98 112.64-74.98 181.02 " +
        "26.628 132.667 74.98 181.02 112.64 74.98 181.02 74.98 132.667-26.628 181.02-74.98 74.98-112.64 " +
        "74.98-181.02-26.628-132.667-74.98-181.02zm-2.132 315.679c-15.31-10.361-31.336-19.314-47.952-26.789 7.339-28.617 " +
        "11.697-59.688 12.784-91.87h79.702c-3.144 44.336-19.244 85.147-44.534 118.659zm-402.31-118.659h79.702c1.088 32.183 " +
        "5.446 63.254 12.784 91.87-16.616 7.475-32.642 16.427-47.952 26.789-25.29-33.512-41.39-74.323-44.534-118.659zm44.53-150.654c15.31 " +
        "10.362 31.336 19.315 47.954 26.79-7.338 28.615-11.695 59.683-12.783 91.864h-79.701c3.144-44.334 19.243-85.142 44.53-118.654zm283.519-42.581c-5.863-10.992-12.198-20.911-18.935-29.713 " +
        "27.069 11.25 51.473 27.658 71.977 47.997-11.625 7.638-23.702 14.369-36.155 20.185-4.886-13.664-10.528-26.547-16.887-38.469zm-12.965 " +
        "50.404c-29.211 9.792-60.039 14.831-91.662 14.831s-62.451-5.039-91.662-14.831c20.463-58.253 54.273-97.169 91.662-97.169s71.199 38.916 " +
        "91.662 97.169zm-203.359 110.831c1.056-28.342 4.885-55.421 10.937-80.116 32.136 10.644 66.018 16.116 100.76 16.116s68.624-5.472 " +
        "100.76-16.116c6.053 24.695 9.881 51.773 10.937 80.116zm223.394 32c-1.057 28.344-4.885 55.424-10.938 " +
        "80.12-32.139-10.646-66.02-16.12-100.759-16.12s-68.62 5.474-100.759 16.12c-6.053-24.696-9.882-51.776-10.938-80.12zm-216.324-193.235c-6.358 " +
        "11.922-12 24.805-16.887 38.468-12.452-5.815-24.53-12.547-36.155-20.185 20.503-20.34 44.907-36.747 71.977-47.997-6.737 8.803-13.073 18.722-18.935 29.714zm-16.886 316.008c4.886 13.661 10.528 26.542 16.885 38.462 5.863 10.992 12.198 20.911 18.935 29.713-27.067-11.25-51.469-27.655-71.971-47.992 11.625-7.637 23.701-14.368 36.151-20.183zm29.853-11.938c29.213-9.794 60.04-14.835 91.66-14.835s62.447 5.041 91.66 14.835c-20.463 58.251-54.272 97.165-91.66 97.165s-71.197-38.914-91.66-97.165zm196.287 50.4c6.357-11.92 11.999-24.801 16.885-38.462 12.451 5.815 24.527 12.547 36.151 20.183-20.502 20.337-44.904 36.743-71.971 47.992 6.737-8.802 13.073-18.721 18.935-29.713zm39.093-193.235c-1.088-32.18-5.445-63.249-12.783-91.864 16.618-7.475 32.645-16.428 47.954-26.79 25.287 33.511 41.386 74.319 44.53 118.654z";
}