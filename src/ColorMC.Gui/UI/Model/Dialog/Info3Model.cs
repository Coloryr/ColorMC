using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 输入框信息
/// </summary>
/// <param name="name">窗口ID</param>
public partial class Info3Model(string? name) : ObservableObject
{
    /// <summary>
    /// 执行
    /// </summary>
    public Action? Call;
    /// <summary>
    /// 选择执行
    /// </summary>
    public Action? ChoiseCall;

    /// <summary>
    /// 显示文本1
    /// </summary>
    [ObservableProperty]
    private string _text1;
    /// <summary>
    /// 显示文本2
    /// </summary>
    [ObservableProperty]
    private string _text2;
    /// <summary>
    /// 提示文本1
    /// </summary>
    [ObservableProperty]
    private string _watermark1;
    /// <summary>
    /// 提示文本2
    /// </summary>
    [ObservableProperty]
    private string _watermark2;
    /// <summary>
    /// 是否启用确认
    /// </summary>
    [ObservableProperty]
    private bool _confirmEnable;
    /// <summary>
    /// 是否启用取消
    /// </summary>
    [ObservableProperty]
    private bool _cancelEnable;
    /// <summary>
    /// 是否显示取消
    /// </summary>
    [ObservableProperty]
    private bool _cancelVisible;
    /// <summary>
    /// 文本是否只读
    /// </summary>
    [ObservableProperty]
    private bool _textReadonly;
    /// <summary>
    /// 文本2是否显示
    /// </summary>
    [ObservableProperty]
    private bool _text2Visable;
    /// <summary>
    /// 进度条是否显示
    /// </summary>
    [ObservableProperty]
    private bool _valueVisable;
    /// <summary>
    /// 是否密码
    /// </summary>
    [ObservableProperty]
    private char _password;
    /// <summary>
    /// 选择按钮是否显示
    /// </summary>
    [ObservableProperty]
    private bool _choiseVisible;
    /// <summary>
    /// 选择按钮文本
    /// </summary>
    [ObservableProperty]
    private string _choiseText;

    /// <summary>
    /// 选择按钮
    /// </summary>
    [RelayCommand]
    public void Choise()
    {
        ChoiseCall?.Invoke();
    }

    /// <summary>
    /// 取消按钮
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        if (Call != null)
        {
            Call();
            CancelEnable = false;
            Call = null;
            return;
        }
        DialogHost.Close(name, false);
    }

    /// <summary>
    /// 同意按钮
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(name, true);
    }
}
