using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 下拉框
/// </summary>
/// <param name="name">窗口Id</param>
public partial class SelectModel(string name) : BaseDialogModel(name)
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
    private string? _select;
    /// <summary>
    /// 选择项
    /// </summary>
    [ObservableProperty]
    private int _index;

    /// <summary>
    /// 项目列表
    /// </summary>
    public ObservableCollection<string> Items { get; init; } = [];
}
