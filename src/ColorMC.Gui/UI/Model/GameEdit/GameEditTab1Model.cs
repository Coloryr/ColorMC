using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : GameModel
{
    public ObservableCollection<string> GameVersionList { get; init; } = new();
    public ObservableCollection<string> LoaderVersionList { get; init; } = new();
    public ObservableCollection<string> GroupList { get; init; } = new();
    public List<string> VersionTypeList { get; init; } = LanguageBinding.GetVersionType();
    public ObservableCollection<string> LoaderTypeList { get; init; } = new();

    private List<Loaders> _loaderTypeList = new();

    private bool _gameLoad = false;

    [ObservableProperty]
    private string _gameVersion;
    [ObservableProperty]
    private string? _loaderVersion;
    [ObservableProperty]
    private string? _group;
    [ObservableProperty]
    private string? _pID;
    [ObservableProperty]
    private string? _fID;

    [ObservableProperty]
    private int _versionType = -1;
    [ObservableProperty]
    private int _loaderType = -1;

    [ObservableProperty]
    private bool _modPack;

    [ObservableProperty]
    private bool _gameRun;

    [ObservableProperty]
    private bool _enableLoader;

    partial void OnVersionTypeChanged(int value)
    {
        GameVersionLoad();
    }

    async partial void OnLoaderTypeChanged(int value)
    {
        if (_gameLoad)
            return;

        var loader = _loaderTypeList[value];
        LoaderVersionList.Clear();
        switch (loader)
        {
            case Loaders.Normal:
                Obj.Loader = Loaders.Normal;
                Obj.LoaderVersion = null;
                Obj.Save();
                break;
            case Loaders.Forge:
                Obj.Loader = Loaders.Forge;
                Obj.LoaderVersion = null;
                Obj.Save();
                await LoaderVersionLoad();
                break;
            case Loaders.NeoForge:
                Obj.Loader = Loaders.NeoForge;
                Obj.LoaderVersion = null;
                Obj.Save();
                await LoaderVersionLoad();
                break;
            case Loaders.Fabric:
                Obj.Loader = Loaders.Fabric;
                Obj.LoaderVersion = null;
                Obj.Save();
                await LoaderVersionLoad();
                break;
            case Loaders.Quilt:
                Obj.Loader = Loaders.Quilt;
                Obj.LoaderVersion = null;
                Obj.Save();
                await LoaderVersionLoad();
                break;
        }

    }

    partial void OnGameVersionChanged(string value)
    {
        if (_gameLoad)
            return;

        LoaderVersion = null;
        LoaderVersionList.Clear();
        LoaderType = 0;

        Obj.Version = value;
        Obj.Loader = Loaders.Normal;
        Obj.LoaderVersion = null;
        Obj.Save();
    }

    partial void OnLoaderVersionChanged(string? value)
    {
        if (_gameLoad)
            return;

        Obj.LoaderVersion = value;
        Obj.Save();
    }

    partial void OnGroupChanged(string? value)
    {
        if (_gameLoad)
            return;

        GameBinding.MoveGameGroup(Obj, value);
    }

    partial void OnModPackChanged(bool value)
    {
        if (_gameLoad)
            return;

        Obj.ModPack = value;
        Obj.Save();
    }

    partial void OnPIDChanged(string? value)
    {
        if (_gameLoad)
            return;

        Obj.PID = value;
        Obj.Save();
    }

    partial void OnFIDChanged(string? value)
    {
        if (_gameLoad)
            return;

        Obj.FID = value;
        Obj.Save();
    }

    [RelayCommand]
    public void ExportGame()
    {
        App.ShowGameExport(Obj);
    }

    [RelayCommand]
    public async Task CheckModPackUpdate()
    {
        if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
        {
            Show(App.GetLanguage("GameEditWindow.Tab1.Error3"));
            return;
        }

        Progress(App.GetLanguage("GameEditWindow.Tab1.Info2"));
        if (FuntionUtils.CheckNotNumber(PID) || FuntionUtils.CheckNotNumber(FID))
        {
            var list = await ModrinthAPI.GetFileVersions(PID, Obj.Version, Obj.Loader);
            ProgressClose();
            if (list == null)
            {
                Show(App.GetLanguage("GameEditWindow.Tab1.Info3"));
            }
            else if (list.Count == 0)
            {
                Notify(App.GetLanguage("GameEditWindow.Tab1.Info4"));
            }
            else if (list[0].id.ToString() == FID)
            {
                Notify(App.GetLanguage("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await ShowWait(App.GetLanguage("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                Progress(App.GetLanguage("GameEditWindow.Tab1.Info8"));
                var item = list[0];
                res = await GameBinding.ModPackUpdate(Obj, item);
                ProgressClose();
                if (!res)
                {
                    Show(App.GetLanguage("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    Notify(App.GetLanguage("GameEditWindow.Tab1.Info7"));
                    FID = item.id.ToString();
                }
            }
        }
        else
        {
            var list = await CurseForgeAPI.GetCurseForgeFiles(PID, Obj.Version);
            ProgressClose();
            if (list == null)
            {
                Show(App.GetLanguage("GameEditWindow.Tab1.Info3"));
            }
            else if (list.data.Count == 0)
            {
                Notify(App.GetLanguage("GameEditWindow.Tab1.Info4"));
            }
            else if (list.data[0].id.ToString() == FID)
            {
                Notify(App.GetLanguage("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await ShowWait(App.GetLanguage("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                Progress(App.GetLanguage("GameEditWindow.Tab1.Info8"));
                var item = list.data[0];
                res = await GameBinding.ModPackUpdate(Obj, item);
                ProgressClose();
                if (!res)
                {
                    Show(App.GetLanguage("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    Notify(App.GetLanguage("GameEditWindow.Tab1.Info7"));
                    FID = item.id.ToString();
                }
            }
        }
    }

    [RelayCommand]
    public void OpenGameLog()
    {
        App.ShowGameLog(Obj);
    }

    [RelayCommand]
    public async Task AddGroup()
    {
        var (Cancel, Text1) = await ShowOne(App.GetLanguage("AddGameWindow.Tab1.Info5"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text1))
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(Text1))
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Error3"));
            return;
        }

        Notify(App.GetLanguage("AddGameWindow.Tab1.Info6"));

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    [RelayCommand]
    public async Task LoaderVersionLoad()
    {
        EnableLoader = false;

        var loader = _loaderTypeList[LoaderType];
        switch (loader)
        {
            case Loaders.Normal:
                LoaderVersionList.Clear();
                break;
            case Loaders.Forge:
                Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
                var list = await GameBinding.GetForgeVersion(Obj.Version);
                ProgressClose();
                if (list == null)
                {
                    Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.NeoForge:
                Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
                list = await GameBinding.GetNeoForgeVersion(Obj.Version);
                ProgressClose();
                if (list == null)
                {
                    Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.Fabric:
                Progress(App.GetLanguage("AddGameWindow.Tab1.Info2"));
                list = await GameBinding.GetFabricVersion(Obj.Version);
                ProgressClose();
                if (list == null)
                {
                    Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.Quilt:
                Progress(App.GetLanguage("AddGameWindow.Tab1.Info3"));
                list = await GameBinding.GetQuiltVersion(Obj.Version);
                ProgressClose();
                if (list == null)
                {
                    Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
        }
    }

    [RelayCommand]
    public async Task LoaderReload()
    {
        _gameLoad = true;

        _loaderTypeList.Clear();
        LoaderTypeList.Clear();
        _loaderTypeList.Add(Loaders.Normal);
        LoaderTypeList.Add(Loaders.Normal.GetName());
        Progress(App.GetLanguage("AddGameWindow.Tab1.Info4"));
        var list = await GameBinding.GetForgeSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            _loaderTypeList.Add(Loaders.Forge);
            LoaderTypeList.Add(Loaders.Forge.GetName());
        }

        list = await GameBinding.GetNeoForgeSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            _loaderTypeList.Add(Loaders.NeoForge);
            LoaderTypeList.Add(Loaders.NeoForge.GetName());
        }

        list = await GameBinding.GetFabricSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            _loaderTypeList.Add(Loaders.Fabric);
            LoaderTypeList.Add(Loaders.Fabric.GetName());
        }

        list = await GameBinding.GetQuiltSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            _loaderTypeList.Add(Loaders.Quilt);
            LoaderTypeList.Add(Loaders.Quilt.GetName());
        }
        ProgressClose();

        _gameLoad = false;
    }

    [RelayCommand]
    public async Task GameVersionReload()
    {
        Progress(App.GetLanguage("GameEditWindow.Info1"));
        var res = await GameBinding.ReloadVersion();
        ProgressClose();
        if (!res)
        {
            Show(App.GetLanguage("GameEditWindow.Error1"));
            return;
        }

        LoaderVersion = null;
        LoaderType = 0;

        GameVersionLoad();
    }

    [RelayCommand]
    public async Task Delete()
    {
        if (BaseBinding.IsGameRun(Obj))
        {
            Show(App.GetLanguage("GameEditWindow.Tab1.Error1"));
            return;
        }

        var res = await ShowWait(string.Format(
            App.GetLanguage("GameEditWindow.Tab1.Info1"), Obj.Name));
        if (!res)
            return;

        var res1 = await GameBinding.DeleteGame(Obj);
        if (!res1)
        {
            Show(App.GetLanguage("MainWindow.Info37"));
        }
    }

    [RelayCommand]
    public void Open()
    {
        PathBinding.OpPath(Obj, PathType.BasePath);
    }

    [RelayCommand]
    public void OpenOptifine()
    {
        App.ShowAdd(Obj, FileType.Optifne);
    }

    [RelayCommand]
    public void OpenServerPack()
    {
        App.ShowServerPack(Obj);
    }

    [RelayCommand]
    public void OpenConfigEdit()
    {
        App.ShowConfigEdit(Obj);
    }

    private async void GameVersionLoad()
    {
        GameVersionList.Clear();
        switch (VersionType)
        {
            case 0:
                Obj.GameType = GameType.Release;
                GameVersionList.AddRange(await GameBinding.GetGameVersion(true, false, false));
                break;
            case 1:
                Obj.GameType = GameType.Snapshot;
                GameVersionList.AddRange(await GameBinding.GetGameVersion(false, true, false));
                break;
            case 2:
                Obj.GameType = GameType.Other;
                GameVersionList.AddRange(await GameBinding.GetGameVersion(false, false, true));
                break;
        }
    }

    private void GroupLoad()
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    public void GameStateChange()
    {
        GameRun = BaseBinding.IsGameRun(Obj);
    }

    public void GameLoad()
    {
        _gameLoad = true;

        if (Obj.GameType == GameType.Snapshot)
        {
            VersionType = 2;
        }
        else if (Obj.GameType == GameType.Other)
        {
            VersionType = 3;
        }
        else
        {
            VersionType = 0;
        }

        _loaderTypeList.Add(Loaders.Normal);
        LoaderTypeList.Add(Loaders.Normal.GetName());

        if (Obj.Loader != Loaders.Normal)
        {
            _loaderTypeList.Add(Obj.Loader);
            LoaderTypeList.Add(Obj.Loader.GetName());

            LoaderType = 1;

            EnableLoader = false;
            LoaderVersionList.Clear();
            if (Obj.LoaderVersion != null)
            {
                LoaderVersionList.Add(Obj.LoaderVersion);
            }
        }
        else
        {
            LoaderType = 0;
        }

        GameVersionLoad();
        GroupLoad();

        LoaderVersion = Obj.LoaderVersion;
        ModPack = Obj.ModPack;
        GameVersion = Obj.Version;
        Group = Obj.GroupName;
        FID = Obj.FID;
        PID = Obj.PID;

        GameRun = BaseBinding.IsGameRun(Obj);

        _gameLoad = false;
    }
}
