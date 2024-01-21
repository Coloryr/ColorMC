using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class Info4Model(string? name) : ObservableObject
{
    public Action<bool>? Call;

    [ObservableProperty]
    private string _text;
    [ObservableProperty]
    private bool _enable;
    [ObservableProperty]
    private bool _cancelVisable;
    [ObservableProperty]
    private bool _enableVisable = true;

    [RelayCommand]
    public void Cancel()
    {
        Enable = false;
        Call?.Invoke(false);
        DialogHost.Close(name);
    }

    [RelayCommand]
    public void Confirm()
    {
        Enable = false;
        Call?.Invoke(true);
        DialogHost.Close(name);
    }
}
