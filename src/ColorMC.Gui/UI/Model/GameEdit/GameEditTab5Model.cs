using Avalonia.Input;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab5Model : GameModel, ILoadFuntion<WorldModel>
{
    public ObservableCollection<WorldModel> WorldList { get; init; } = new();

    private WorldModel? _last;

    public GameEditTab5Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public async Task Backup()
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
        var (cancel, index, _) = await ShowCombo(App.GetLanguage("GameEditWindow.Tab5.Info9"), names);
        if (cancel)
            return;
        var item1 = list[index];
        var res1 = await ShowWait(App.GetLanguage("GameEditWindow.Tab5.Info10"));
        if (!res1)
            return;

        Progress(App.GetLanguage("GameEditWindow.Tab5.Info11"));
        res1 = await GameBinding.BackupWorld(Obj, item1);
        ProgressClose();
        if (!res1)
        {
            Show(App.GetLanguage("GameEditWindow.Tab5.Error4"));
        }
        else
        {
            Notify(App.GetLanguage("GameEditWindow.Tab5.Info12"));
            await Load();
        }
    }
    [RelayCommand]
    public async Task Import()
    {
        var file = await PathBinding.AddFile(Window, Obj, FileType.World);
        if (file == null)
            return;

        if (file == false)
        {
            Notify(App.GetLanguage("GameEditWindow.Tab5.Error2"));
            return;
        }

        Notify(App.GetLanguage("GameEditWindow.Tab4.Info2"));
        await Load();
    }
    [RelayCommand]
    public void Open()
    {
        PathBinding.OpPath(Obj, PathType.SavePath);
    }
    [RelayCommand]
    public void Add()
    {
        App.ShowAdd(Obj, FileType.World);
    }

    [RelayCommand]
    public void OpenBackup()
    {
        PathBinding.OpPath(Obj, PathType.WorldBackPath);
    }
    [RelayCommand]
    public async Task Load()
    {
        Progress(App.GetLanguage("GameEditWindow.Tab5.Info5"));
        WorldList.Clear();

        var res = await GameBinding.GetWorlds(Obj!);
        ProgressClose();
        foreach (var item in res)
        {
            WorldList.Add(new(Control, this, item));
        }
    }
    [RelayCommand]
    public async Task EditWorld()
    {
        Progress(App.GetLanguage("GameEditWindow.Tab5.Info13"));
        var res = await ToolPath.OpenMapEdit();
        ProgressClose();
        if (!res.Item1)
        {
            Show(res.Item2!);
        }
    }

    public async void Drop(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.World);
        if (res)
        {
            await Load();
        }
    }

    public void SetSelect(WorldModel item)
    {
        if (_last != null)
        {
            _last.IsSelect = false;
        }
        _last = item;
        _last.IsSelect = true;
    }
}
