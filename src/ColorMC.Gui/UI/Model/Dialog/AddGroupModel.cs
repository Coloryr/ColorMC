using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UIBinding;
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
    public string[] GroupList { get; init; } = [.. GameBinding.GetGameGroups().Keys];

    /// <summary>
    /// 选择的群组
    /// </summary>
    [ObservableProperty]
    private string? _groupItem = group;

    public bool IsCancel;

    /// <summary>
    /// 添加群组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddGroup()
    {
        var res = await model.InputWithEditAsync(App.Lang("Text.Group"), false);
        if (res.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            model.Show(App.Lang("MainWindow.Error3"));
            return;
        }

        if (!GameBinding.AddGameGroup(res.Text1))
        {
            model.Show(App.Lang("MainWindow.Error4"));
            return;
        }
    }

    /// <summary>
    /// 确认
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        IsCancel = false;
        DialogHost.Close(MainControl.DialogName);
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        IsCancel = true;
        DialogHost.Close(MainControl.DialogName);
    }
}
