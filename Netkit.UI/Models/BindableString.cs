using CommunityToolkit.Mvvm.ComponentModel;

namespace NetKit.UI.Models;

public partial class BindableString(string value) : ObservableObject
{
    [ObservableProperty] private string _value = value;
    
    

    public BindableString() : this(string.Empty)
    {
    }
}