using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 确认框
/// </summary>
/// <param name="name">窗口ID</param>
public partial class ChoiceModel(string name) : BaseDialogModel(name)
{
    /// <summary>
    /// 选择执行
    /// </summary>
    public Action? ChoiceCall;

    /// <summary>
    /// 显示的文本
    /// </summary>
    [ObservableProperty]
    public partial string Text { get; set; }

    /// <summary>
    /// 是否启用默认按钮
    /// </summary>
    [ObservableProperty]
    public partial bool EnableButton { get; set; } = true;

    /// <summary>
    /// 是否显示取消按钮
    /// </summary>
    [ObservableProperty]
    public partial bool CancelVisable { get; set; }

    /// <summary>
    /// 是否显示确认按钮
    /// </summary>
    [ObservableProperty]
    public partial bool ConfirmVisable { get; set; } = true;

    /// <summary>
    /// 选择按钮文本
    /// </summary>
    [ObservableProperty]
    public partial string ChoiceText { get; set; }

    /// <summary>
    /// 是否显示选择按钮
    /// </summary>
    [ObservableProperty]
    public partial bool ChoiceVisiable { get; set; }

    [RelayCommand]
    public void Choice()
    {
        ChoiceCall?.Invoke();
    }
}
