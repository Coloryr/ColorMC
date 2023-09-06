using Avalonia.Input;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : GameModel
{
    public ObservableCollection<WorldModel> WorldList { get; init; } = new();

    private WorldModel? _selectWorld;


    [RelayCommand]
    public async Task BackupWorld()
    {
        var info = new DirectoryInfo(Obj.GetWorldBackupPath());
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
        var (cancel, index, _) = await Model.ShowCombo(App.GetLanguage("GameEditWindow.Tab5.Info9"), names);
        if (cancel)
            return;
        var item1 = list[index];
        var res1 = await Model.ShowWait(App.GetLanguage("GameEditWindow.Tab5.Info10"));
        if (!res1)
            return;

        Model.Progress(App.GetLanguage("GameEditWindow.Tab5.Info11"));
        res1 = await GameBinding.BackupWorld(Obj, item1);
        Model.ProgressClose();
        if (!res1)
        {
            Model.Show(App.GetLanguage("GameEditWindow.Tab5.Error4"));
        }
        else
        {
            Model.Notify(App.GetLanguage("GameEditWindow.Tab5.Info12"));
            await LoadWorld();
        }
    }
    [RelayCommand]
    public async Task ImportWorld()
    {
        var file = await PathBinding.AddFile(Obj, FileType.World);
        if (file == null)
            return;

        if (file == false)
        {
            Model.Notify(App.GetLanguage("GameEditWindow.Tab5.Error2"));
            return;
        }

        Model.Notify(App.GetLanguage("GameEditWindow.Tab4.Info2"));
        await LoadWorld();
    }
    [RelayCommand]
    public void OpenWorld()
    {
        PathBinding.OpPath(Obj, PathType.SavePath);
    }
    [RelayCommand]
    public void AddWorld()
    {
        App.ShowAdd(Obj, FileType.World);
    }

    [RelayCommand]
    public void OpenBackupWorld()
    {
        PathBinding.OpPath(Obj, PathType.WorldBackPath);
    }
    [RelayCommand]
    public async Task LoadWorld()
    {
        Model.Progress(App.GetLanguage("GameEditWindow.Tab5.Info5"));
        WorldList.Clear();

        var res = await GameBinding.GetWorlds(Obj!);
        Model.ProgressClose();
        foreach (var item in res)
        {
            WorldList.Add(new(this, item));
        }
    }
    [RelayCommand]
    public async Task EditWorld()
    {
        Model.Progress(App.GetLanguage("GameEditWindow.Tab5.Info13"));
        var res = await ToolPath.OpenMapEdit();
        Model.ProgressClose();
        if (!res.Item1)
        {
            Model.Show(res.Item2!);
        }
    }

    public async void DropWorld(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.World);
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
            string.Format(App.GetLanguage("GameEditWindow.Tab5.Info1"), obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteWorld(obj.World);
        Model.Notify(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        await LoadWorld();
    }

    public async void Export(WorldModel obj)
    {
        Model.Progress(App.GetLanguage("GameEditWindow.Tab5.Info4"));
        var file = await PathBinding.SaveFile(FileType.World, new object[]
            { obj });
        Model.ProgressClose();
        if (file == null)
            return;

        if (file == false)
        {
            Model.Show(App.GetLanguage("GameEditWindow.Tab5.Error1"));
        }
        else
        {
            Model.Notify(App.GetLanguage("GameEditWindow.Tab5.Info3"));
        }
    }

    public async void BackupWorld(WorldModel obj)
    {
        Model.Progress(App.GetLanguage("GameEditWindow.Tab5.Info7"));
        var res = await GameBinding.BackupWorld(obj.World);
        Model.ProgressClose();
        if (res)
        {
            Model.Notify(App.GetLanguage("GameEditWindow.Tab5.Info8"));
        }
        else
        {
            Model.Show(App.GetLanguage("GameEditWindow.Tab5.Error3"));
        }
    }

    public async void LaunchWorld(WorldModel world)
    {
        if (BaseBinding.IsGameRun(world.World.Game))
        {
            return;
        }

        var res = await GameBinding.Launch(Model, world.World.Game, world.World);
        if (!res.Item1)
        {
            Model.Show(res.Item2!);
        }
    }
}
