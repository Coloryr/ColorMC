using CommunityToolkit.Mvvm.ComponentModel;

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
    public partial string Text1 { get; set; }

    /// <summary>
    /// 文本2
    /// </summary>
    [ObservableProperty]
    public partial string Text2 { get; set; }

    /// <summary>
    /// 是否启用取消
    /// </summary>
    [ObservableProperty]
    public partial bool CancelEnable { get; set; }
}
