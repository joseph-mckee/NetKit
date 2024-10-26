using System;
using System.Linq;
using Avalonia.Controls;
using NetKit.UI.ViewModels.PageViewModels;

namespace NetKit.UI.Views.PageViews;

public partial class PingPageView : UserControl
{
    
    private readonly DataGrid? _dataGrid;
    
    public PingPageView()
    {
        InitializeComponent();
        _dataGrid = this.FindControl<DataGrid>("ReplyGrid");
        DataContextChanged += PingView_DataContextChanged;
    }
    
    private void PingView_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is PingPageViewModel viewModel)
            viewModel.ScrollToNewItemRequested += ViewModel_ScrollToNewItemRequested;
    }

    private void ViewModel_ScrollToNewItemRequested(object? sender, EventArgs e)
    {
        if (DataContext is PingPageViewModel { PingReplies.Count: > 0 } viewModel)
            _dataGrid?.ScrollIntoView(viewModel.PingReplies.Last(), null);
    }
}