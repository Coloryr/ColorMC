using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    protected virtual void IsSelectChanged(bool value) { }
}
