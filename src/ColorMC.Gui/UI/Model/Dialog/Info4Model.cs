using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class Info4Model(string? name) : ObservableObject
{
    public Action<bool>? Call;
    public Action? ChoiseCall;

    [ObservableProperty]
    private string _text;
    [ObservableProperty]
    private bool _enable;
    [ObservableProperty]
    private bool _cancelVisable;
    [ObservableProperty]
    private bool _enableVisable = true;

    [ObservableProperty]
    private string _choiseText;
    [ObservableProperty]
    private bool _choiseVisiable;

    [RelayCommand]
    public void Choise()
    {
        ChoiseCall?.Invoke();
    }

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
