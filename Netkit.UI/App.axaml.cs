using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NetKit.UI.Services;
using NetKit.UI.ViewModels;
using NetKit.UI.ViewModels.PageViewModels;
using NetKit.UI.Views;

namespace NetKit.UI;

public partial class App : Application
{
    public static new App Current => (App)Application.Current;
    public IServiceProvider Services { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        BindingPlugins.DataValidators.RemoveAt(0);
        
        var collection = new ServiceCollection();
        collection.AddCommonServices();

        Services = collection.BuildServiceProvider();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public static class ServiceCollectionExtensions {
    public static void AddCommonServices(this IServiceCollection collection) {
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddSingleton<PingPageViewModel>();
        collection.AddSingleton<TraceRoutePageViewModel>();
        collection.AddSingleton<ScanPageViewModel>();
        collection.AddSingleton<IpConfigurationPageViewModel>();
        collection.AddSingleton<IpConfigurationProfileViewModel>();
    }
}