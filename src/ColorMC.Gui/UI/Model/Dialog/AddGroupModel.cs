using System.Collections.ObjectModel;
using ColorMC.Core.LaunchPath;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 添加游戏分组
/// </summary>
/// <param name="window"></param>
/// <param name="group"></param>
public partial class GroupEditModel(WindowModel window, string? group) : BaseDialogModel(window.WindowId)
{
    /// <summary>
    /// 游戏分组列表
    /// </summary>
    public ObservableCollection<string> GroupList { get; init; } = [.. InstancesPath.GroupKeys];

    /// <summary>
    /// 选择的群组
    /// </summary>
    [ObservableProperty]
    private string? _groupItem = group;
}
