using System.Collections.ObjectModel;
using System.Linq;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.LuckBlock;

/// <summary>
/// 方块背包界面
/// </summary>
public partial class BlockBackpackModel : TopModel, IBlockTop
{
    /// <summary>
    /// 方块列表
    /// </summary>
    public ObservableCollection<BlockItemModel> Blocks { get; init; } = [];

    /// <summary>
    /// 是否没有方块列表
    /// </summary>
    [ObservableProperty]
    private bool _isEmpty;

    public BlockBackpackModel(BaseModel model) : base(model)
    {

    }

    [RelayCommand]
    public void OpenLuck()
    {
        WindowManager.ShowLuck();
    }

    /// <summary>
    /// 加载方块列表
    /// </summary>
    public async void Load()
    {
        Blocks.Clear();
        Model.Progress(App.Lang("LuckBlockWindow.Info1"));
        var res = await BaseBinding.StartLoadBlock();
        Model.ProgressClose();
        if (!res.State)
        {
            Model.ShowWithOk(res.Data!, Close);
            return;
        }

        var list = await BaseBinding.BuildUnlockItems();
        if (list == null)
        {
            Model.ShowWithOk(App.Lang("LuckBlockWindow.Error4"), Close);
            return;
        }

        foreach (var item in list)
        {
            item.Top = this;
            Blocks.Add(item);
        }

        IsEmpty = !Blocks.Any();
    }

    public override void Close()
    {
        Blocks.Clear();
    }

    /// <summary>
    /// 右键使用改方块
    /// </summary>
    /// <param name="model">方块</param>
    public async void Use(BlockItemModel model)
    {
        var list = InstancesPath.Games;
        var names = list.Select(item => item.Name);
        var res = await Model.ShowCombo(App.Lang("BlockBackpackWindow.Info1"), names);
        if (res.Cancel || res.Index == -1)
        {
            return;
        }

        var game = list[res.Index];
        GameBinding.SetGameIconBlock(game, model.Key);
    }
}
