using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class Info4Model : ObservableObject
{
    public Action<bool>? Call;

    [ObservableProperty]
    private string _text;
    [ObservableProperty]
    private bool _enable;
    [ObservableProperty]
    private bool _cancelVisable;

    private readonly string? _name;

    public Info4Model(string? name)
    {
        _name = name;
    }

    [RelayCommand]
    public void Cancel()
    {
        Enable = false;

        DialogHost.Close(_name);

        Call?.Invoke(false);
    }

    [RelayCommand]
    public void Confirm()
    {
        Enable = false;

        DialogHost.Close(_name);

        Call?.Invoke(true);
    }
}
