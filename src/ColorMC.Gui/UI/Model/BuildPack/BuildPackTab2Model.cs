using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.BuildPack;

public partial class BuildPackModel
{
    /// <summary>
    /// 文件列表
    /// </summary>
    [ObservableProperty]
    private HierarchicalTreeDataGridSource<GameFileTreeNodeModel> _games;

    private GamesPage _gamesPage;

    public void LoadGames()
    {
        _gamesPage = new(false);
        Games = _gamesPage.Source;
    }
}
