using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

public abstract partial class BaseDialogModel(string window) : ObservableObject
{
    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(window, false, this);
    }
    /// <summary>
    /// 同意
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(window, true, this);
    }
}
