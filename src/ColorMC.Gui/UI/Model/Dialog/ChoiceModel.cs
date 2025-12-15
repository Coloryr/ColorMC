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
    private string _text;
    /// <summary>
    /// 是否启用默认按钮
    /// </summary>
    [ObservableProperty]
    private bool _enableButton = true;
    /// <summary>
    /// 是否显示取消按钮
    /// </summary>
    [ObservableProperty]
    private bool _cancelVisable;
    /// <summary>
    /// 是否显示确认按钮
    /// </summary>
    [ObservableProperty]
    private bool _confirmVisable = true;

    /// <summary>
    /// 选择按钮文本
    /// </summary>
    [ObservableProperty]
    private string _choiceText;
    /// <summary>
    /// 是否显示选择按钮
    /// </summary>
    [ObservableProperty]
    private bool _choiceVisiable;

    [RelayCommand]
    public void Choice()
    {
        ChoiceCall?.Invoke();
    }
}
