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

public partial class Info3Model : ObservableObject
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

    private readonly string? _name;

    public Info3Model(string? name)
    {
        _name = name;
    }

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

        DialogHost.Close(_name);
    }

    [RelayCommand]
    public void Confirm()
    {
        IsCancel = false;

        DialogHost.Close(_name);
    }


}
