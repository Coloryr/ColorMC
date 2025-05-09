using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.BuildPack;

/// <summary>
/// 导出客户端
/// 游戏实例列表
/// </summary>
public partial class BuildPackModel
{
    /// <summary>
    /// 文件列表
    /// </summary>
    [ObservableProperty]
    private HierarchicalTreeDataGridSource<GameFileTreeNodeModel> _games;

    private GamesPage _gamesPage;

    /// <summary>
    /// 加载游戏列表
    /// </summary>
    private async void LoadGames()
    {
        Model.Progress(App.Lang("UserWindow.Info1"));
        await Task.Run(() =>
        {
            _gamesPage = new();
        });
        Model.ProgressClose();
        Games = _gamesPage.Source;
    }

    /// <summary>
    /// 获取选择的游戏文件
    /// </summary>
    /// <param name="getdir"></param>
    /// <returns></returns>
    public List<string> GetSelectItems()
    {
        return _gamesPage.GetSelectItems();
    }
}
