using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 进度条信息
/// </summary>
public partial class Info1Model : ObservableObject
{
    /// <summary>
    /// 显示的文本
    /// </summary>
    [ObservableProperty]
    private string _text;
    /// <summary>
    /// 进度数值
    /// </summary>
    [ObservableProperty]
    private double _value;
    /// <summary>
    /// 是否为循环滚动
    /// </summary>
    [ObservableProperty]
    private bool _indeterminate;
}
