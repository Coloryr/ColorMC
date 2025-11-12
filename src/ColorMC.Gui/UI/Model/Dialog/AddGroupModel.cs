using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 添加游戏分组
/// </summary>
/// <param name="model"></param>
/// <param name="group"></param>
public partial class AddGroupModel(BaseModel model, string? group) : ObservableObject
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
        var res = await model.InputWithEditAsync(LanguageUtils.Get("Text.Group"), false);
        if (res.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            model.Show(LanguageUtils.Get("MainWindow.Text82"));
            return;
        }

        if (!GameBinding.AddGameGroup(res.Text1))
        {
            model.Show(LanguageUtils.Get("MainWindow.Text83"));
            return;
        }

        GroupList.Add(res.Text1);
    }

    /// <summary>
    /// 确认
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(MainControl.DialogName, true);
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(MainControl.DialogName, false);
    }
}
