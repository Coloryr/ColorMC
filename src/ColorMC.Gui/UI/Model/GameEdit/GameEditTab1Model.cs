using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : MenuModel
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
                _obj.Loader = Loaders.Normal;
                _obj.LoaderVersion = null;
                _obj.Save();
                break;
            case Loaders.Forge:
                _obj.Loader = Loaders.Forge;
                _obj.LoaderVersion = null;
                _obj.Save();
                await LoaderVersionLoad();
                break;
            case Loaders.NeoForge:
                _obj.Loader = Loaders.NeoForge;
                _obj.LoaderVersion = null;
                _obj.Save();
                await LoaderVersionLoad();
                break;
            case Loaders.Fabric:
                _obj.Loader = Loaders.Fabric;
                _obj.LoaderVersion = null;
                _obj.Save();
                await LoaderVersionLoad();
                break;
            case Loaders.Quilt:
                _obj.Loader = Loaders.Quilt;
                _obj.LoaderVersion = null;
                _obj.Save();
                await LoaderVersionLoad();
                break;
        }

    }

    async partial void OnGameVersionChanged(string value)
    {
        if (_gameLoad)
            return;

        LoaderVersion = null;
        LoaderVersionList.Clear();
        LoaderType = 0;

        _obj.Version = value;
        _obj.Loader = Loaders.Normal;
        _obj.LoaderVersion = null;
        _obj.Save();

        await LoaderReload();
    }

    partial void OnLoaderVersionChanged(string? value)
    {
        if (_gameLoad)
            return;

        _obj.LoaderVersion = value;
        _obj.Save();
    }

    partial void OnGroupChanged(string? value)
    {
        if (_gameLoad)
            return;

        GameBinding.MoveGameGroup(_obj, value);
    }

    partial void OnModPackChanged(bool value)
    {
        if (_gameLoad)
            return;

        _obj.ModPack = value;
        _obj.Save();
    }

    partial void OnPIDChanged(string? value)
    {
        if (_gameLoad)
            return;

        _obj.PID = value;
        _obj.Save();
    }

    partial void OnFIDChanged(string? value)
    {
        if (_gameLoad)
            return;

        _obj.FID = value;
        _obj.Save();
    }

    [RelayCommand]
    public void ExportGame()
    {
        App.ShowGameExport(_obj);
    }

    [RelayCommand]
    public async Task CheckModPackUpdate()
    {
        if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
        {
            Model.Show(App.GetLanguage("GameEditWindow.Tab1.Error3"));
            return;
        }

        Model.Progress(App.GetLanguage("GameEditWindow.Tab1.Info2"));
        if (FuntionUtils.CheckNotNumber(PID) || FuntionUtils.CheckNotNumber(FID))
        {
            var list = await ModrinthAPI.GetFileVersions(PID, _obj.Version, _obj.Loader);
            Model.ProgressClose();
            if (list == null)
            {
                Model.Show(App.GetLanguage("GameEditWindow.Tab1.Info3"));
            }
            else if (list.Count == 0)
            {
                Model.Notify(App.GetLanguage("GameEditWindow.Tab1.Info4"));
            }
            else if (list[0].id.ToString() == FID)
            {
                Model.Notify(App.GetLanguage("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await Model.ShowWait(App.GetLanguage("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                Model.Progress(App.GetLanguage("GameEditWindow.Tab1.Info8"));
                var item = list[0];
                res = await GameBinding.ModPackUpdate(_obj, item);
                Model.ProgressClose();
                if (!res)
                {
                    Model.Show(App.GetLanguage("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    Model.Notify(App.GetLanguage("GameEditWindow.Tab1.Info7"));
                    FID = item.id.ToString();
                }
            }
        }
        else
        {
            var list = await CurseForgeAPI.GetCurseForgeFiles(PID, _obj.Version);
            Model.ProgressClose();
            if (list == null)
            {
                Model.Show(App.GetLanguage("GameEditWindow.Tab1.Info3"));
            }
            else if (list.data.Count == 0)
            {
                Model.Notify(App.GetLanguage("GameEditWindow.Tab1.Info4"));
            }
            else if (list.data[0].id.ToString() == FID)
            {
                Model.Notify(App.GetLanguage("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await Model.ShowWait(App.GetLanguage("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                Model.Progress(App.GetLanguage("GameEditWindow.Tab1.Info8"));
                var item = list.data[0];
                res = await GameBinding.ModPackUpdate(_obj, item);
                Model.ProgressClose();
                if (!res)
                {
                    Model.Show(App.GetLanguage("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    Model.Notify(App.GetLanguage("GameEditWindow.Tab1.Info7"));
                    FID = item.id.ToString();
                }
            }
        }
    }

    [RelayCommand]
    public void OpenGameLog()
    {
        App.ShowGameLog(_obj);
    }

    [RelayCommand]
    public async Task AddGroup()
    {
        var (Cancel, Text1) = await Model.ShowOne(App.GetLanguage("AddGameWindow.Tab1.Info5"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text1))
        {
            Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(Text1))
        {
            Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Error3"));
            return;
        }

        Model.Notify(App.GetLanguage("AddGameWindow.Tab1.Info6"));

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
                Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
                var list = await GameBinding.GetForgeVersion(_obj.Version);
                Model.ProgressClose();
                if (list == null)
                {
                    Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.NeoForge:
                Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
                list = await GameBinding.GetNeoForgeVersion(_obj.Version);
                Model.ProgressClose();
                if (list == null)
                {
                    Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.Fabric:
                Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info2"));
                list = await GameBinding.GetFabricVersion(_obj.Version);
                Model.ProgressClose();
                if (list == null)
                {
                    Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.Quilt:
                Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info3"));
                list = await GameBinding.GetQuiltVersion(_obj.Version);
                Model.ProgressClose();
                if (list == null)
                {
                    Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
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
        Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info4"));
        var list = await GameBinding.GetForgeSupportVersion();
        if (list != null && list.Contains(_obj.Version))
        {
            _loaderTypeList.Add(Loaders.Forge);
            LoaderTypeList.Add(Loaders.Forge.GetName());
        }

        list = await GameBinding.GetNeoForgeSupportVersion();
        if (list != null && list.Contains(_obj.Version))
        {
            _loaderTypeList.Add(Loaders.NeoForge);
            LoaderTypeList.Add(Loaders.NeoForge.GetName());
        }

        list = await GameBinding.GetFabricSupportVersion();
        if (list != null && list.Contains(_obj.Version))
        {
            _loaderTypeList.Add(Loaders.Fabric);
            LoaderTypeList.Add(Loaders.Fabric.GetName());
        }

        list = await GameBinding.GetQuiltSupportVersion();
        if (list != null && list.Contains(_obj.Version))
        {
            _loaderTypeList.Add(Loaders.Quilt);
            LoaderTypeList.Add(Loaders.Quilt.GetName());
        }
        Model.ProgressClose();

        _gameLoad = false;
    }

    [RelayCommand]
    public async Task GameVersionReload()
    {
        Model.Progress(App.GetLanguage("GameEditWindow.Info1"));
        var res = await GameBinding.ReloadVersion();
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.GetLanguage("GameEditWindow.Error1"));
            return;
        }

        LoaderVersion = null;
        LoaderType = 0;

        GameVersionLoad();
    }

    [RelayCommand]
    public async Task Delete()
    {
        if (BaseBinding.IsGameRun(_obj))
        {
            Model.Show(App.GetLanguage("GameEditWindow.Tab1.Error1"));
            return;
        }

        var res = await Model.ShowWait(string.Format(
            App.GetLanguage("GameEditWindow.Tab1.Info1"), _obj.Name));
        if (!res)
            return;

        var res1 = await GameBinding.DeleteGame(_obj);
        if (!res1)
        {
            Model.Show(App.GetLanguage("MainWindow.Info37"));
        }
    }

    [RelayCommand]
    public void Open()
    {
        PathBinding.OpPath(_obj, PathType.BasePath);
    }

    [RelayCommand]
    public void OpenOptifine()
    {
        App.ShowAdd(_obj, FileType.Optifne);
    }

    [RelayCommand]
    public void OpenServerPack()
    {
        App.ShowServerPack(_obj);
    }

    [RelayCommand]
    public void OpenConfigEdit()
    {
        App.ShowConfigEdit(_obj);
    }

    private async void GameVersionLoad()
    {
        GameVersionList.Clear();
        switch (VersionType)
        {
            case 0:
                _obj.GameType = GameType.Release;
                GameVersionList.AddRange(await GameBinding.GetGameVersion(true, false, false));
                break;
            case 1:
                _obj.GameType = GameType.Snapshot;
                GameVersionList.AddRange(await GameBinding.GetGameVersion(false, true, false));
                break;
            case 2:
                _obj.GameType = GameType.Other;
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
        GameRun = BaseBinding.IsGameRun(_obj);
    }

    public void GameLoad()
    {
        _gameLoad = true;

        if (_obj.GameType == GameType.Snapshot)
        {
            VersionType = 2;
        }
        else if (_obj.GameType == GameType.Other)
        {
            VersionType = 3;
        }
        else
        {
            VersionType = 0;
        }

        _loaderTypeList.Add(Loaders.Normal);
        LoaderTypeList.Add(Loaders.Normal.GetName());

        if (_obj.Loader != Loaders.Normal)
        {
            _loaderTypeList.Add(_obj.Loader);
            LoaderTypeList.Add(_obj.Loader.GetName());

            LoaderType = 1;

            EnableLoader = false;
            LoaderVersionList.Clear();
            if (_obj.LoaderVersion != null)
            {
                LoaderVersionList.Add(new(_obj.LoaderVersion));
            }
        }
        else
        {
            LoaderType = 0;
        }

        GameVersionLoad();
        GroupLoad();

        if (_obj.LoaderVersion != null)
        {
            LoaderVersion = new(_obj.LoaderVersion);
        }
        ModPack = _obj.ModPack;
        GameVersion = _obj.Version;
        Group = _obj.GroupName;
        FID = _obj.FID;
        PID = _obj.PID;

        GameRun = BaseBinding.IsGameRun(_obj);

        _gameLoad = false;
    }
}
