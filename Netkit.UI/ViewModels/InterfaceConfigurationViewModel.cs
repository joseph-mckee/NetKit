using CommunityToolkit.Mvvm.ComponentModel;
using NetKit.UI.Services;
using NetKit.UI.ViewModels.PageViewModels;

namespace NetKit.UI.ViewModels;

public partial class InterfaceConfigurationViewModel(IpConfigurationPageViewModel parentContext, IpConfigurationService ipConfigurationService) : ViewModelBase
{
    [ObservableProperty] private IpConfigurationPageViewModel _parentContext = parentContext;

    private IpConfigurationService _ipConfigurationService = ipConfigurationService;
}