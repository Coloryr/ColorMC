using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 存档显示
/// </summary>
public partial class WorldModel : SelectItemModel
{
    /// <summary>
    /// 存档
    /// </summary>
    public readonly WorldObj World;
    /// <summary>
    /// 游戏设置
    /// </summary>
    public readonly GameEditModel TopModel;

    /// <summary>
    /// 数据包是否为空
    /// </summary>
    [ObservableProperty]
    private bool _empty;
    /// <summary>
    /// 选择的数据包
    /// </summary>
    [ObservableProperty]
    private DataPackModel _dataPack;

    /// <summary>
    /// 存档名字
    /// </summary>
    public string Name => World.LevelName;
    /// <summary>
    /// 生存模式
    /// </summary>
    public string Mode => LanguageHelper.GetNameWithGameType(World.GameType);
    /// <summary>
    /// 上次游玩时间
    /// </summary>
    public string Time => FuntionUtils.MillisecondsToDataTime(World.LastPlayed).ToString();
    /// <summary>
    /// 存档位置
    /// </summary>
    public string Local => World.Local;
    /// <summary>
    /// 难度
    /// </summary>
    public string Difficulty => LanguageHelper.GetNameWithDifficulty(World.Difficulty);
    /// <summary>
    /// 是否为极限模式
    /// </summary>
    public string Hardcore => World.Hardcore == 1 ? "True" : "False";
    /// <summary>
    /// 存档图标
    /// </summary>
    public Bitmap Pic { get; }

    /// <summary>
    /// 数据包列表
    /// </summary>
    public ObservableCollection<DataPackModel> DataPackList { get; init; } = [];

    public WorldModel(GameEditModel top, WorldObj world)
    {
        TopModel = top;
        World = world;
        Pic = World.Icon != null ? new Bitmap(World.Icon) : ImageManager.GameIcon;
    }

    /// <summary>
    /// 清理图标
    /// </summary>
    public void Close()
    {
        if (Pic != ImageManager.GameIcon)
        {
            Pic.Dispose();
        }
    }

    /// <summary>
    /// 加载数据包列表
    /// </summary>
    /// <returns></returns>
    public async Task Load()
    {
        DataPackList.Clear();
        var list = await GameBinding.GetWorldDataPackAsync(World);
        foreach (var item in list)
        {
            DataPackList.Add(new(item));
        }
        Empty = DataPackList.Count == 0;
    }

    /// <summary>
    /// 加载
    /// </summary>
    private async void LoadList()
    {
        TopModel.Model.Progress(App.Lang("GameEditWindow.Tab5.Info16"));
        IsSelect = false;
        await Load();
        IsSelect = true;
        TopModel.Model.ProgressClose();
    }

    /// <summary>
    /// 选中这个存档
    /// </summary>
    public void Select()
    {
        TopModel.SetSelectWorld(this);
    }

    /// <summary>
    /// 禁用/启用选中的数据包
    /// </summary>
    public void DisE()
    {
        if (DataPack != null)
        {
            DisE(DataPack);
        }
    }

    /// <summary>
    /// 禁用/启用选中的数据包
    /// </summary>
    /// <param name="pack"></param>
    public async void DisE(DataPackModel pack)
    {
        var res = await Task.Run(() => GameBinding.DataPackDisableOrEnable(pack.Pack));
        if (res)
        {
            LoadList();
        }
    }

    /// <summary>
    /// 禁用/启用选中的数据包
    /// </summary>
    /// <param name="pack"></param>
    public async void DisE(IEnumerable<DataPackModel> pack)
    {
        var res = await Task.Run(() => GameBinding.DataPackDisE(pack));
        if (res)
        {
            LoadList();
        }
    }

    /// <summary>
    /// 删除选中的数据包
    /// </summary>
    /// <param name="item"></param>
    public async void Delete(DataPackModel item)
    {
        var res = await TopModel.Model.ShowAsync(
            string.Format(App.Lang("GameEditWindow.Tab5.Info15"), item.Name));
        if (!res)
        {
            return;
        }

        res = await GameBinding.DeleteDataPack(item, TopModel.Model.ShowAsync);
        if (res)
        {
            LoadList();
        }
    }

    /// <summary>
    /// 删除选中的数据包
    /// </summary>
    /// <param name="items"></param>
    public async void Delete(IEnumerable<DataPackModel> items)
    {
        var res = await TopModel.Model.ShowAsync(
            string.Format(App.Lang("GameEditWindow.Tab5.Info14"), items.Count()));
        if (!res)
        {
            return;
        }

        res = await GameBinding.DeleteDataPack(items, TopModel.Model.ShowAsync);
        if (res)
        {
            LoadList();
        }
    }
}
