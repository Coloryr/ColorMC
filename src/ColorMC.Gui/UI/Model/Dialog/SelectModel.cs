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
    public partial string Text { get; set; }

    /// <summary>
    /// 选择项
    /// </summary>
    [ObservableProperty]
    public partial string? Select { get; set; }

    /// <summary>
    /// 选择项
    /// </summary>
    [ObservableProperty]
    public partial int Index { get; set; }

    [ObservableProperty]
    public partial string? SelectText { get; set; }

    [ObservableProperty]
    public partial bool IsEdit { get; set; }

    /// <summary>
    /// 项目列表
    /// </summary>
    public ObservableCollection<string> Items { get; init; } = [];
}
