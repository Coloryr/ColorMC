using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 确认框
/// </summary>
/// <param name="name">窗口ID</param>
public partial class Info4Model(string? name) : ObservableObject
{
    /// <summary>
    /// 执行
    /// </summary>
    public Action<bool>? Call;
    /// <summary>
    /// 选择执行
    /// </summary>
    public Action? ChoiseCall;

    /// <summary>
    /// 显示的文本
    /// </summary>
    [ObservableProperty]
    private string _text;
    /// <summary>
    /// 是否启用默认按钮
    /// </summary>
    [ObservableProperty]
    private bool _enableButton;
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
    private string _choiseText;
    /// <summary>
    /// 是否显示选择按钮
    /// </summary>
    [ObservableProperty]
    private bool _choiseVisiable;

    [RelayCommand]
    public void Choise()
    {
        ChoiseCall?.Invoke();
    }

    [RelayCommand]
    public void Cancel()
    {
        EnableButton = false;
        Call?.Invoke(false);
        DialogHost.Close(name);
    }

    [RelayCommand]
    public void Confirm()
    {
        EnableButton = false;
        Call?.Invoke(true);
        DialogHost.Close(name);
    }
}
