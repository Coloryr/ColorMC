using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Input;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel
{
    public ObservableCollection<WorldModel> WorldList { get; init; } = [];

    private WorldModel? _selectWorld;


    [RelayCommand]
    public async Task BackupWorld()
    {
        var info = new DirectoryInfo(_obj.GetWorldBackupPath());
        if (!info.Exists)
        {
            info.Create();
        }

        var list = info.GetFiles();
        var names = new List<string>();
        foreach (var item in list)
        {
            names.Add(item.Name);
        }
        if (names.Count == 0)
        {
            Model.Show(App.Lang("GameEditWindow.Tab5.Error5"));
            return;
        }
        var (cancel, index, _) = await Model.ShowCombo(App.Lang("GameEditWindow.Tab5.Info9"), names);
        if (cancel)
            return;
        var item1 = list[index];
        var res1 = await Model.ShowWait(App.Lang("GameEditWindow.Tab5.Info10"));
        if (!res1)
            return;

        Model.Progress(App.Lang("GameEditWindow.Tab5.Info11"));
        res1 = await GameBinding.BackupWorld(_obj, item1, Model.ShowWait);
        Model.ProgressClose();
        if (!res1)
        {
            Model.Show(App.Lang("GameEditWindow.Tab5.Error4"));
        }
        else
        {
            Model.Notify(App.Lang("GameEditWindow.Tab5.Info12"));
            await LoadWorld();
        }
    }
    [RelayCommand]
    public async Task ImportWorld()
    {
        var file = await PathBinding.AddFile(_obj, FileType.World);
        if (file == null)
            return;

        if (file == false)
        {
            Model.Notify(App.Lang("GameEditWindow.Tab5.Error2"));
            return;
        }

        Model.Notify(App.Lang("GameEditWindow.Tab4.Info2"));
        await LoadWorld();
    }
    [RelayCommand]
    public void OpenWorld()
    {
        PathBinding.OpPath(_obj, PathType.SavePath);
    }
    [RelayCommand]
    public void AddWorld()
    {
        WindowManager.ShowAdd(_obj, FileType.World);
    }

    [RelayCommand]
    public void OpenBackupWorld()
    {
        PathBinding.OpPath(_obj, PathType.WorldBackPath);
    }
    [RelayCommand]
    public async Task LoadWorld()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab5.Info5"));
        WorldList.Clear();

        var res = await GameBinding.GetWorlds(_obj!);
        foreach (var item in res)
        {
            var item1 = new WorldModel(this, item);
            await item1.Load();
            WorldList.Add(item1);
        }

        Model.ProgressClose();
    }
    [RelayCommand]
    public async Task EditWorld()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab5.Info13"));
        var res = await ToolPath.OpenMapEditAsync();
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Message!);
        }
    }

    public async void DropWorld(IDataObject data)
    {
        var res = await GameBinding.AddFile(_obj, data, FileType.World);
        if (res)
        {
            await LoadWorld();
        }
    }

    public void SetSelectWorld(WorldModel item)
    {
        if (_selectWorld != null)
        {
            _selectWorld.IsSelect = false;
        }
        _selectWorld = item;
        _selectWorld.IsSelect = true;
    }

    public async void DeleteWorld(WorldModel obj)
    {
        var res = await Model.ShowWait(
            string.Format(App.Lang("GameEditWindow.Tab5.Info1"), obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteWorld(obj.World);
        Model.Notify(App.Lang("GameEditWindow.Tab4.Info3"));
        await LoadWorld();
    }

    public async void Export(WorldModel obj)
    {
        Model.Progress(App.Lang("GameEditWindow.Tab5.Info4"));
        var file = await PathBinding.SaveFile(FileType.World, [obj]);
        Model.ProgressClose();
        if (file == null)
            return;

        if (file == false)
        {
            Model.Show(App.Lang("GameEditWindow.Tab5.Error1"));
        }
        else
        {
            Model.Notify(App.Lang("GameEditWindow.Tab5.Info3"));
        }
    }

    public async void BackupWorld(WorldModel obj)
    {
        Model.Progress(App.Lang("GameEditWindow.Tab5.Info7"));
        var res = await GameBinding.BackupWorld(obj.World);
        Model.ProgressClose();
        if (res)
        {
            Model.Notify(App.Lang("GameEditWindow.Tab5.Info8"));
        }
        else
        {
            Model.Show(App.Lang("GameEditWindow.Tab5.Error3"));
        }
    }

    public async void LaunchWorld(WorldModel world)
    {
        if (BaseBinding.IsGameRun(world.World.Game))
        {
            return;
        }

        var res = await GameBinding.Launch(Model, world.World.Game, world.World, GuiConfigUtils.Config.CloseBeforeLaunch);
        if (!res.Item1)
        {
            Model.Show(res.Item2!);
        }
    }
}
