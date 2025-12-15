using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 文本显示
/// </summary>
/// <param name="name">窗口Id</param>
public partial class LongTextModel(string name) : BaseDialogModel(name)
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
}
