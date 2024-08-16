using Avalonia.Media;
using ColorMC.Gui.Manager;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model;

public partial class SelectItemModel : ObservableObject
{
    [ObservableProperty]
    private BoxShadows _border = ThemeManager.BorderShadows;

    [ObservableProperty]
    private bool _isSelect;
    [ObservableProperty]
    private bool _top;
    [ObservableProperty]
    private bool _enableButton;

    partial void OnIsSelectChanged(bool value)
    {
        if (IsSelect)
        {
            Border = ThemeManager.BorderSelecrShadows;
        }
        else
        {
            Border = ThemeManager.BorderShadows;
        }

        IsSelectChanged(value);
    }

    protected virtual void IsSelectChanged(bool value) 
    {
        EnableButton = Top || IsSelect;
    }

    partial void OnTopChanged(bool value)
    {
        EnableButton = Top || IsSelect;
    }
}
