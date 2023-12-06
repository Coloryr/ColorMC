using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class Info3Model(string? name) : ObservableObject
{
    public Action? Call;

    public bool IsCancel;

    [ObservableProperty]
    private string _text1;
    [ObservableProperty]
    private string _text2;

    [ObservableProperty]
    private string _watermark1;
    [ObservableProperty]
    private string _watermark2;
    [ObservableProperty]
    private bool _confirmEnable;
    [ObservableProperty]
    private bool _cancelEnable;
    [ObservableProperty]
    private bool _cancelVisible;
    [ObservableProperty]
    private bool _textReadonly;
    [ObservableProperty]
    private bool _text2Visable;
    [ObservableProperty]
    private bool _valueVisable;
    [ObservableProperty]
    private char _password;

    [RelayCommand]
    public void Cancel()
    {
        if (Call != null)
        {
            Call();
            CancelEnable = false;
            Call = null;
            return;
        }

        IsCancel = true;

        DialogHost.Close(name);
    }

    [RelayCommand]
    public void Confirm()
    {
        IsCancel = false;

        DialogHost.Close(name);
    }


}
