using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class Info6Model(string? name) : ObservableObject
{
    public bool IsCancel;

    [ObservableProperty]
    private string _text1;
    [ObservableProperty]
    private string _text2;

    [ObservableProperty]
    private bool _needCancel;

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
