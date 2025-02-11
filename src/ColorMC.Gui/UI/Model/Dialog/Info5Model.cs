using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 下拉框
/// </summary>
/// <param name="name">窗口Id</param>
public partial class Info5Model(string? name) : ObservableObject
{
    /// <summary>
    /// 显示文本
    /// </summary>
    [ObservableProperty]
    private string _text;
    /// <summary>
    /// 选择项
    /// </summary>
    [ObservableProperty]
    private string _select;
    /// <summary>
    /// 选择项
    /// </summary>
    [ObservableProperty]
    private int _index;

    /// <summary>
    /// 是否取消
    /// </summary>
    public bool IsCancel;

    /// <summary>
    /// 项目列表
    /// </summary>
    public ObservableCollection<string> Items { get; init; } = [];

    [RelayCommand]
    public void Cancel()
    {
        IsCancel = true;

        DialogHost.Close(name);
    }

    [RelayCommand]
    public void Confirm()
    {
        IsCancel = false;

        DialogHost.Close(name);
    }
}
