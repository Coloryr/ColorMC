using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class Info6Model : ObservableObject
{
    public bool IsCancel;

    [ObservableProperty]
    private string _text1;
    [ObservableProperty]
    private string _text2;

    private readonly string? _name;
    public Info6Model(string? name)
    {
        _name = name;
    }

    [RelayCommand]
    public void Cancel()
    {
        IsCancel = true;

        DialogHost.Close(_name);
    }

    [RelayCommand]
    public void Confirm()
    {
        IsCancel = false;

        DialogHost.Close(_name);
    }
}
