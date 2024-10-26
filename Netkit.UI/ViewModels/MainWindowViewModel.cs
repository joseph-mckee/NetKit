using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetKit.UI.ViewModels.PageViewModels;

namespace NetKit.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isPaneOpen;

    [ObservableProperty] private IPageViewModel _currentPage = new HomePageViewModel();

    [ObservableProperty] private string _theme = "Dark";

    [ObservableProperty] private string _oppositeTheme = "Light";
    
    partial void OnThemeChanged(string value)
    {
        if (value != "Dark" && value != "Light") return;
        if (value == "Dark") OppositeTheme = "Light";
        if (value == "Light") OppositeTheme = "Dark";
    }
    
    [ObservableProperty] private IPageViewModel? _selectedMenuItem;
    [ObservableProperty] private int _selectedIndex;

    partial void OnSelectedMenuItemChanged(IPageViewModel? value)
    {
        if (value is null) return;
        CurrentPage = value;
    }

    [ObservableProperty] private ObservableCollection<IPageViewModel> _menuItems;

    [RelayCommand]
    public void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [RelayCommand]
    public void ToggleTheme()
    {
        Theme = OppositeTheme;
    }

    public MainWindowViewModel()
    {
        _menuItems = [
            new IpConfigurationPageViewModel(),
            new PingPageViewModel(),
            new TraceRoutePageViewModel(),
            new ScanPageViewModel(),
            new DnsPageViewModel()
        ];
        SelectedMenuItem = MenuItems[2];
    }
    
    public MainWindowViewModel(IpConfigurationPageViewModel ipConfigurationPageViewModel, PingPageViewModel pingPageViewModel, TraceRoutePageViewModel traceRoutePageViewModel, ScanPageViewModel scanPageViewModel)
    {
        _menuItems = [
            ipConfigurationPageViewModel,
            pingPageViewModel,
            traceRoutePageViewModel,
            scanPageViewModel,
            new DnsPageViewModel()
        ];
        SelectedMenuItem = MenuItems[2];
    }
} 