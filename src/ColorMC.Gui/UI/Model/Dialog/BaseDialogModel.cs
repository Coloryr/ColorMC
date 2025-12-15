using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

public abstract partial class BaseDialogModel(string name) : ObservableObject
{
    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(name, false, this);
    }
    /// <summary>
    /// 同意
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(name, true, this);
    }
}
