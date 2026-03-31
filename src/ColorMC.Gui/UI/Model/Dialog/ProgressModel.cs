using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 进度条信息
/// </summary>
public partial class ProgressModel : ObservableObject
{
    /// <summary>
    /// 显示的文本
    /// </summary>
    [ObservableProperty]
    public partial string Text { get; set; }

    /// <summary>
    /// 进度数值
    /// </summary>
    [ObservableProperty]
    public partial double Value { get; set; }

    /// <summary>
    /// 是否为循环滚动
    /// </summary>
    [ObservableProperty]
    public partial bool Indeterminate { get; set; } = true;
}
