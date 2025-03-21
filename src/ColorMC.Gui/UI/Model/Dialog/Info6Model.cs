using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 文本显示
/// </summary>
/// <param name="name">窗口Id</param>
public partial class Info6Model(string? name) : ObservableObject
{
    /// <summary>
    /// 文本1
    /// </summary>
    [ObservableProperty]
    private string _text1;
    /// <summary>
    /// 文本2
    /// </summary>
    [ObservableProperty]
    private string _text2;

    /// <summary>
    /// 是否启用取消
    /// </summary>
    [ObservableProperty]
    private bool _cancelEnable;

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(name, false);
    }

    /// <summary>
    /// 同意
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(name, true);
    }
}
