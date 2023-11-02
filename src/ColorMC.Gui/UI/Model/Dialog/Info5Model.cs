using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class Info5Model : ObservableObject
{

    [ObservableProperty]
    private string _text;
    [ObservableProperty]
    private string _select;
    [ObservableProperty]
    private int _index;

    public bool IsCancel;

    public ObservableCollection<string> Items { get; init; } = new();

    private readonly string? _name;

    public Info5Model(string? name)
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
