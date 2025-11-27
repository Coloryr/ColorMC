using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 添加游戏分组
/// </summary>
/// <param name="window"></param>
/// <param name="group"></param>
public partial class AddGroupModel(WindowModel window, string? group) : ObservableObject
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

    /// <summary>
    /// 添加群组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddGroup()
    {
        var dialog = new InputModel(window.WindowId)
        {
            Watermark1 = LanguageUtils.Get("Text.Group")
        };
        var res = await window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(dialog.Text1))
        {
            window.Show(LanguageUtils.Get("MainWindow.Text82"));
            return;
        }

        if (!GameBinding.AddGameGroup(dialog.Text1))
        {
            window.Show(LanguageUtils.Get("MainWindow.Text83"));
            return;
        }

        GroupList.Add(dialog.Text1);
    }

    /// <summary>
    /// 确认
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(window.WindowId, true, this);
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(window.WindowId, false, this);
    }
}
