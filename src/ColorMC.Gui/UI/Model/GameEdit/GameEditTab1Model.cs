using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
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
    public string[] VersionTypeList { get; init; } = LanguageBinding.GetVersionType();
    public ObservableCollection<string> LoaderTypeList { get; init; } = new();
    public ObservableCollection<string> LangList { get; init; } = new();

    private readonly List<Loaders> _loaderTypeList = new();
    private readonly List<string> _langList = new();

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
    private int _lang = -1;

    [ObservableProperty]
    private bool _modPack;

    [ObservableProperty]
    private bool _gameRun;

    [ObservableProperty]
    private bool _enableLoader;

    partial void OnLangChanged(int value)
    {
        if (_gameLoad)
        {
            return;
        }

        if (value < 0 || value >= _langList.Count)
        {
            return;
        }

        var opt = _obj.GetOptions();
        if (opt.ContainsKey(GameLang.Name2))
        {
            opt[GameLang.Name2] = _langList[value];
        }
        else
        {
            opt.Add(GameLang.Name2, _langList[value]);
        }

        _obj.SaveOptions(opt);
    }

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
        LoaderVersion = null;
        _obj.Loader = loader;
        _obj.LoaderVersion = null;
        _obj.Save();
        await LoaderVersionLoad();

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
    public async Task LangReload()
    { 
        await LangLoad();
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
            Model.Show(App.Lang("GameEditWindow.Tab1.Error3"));
            return;
        }

        Model.Progress(App.Lang("GameEditWindow.Tab1.Info2"));
        if (FuntionUtils.CheckNotNumber(PID) || FuntionUtils.CheckNotNumber(FID))
        {
            var list = await ModrinthAPI.GetFileVersions(PID, _obj.Version, _obj.Loader);
            Model.ProgressClose();
            if (list == null)
            {
                Model.Show(App.Lang("GameEditWindow.Tab1.Info3"));
            }
            else if (list.Count == 0)
            {
                Model.Notify(App.Lang("GameEditWindow.Tab1.Info4"));
            }
            else if (list[0].id.ToString() == FID)
            {
                Model.Notify(App.Lang("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await Model.ShowWait(App.Lang("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                Model.Progress(App.Lang("GameEditWindow.Tab1.Info8"));
                var item = list[0];
                res = await GameBinding.ModPackUpdate(_obj, item);
                Model.ProgressClose();
                if (!res)
                {
                    Model.Show(App.Lang("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    Model.Notify(App.Lang("GameEditWindow.Tab1.Info7"));
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
                Model.Show(App.Lang("GameEditWindow.Tab1.Info3"));
            }
            else if (list.data.Count == 0)
            {
                Model.Notify(App.Lang("GameEditWindow.Tab1.Info4"));
            }
            else if (list.data[0].id.ToString() == FID)
            {
                Model.Notify(App.Lang("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await Model.ShowWait(App.Lang("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                Model.Progress(App.Lang("GameEditWindow.Tab1.Info8"));
                var item = list.data[0];
                res = await GameBinding.ModPackUpdate(_obj, item);
                Model.ProgressClose();
                if (!res)
                {
                    Model.Show(App.Lang("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    Model.Notify(App.Lang("GameEditWindow.Tab1.Info7"));
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
        var (Cancel, Text1) = await Model.ShowInputOne(App.Lang("AddGameWindow.Tab1.Info5"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text1))
        {
            Model.Progress(App.Lang("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(Text1))
        {
            Model.Progress(App.Lang("AddGameWindow.Tab1.Error3"));
            return;
        }

        Model.Notify(App.Lang("AddGameWindow.Tab1.Info6"));

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
                Model.Progress(App.Lang("AddGameWindow.Tab1.Info1"));
                var list = await WebBinding.GetForgeVersion(_obj.Version);
                Model.ProgressClose();
                if (list == null)
                {
                    Model.Show(App.Lang("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.NeoForge:
                Model.Progress(App.Lang("AddGameWindow.Tab1.Info1"));
                list = await WebBinding.GetNeoForgeVersion(_obj.Version);
                Model.ProgressClose();
                if (list == null)
                {
                    Model.Show(App.Lang("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.Fabric:
                Model.Progress(App.Lang("AddGameWindow.Tab1.Info2"));
                list = await WebBinding.GetFabricVersion(_obj.Version);
                Model.ProgressClose();
                if (list == null)
                {
                    Model.Show(App.Lang("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.Quilt:
                Model.Progress(App.Lang("AddGameWindow.Tab1.Info3"));
                list = await WebBinding.GetQuiltVersion(_obj.Version);
                Model.ProgressClose();
                if (list == null)
                {
                    Model.Show(App.Lang("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.OptiFine:
                Model.Progress(App.Lang("AddGameWindow.Tab1.Info16"));
                list = await WebBinding.GetOptifineVersion(_obj.Version);
                Model.ProgressClose();
                if (list == null)
                {
                    Model.Show(App.Lang("AddGameWindow.Tab1.Error1"));
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
        Model.Progress(App.Lang("AddGameWindow.Tab1.Info4"));
        var list = await WebBinding.GetForgeSupportVersion();
        if (list != null && list.Contains(_obj.Version))
        {
            _loaderTypeList.Add(Loaders.Forge);
            LoaderTypeList.Add(Loaders.Forge.GetName());
        }

        list = await WebBinding.GetNeoForgeSupportVersion();
        if (list != null && list.Contains(_obj.Version))
        {
            _loaderTypeList.Add(Loaders.NeoForge);
            LoaderTypeList.Add(Loaders.NeoForge.GetName());
        }

        list = await WebBinding.GetFabricSupportVersion();
        if (list != null && list.Contains(_obj.Version))
        {
            _loaderTypeList.Add(Loaders.Fabric);
            LoaderTypeList.Add(Loaders.Fabric.GetName());
        }

        list = await WebBinding.GetQuiltSupportVersion();
        if (list != null && list.Contains(_obj.Version))
        {
            _loaderTypeList.Add(Loaders.Quilt);
            LoaderTypeList.Add(Loaders.Quilt.GetName());
        }
        list = await WebBinding.GetOptifineSupportVersion();
        if (list != null && list.Contains(_obj.Version))
        {
            _loaderTypeList.Add(Loaders.OptiFine);
            LoaderTypeList.Add(Loaders.OptiFine.GetName());
        }
        Model.ProgressClose();

        _gameLoad = false;
    }

    [RelayCommand]
    public async Task GameVersionReload()
    {
        Model.Progress(App.Lang("GameEditWindow.Info1"));
        var res = await GameBinding.ReloadVersion();
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("GameEditWindow.Error1"));
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
            Model.Show(App.Lang("GameEditWindow.Tab1.Error1"));
            return;
        }

        var res = await Model.ShowWait(string.Format(
            App.Lang("GameEditWindow.Tab1.Info1"), _obj.Name));
        if (!res)
            return;

        Model.Progress(App.Lang("Gui.Info34"));
        var res1 = await GameBinding.DeleteGame(Model, _obj);
        Model.ProgressClose();
        if (!res1)
        {
            Model.Show(App.Lang("MainWindow.Info37"));
        }
    }

    [RelayCommand]
    public void Open()
    {
        PathBinding.OpPath(_obj, PathType.BasePath);
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

    public async Task LangLoad()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab1.Info9"));
        LangList.Clear();
        var list = await Task.Run(() =>
        {
            var version = VersionPath.GetVersion(_obj.Version);
            if (version != null)
            {
                var ass = AssetsPath.GetIndex(version);
                if (ass != null)
                {
                    return ass.GetLangs();
                }
            }

            return GameLang.GetLangs(null);
        });

        var opt = _obj.GetOptions();
        int a = 0;

        opt.TryGetValue("lang", out string? lang);

        foreach (var item in list)
        {
            LangList.Add(item.Value);
            _langList.Add(item.Key);

            if (lang != null && lang == item.Key)
            {
                Lang = a;
            }
            a++;
        }
        Model.ProgressClose();
    }

    public async void GameLoad()
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

        await LangLoad();

        _gameLoad = false;
    }
}
