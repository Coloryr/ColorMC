using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 输入框信息
/// </summary>
/// <param name="window">窗口ID</param>
public partial class InputModel(string window) : BaseDialogModel(window)
{
    /// <summary>
    /// 选择执行
    /// </summary>
    public Action? ChoiseCall;

    /// <summary>
    /// 显示文本1
    /// </summary>
    [ObservableProperty]
    public partial string Text1 { get; set; }

    /// <summary>
    /// 显示文本2
    /// </summary>
    [ObservableProperty]
    public partial string Text2 { get; set; }

    /// <summary>
    /// 提示文本1
    /// </summary>
    [ObservableProperty]
    public partial string Watermark1 { get; set; }

    /// <summary>
    /// 提示文本2
    /// </summary>
    [ObservableProperty]
    public partial string Watermark2 { get; set; }

    /// <summary>
    /// 是否启用确认
    /// </summary>
    [ObservableProperty]
    public partial bool ConfirmEnable { get; set; } = true;

    /// <summary>
    /// 是否启用取消
    /// </summary>
    [ObservableProperty]
    public partial bool CancelEnable { get; set; } = true;

    /// <summary>
    /// 是否显示取消
    /// </summary>
    [ObservableProperty]
    public partial bool CancelVisible { get; set; } = true;

    /// <summary>
    /// 文本是否只读
    /// </summary>
    [ObservableProperty]
    public partial bool TextReadonly { get; set; }

    /// <summary>
    /// 文本2是否显示
    /// </summary>
    [ObservableProperty]
    public partial bool Text2Visable { get; set; }

    /// <summary>
    /// 进度条是否显示
    /// </summary>
    [ObservableProperty]
    public partial bool ValueVisable { get; set; }

    /// <summary>
    /// 是否密码
    /// </summary>
    [ObservableProperty]
    public partial char? Password { get; set; }

    /// <summary>
    /// 选择按钮是否显示
    /// </summary>
    [ObservableProperty]
    public partial bool ChoiseVisible { get; set; }

    /// <summary>
    /// 选择按钮文本
    /// </summary>
    [ObservableProperty]
    public partial string ChoiseText { get; set; }

    /// <summary>
    /// 选择按钮
    /// </summary>
    [RelayCommand]
    public void Choise()
    {
        ChoiseCall?.Invoke();
    }
}
