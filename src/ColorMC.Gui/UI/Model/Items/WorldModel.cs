using Avalonia.Media.Imaging;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

public partial class WorldModel : ObservableObject
{
    public readonly WorldObj World;
    public readonly GameEditModel Top;

    [ObservableProperty]
    private bool _isSelect;
    [ObservableProperty]
    private bool _empty;
    [ObservableProperty]
    private DataPackModel _dataPack;

    public string Name => World.LevelName;
    public string Mode => LanguageHelper.GetNameWithGameType(World.GameType);
    public string Time => FuntionUtils.MillisecondsToDataTime(World.LastPlayed).ToString();
    public string Local => World.Local;
    public string Difficulty => LanguageHelper.GetNameWithDifficulty(World.Difficulty);
    public string Hardcore => World.Hardcore == 1 ? "True" : "False";
    public Bitmap Pic { get; }

    public ObservableCollection<DataPackModel> DataPackList { get; init; } = [];

    public WorldModel(GameEditModel top, WorldObj world)
    {
        Top = top;
        World = world;
        Pic = World.Icon != null ? new Bitmap(World.Icon) : App.GameIcon;
    }

    public void Close()
    {
        if (Pic != App.GameIcon)
        {
            Pic.Dispose();
        }
    }

    public async Task Load()
    {
        DataPackList.Clear();
        var list = await Task.Run(() => World.GetDataPacks());
        foreach (var item in list)
        {
            DataPackList.Add(new(item));
        }
    }

    private async void Load1()
    {
        Top.Model.Progress(App.Lang("GameEditWindow.Tab5.Info16"));
        IsSelect = false;
        await Load();
        IsSelect = true;
        Top.Model.ProgressClose();
    }

    public void Select()
    {
        Top.SetSelectWorld(this);
    }

    public void DisE()
    {
        if (DataPack != null)
        {
            DisE(DataPack);
        }
    }

    public async void DisE(DataPackModel pack)
    {
        var res = await Task.Run(() => GameBinding.DataPackDisE(pack.Pack));
        if (res)
        {
            Load1();
        }
    }

    public async void DisE(IEnumerable<DataPackModel> pack)
    {
        var res = await Task.Run(() => GameBinding.DataPackDisE(pack));
        if (res)
        {
            Load1();
        }
    }

    public async void Delete(DataPackModel item)
    {
        var res = await Top.Model.ShowWait(
            string.Format(App.Lang("GameEditWindow.Tab5.Info15"), item.Name));
        if (!res)
        {
            return;
        }

        InfoBinding.Window = Top.Model;

        res = await GameBinding.DeleteDataPack(item);
        if (res)
        {
            Load1();
        }
    }

    public async void Delete(IEnumerable<DataPackModel> items)
    {
        var res = await Top.Model.ShowWait(
            string.Format(App.Lang("GameEditWindow.Tab5.Info14"), items.Count()));
        if (!res)
        {
            return;
        }

        res = await GameBinding.DeleteDataPack(items);
        if (res)
        {
            Load1();
        }
    }
}
