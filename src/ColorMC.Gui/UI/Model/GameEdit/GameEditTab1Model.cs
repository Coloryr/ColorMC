using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel
{
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    public ObservableCollection<string> LoaderVersionList { get; init; } = [];
    public ObservableCollection<string> GroupList { get; init; } = [];
    public string[] VersionTypeList { get; init; } = LanguageBinding.GetVersionType();
    public ObservableCollection<string> LoaderTypeList { get; init; } = [];
    public ObservableCollection<string> LangList { get; init; } = [];

    private readonly List<Loaders> _loaderTypeList = [];
    private readonly List<string> _langList = [];

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
    private string? _loaderInfo;

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
    [ObservableProperty]
    private bool _customLoader;
    [ObservableProperty]
    private bool _offLib;
    [ObservableProperty]
    private bool _isLoad;

    partial void OnIsLoadChanged(bool value)
    {
        if (value)
        {
            Model.Work();
        }
        else
        {
            Model.NoWork();
        }
    }

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
        if (opt.TryGetValue(GameLang.Name2, out var value1))
        {
            if (value1 == _langList[value])
            {
                return;
            }
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
        {
            return;
        }

        LoaderVersionList.Clear();
        LoaderVersion = null;

        if (_loaderTypeList.Count != 0)
        {
            var loader = _loaderTypeList[value];

            _obj.Loader = loader;
            _obj.LoaderVersion = null;
            CustomLoader = loader == Loaders.Custom;
        }
        else
        {
            CustomLoader = false;
            _obj.Loader = Loaders.Normal;
            _obj.LoaderVersion = null;
        }
        _obj.Save();
        await LoaderVersionLoad();
    }

    async partial void OnGameVersionChanged(string value)
    {
        if (_gameLoad)
        {
            return;
        }

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

    partial void OnOffLibChanged(bool value)
    {
        if (_gameLoad)
            return;

        _obj.CustomLoader ??= new();
        _obj.CustomLoader.OffLib = value;
        _obj.Save();
    }

    [RelayCommand]
    public async Task LangReload()
    {
        LangList.Clear();
        _langList.Clear();
        await LangLoad();
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
        if (DownloadItemHelper.TestSourceType(PID, FID) == SourceType.Modrinth)
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
                res = await GameBinding.ModPackUpdate(_obj, item, (size, now) =>
                {
                    Model.ProgressUpdate((double)now / size);
                }, PackState);
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
                res = await GameBinding.ModPackUpdate(_obj, item, (size, now) =>
                {
                    Model.ProgressUpdate((double)now / size);
                }, PackState);
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
    public async Task AddGroup()
    {
        var (Cancel, Text1) = await Model.ShowInputOne(App.Lang("Text.Group"), false);
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
                IsLoad = true;
                Model.Title1 = App.Lang("AddGameWindow.Tab1.Info1");
                var list = await WebBinding.GetForgeVersion(_obj.Version);
                IsLoad = false;
                Model.Title1 = "";
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
                IsLoad = true;
                Model.Title1 = App.Lang("AddGameWindow.Tab1.Info19");
                list = await WebBinding.GetNeoForgeVersion(_obj.Version);
                IsLoad = false;
                Model.Title1 = "";
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
                IsLoad = true;
                Model.Title1 = App.Lang("AddGameWindow.Tab1.Info2");
                list = await WebBinding.GetFabricVersion(_obj.Version);
                IsLoad = false;
                Model.Title1 = "";
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
                IsLoad = true;
                Model.Title1 = App.Lang("AddGameWindow.Tab1.Info3");
                list = await WebBinding.GetQuiltVersion(_obj.Version);
                IsLoad = false;
                Model.Title1 = "";
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
                IsLoad = true;
                Model.Title1 = App.Lang("AddGameWindow.Tab1.Info16");
                list = await WebBinding.GetOptifineVersion(_obj.Version);
                IsLoad = false;
                Model.Title1 = "";
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

        LoaderVersion = null;
        LoaderVersionList.Clear();

        _loaderTypeList.Clear();
        LoaderTypeList.Clear();
        _loaderTypeList.Add(Loaders.Normal);
        LoaderTypeList.Add(Loaders.Normal.GetName());
        _loaderTypeList.Add(Loaders.Custom);
        LoaderTypeList.Add(Loaders.Custom.GetName());

        IsLoad = true;
        Model.Title1 = App.Lang("AddGameWindow.Tab1.Info4");

        var loaders = await GameBinding.GetSupportLoader(GameVersion);
        foreach (var item in loaders)
        {
            _loaderTypeList.Add(item);
            LoaderTypeList.Add(item.GetName());
        }

        IsLoad = false;
        Model.Title1 = "";

        _gameLoad = false;
    }

    [RelayCommand]
    public async Task GameVersionReload()
    {
        _gameLoad = true;

        _loaderTypeList.Clear();
        LoaderTypeList.Clear();
        EnableLoader = false;
        IsLoad = true;
        Model.Title1 = App.Lang("GameEditWindow.Tab1.Info12");
        var res = await GameBinding.ReloadVersion();
        IsLoad = false;
        Model.Title1 = "";
        if (!res)
        {
            Model.Show(App.Lang("GameEditWindow.Tab1.Error4"));
            return;
        }

        LoaderVersion = null;
        LoaderType = 0;

        GameVersionLoad();

        _gameLoad = false;
    }

    [RelayCommand]
    public async Task SelectLoader()
    {
        var file = await PathBinding.SelectFile(FileType.Loader);
        if (file.Item1 == null)
        {
            return;
        }

        var res = await GameBinding.SetGameLoader(_obj, file.Item1);
        if (res.State)
        {
            ReadCustomLoader();
        }
        else
        {
            Model.Show(res.Message!);
        }
    }
    private void ExportGame()
    {
        WindowManager.ShowGameExport(_obj);
    }

    private void OpenGameLog()
    {
        WindowManager.ShowGameLog(_obj);
    }

    private void OpenConfigEdit()
    {
        WindowManager.ShowConfigEdit(_obj);
    }

    private void OpPath()
    {
        PathBinding.OpPath(_obj, PathType.BasePath);
    }

    private void OpenServerPack()
    {
        WindowManager.ShowServerPack(_obj);
    }

    private async void Delete()
    {
        if (GameManager.IsGameRun(_obj))
        {
            Model.Show(App.Lang("GameEditWindow.Tab1.Error1"));
            return;
        }

        var res = await Model.ShowWait(string.Format(
            App.Lang("GameEditWindow.Tab1.Info1"), _obj.Name));
        if (!res)
            return;

        Model.Progress(App.Lang("GameEditWindow.Tab1.Info11"));
        var res1 = await GameBinding.DeleteGame(_obj, Model.ShowWait);
        Model.ProgressClose();
        if (!res1)
        {
            Model.Show(App.Lang("MainWindow.Info37"));
        }
    }

    private async void GenGameInfo()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab1.Info10"));
        await GameBinding.GenGameInfo(_obj);
        Model.ProgressClose();
    }

    private async void ReadCustomLoader()
    {
        LoaderInfo = await GameBinding.GetGameLoader(_obj);
    }

    private async void GameVersionLoad()
    {
        GameVersionList.Clear();
        switch (VersionType)
        {
            case 0:
                _obj.GameType = GameType.Release;
                break;
            case 1:
                _obj.GameType = GameType.Snapshot;
                break;
            case 2:
                _obj.GameType = GameType.Other;
                break;
        }

        GameVersionList.AddRange(await GameBinding.GetGameVersions(_obj.GameType));
    }

    private void GroupLoad()
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    public void GameStateChange()
    {
        GameRun = GameManager.IsGameRun(_obj);
    }

    public async Task LangLoad()
    {
        if (LangList.Count > 1)
        {
            return;
        }
        LangList.Clear();
        _langList.Clear();
        if (_obj.Version == null)
        {
            return;
        }

        IsLoad = true;
        Model.Title1 = App.Lang("GameEditWindow.Tab1.Info9");
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
        IsLoad = false;
        Model.Title1 = "";
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

        _loaderTypeList.Clear();
        LoaderTypeList.Clear();

        _loaderTypeList.Add(Loaders.Normal);
        LoaderTypeList.Add(Loaders.Normal.GetName());

        if (_obj.Loader == Loaders.Custom)
        {
            _loaderTypeList.Add(Loaders.Custom);
            LoaderTypeList.Add(Loaders.Custom.GetName());

            LoaderType = 1;

            CustomLoader = true;
        }
        else if (_obj.Loader != Loaders.Normal)
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
            _loaderTypeList.Add(Loaders.Custom);
            LoaderTypeList.Add(Loaders.Custom.GetName());

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

        if (_obj.Loader == Loaders.Custom)
        {
            ReadCustomLoader();
        }

        OffLib = _obj.CustomLoader?.OffLib ?? false;

        GameRun = GameManager.IsGameRun(_obj);

        var opt = _obj.GetOptions();

        opt.TryGetValue("lang", out string? lang);

        if (lang != null && !string.IsNullOrWhiteSpace(_obj.Version))
        {
            var list = await Task.Run(() =>
            {
                var version = VersionPath.GetVersion(_obj.Version);
                if (version != null)
                {
                    var ass = AssetsPath.GetIndex(version);
                    if (ass != null)
                    {
                        return ass.GetLang(lang);
                    }
                }

                return null;
            });
            if (list != null)
            {
                LangList.Add(list.Name);
                _langList.Add(list.Key);

                Lang = 0;
            }
        }

        _gameLoad = false;
    }
}
