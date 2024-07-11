using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class AddLockLoginModel : ObservableObject
{
    public string[] Items => UserBinding.GetLockLoginType();

    [ObservableProperty]
    private int _index;

    [ObservableProperty]
    private bool _enableInput;

    [ObservableProperty]
    private string _inputText;
    [ObservableProperty]
    private string _inputText1;

    partial void OnIndexChanged(int value)
    {
        if (value == 0)
        {
            EnableInput = false;
            InputText = "";
            InputText1 = "";
        }
        else
        {
            EnableInput = true;
            InputText1 = "";
        }
    }

    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close("AddLockLogin", true);
    }

    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close("AddLockLogin", false);
    }
}
