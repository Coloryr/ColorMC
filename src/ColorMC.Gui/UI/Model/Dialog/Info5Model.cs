using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class Info5Model(string? name) : ObservableObject
{

    [ObservableProperty]
    private string _text;
    [ObservableProperty]
    private string _select;
    [ObservableProperty]
    private int _index;

    public bool IsCancel;

    public ObservableCollection<string> Items { get; init; } = [];

    [RelayCommand]
    public void Cancel()
    {
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
