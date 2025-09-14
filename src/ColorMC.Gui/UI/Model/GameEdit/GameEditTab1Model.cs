using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel
{
    /// <summary>
    /// 编码列表
    /// </summary>
    public string[] EncodingList { get; init; } = BaseBinding.GetEncoding();
    /// <summary>
    /// 游戏版本列表
    /// </summary>
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    /// <summary>
    /// 加载器版本列表
    /// </summary>
    public ObservableCollection<string> LoaderVersionList { get; init; } = [];
    /// <summary>
    /// 游戏分组列表
    /// </summary>
    public ObservableCollection<string> GroupList { get; init; } = [];
    /// <summary>
    /// 版本类型列表
    /// </summary>
    public string[] VersionTypeList { get; init; } = LanguageBinding.GetVersionType();
    /// <summary>
    /// 加载器类型列表
    /// </summary>
    public ObservableCollection<string> LoaderTypeList { get; init; } = [];
    /// <summary>
    /// 语言列表
    /// </summary>
    public ObservableCollection<string> LangList { get; init; } = [];
    /// <summary>
    /// 自定义游戏启动配置列表
    /// </summary>
    public ObservableCollection<CustomJsonModel> JsonList { get; init; } = [];

    /// <summary>
    /// 加载器类型
    /// </summary>
    private readonly List<Loaders> _loaderTypeList = [];
    /// <summary>
    /// 语言类型
    /// </summary>
    private readonly List<string> _langList = [];

    /// <summary>
    /// 游戏版本
    /// </summary>
    [ObservableProperty]
    private string _gameVersion;
    /// <summary>
    /// 加载器版本
    /// </summary>
    [ObservableProperty]
    private string? _loaderVersion;
    /// <summary>
    /// 游戏分组
    /// </summary>
    [ObservableProperty]
    private string? _group;
    /// <summary>
    /// 整合包PID
    /// </summary>
    [ObservableProperty]
    private string? _pID;
    /// <summary>
    /// 整合包FID
    /// </summary>
    [ObservableProperty]
    private string? _fID;
    /// <summary>
    /// 自定义加载器信息
    /// </summary>
    [ObservableProperty]
    private string? _loaderInfo;

    /// <summary>
    /// 版本类型
    /// </summary>
    [ObservableProperty]
    private int _versionType = -1;
    /// <summary>
    /// 加载器类型
    /// </summary>
    [ObservableProperty]
    private int _loaderType = -1;
    /// <summary>
    /// 语言
    /// </summary>
    [ObservableProperty]
    private int _lang = -1;
    /// <summary>
    /// 编码类型
    /// </summary>
    [ObservableProperty]
    private int _encoding = 0;

    /// <summary>
    /// 是否为整合包
    /// </summary>
    [ObservableProperty]
    private bool _modPack;
    /// <summary>
    /// 游戏是否在运行中
    /// </summary>
    [ObservableProperty]
    private bool _gameRun;
    /// <summary>
    /// 是否启用加载器
    /// </summary>
    [ObservableProperty]
    private bool _enableLoader;
    /// <summary>
    /// 是否是自定义加载器
    /// </summary>
    [ObservableProperty]
    private bool _customLoader;
    /// <summary>
    /// 是否后加入运行库
    /// </summary>
    [ObservableProperty]
    private bool _offLib;
    /// <summary>
    /// 是否后删除原版运行库
    /// </summary>
    [ObservableProperty]
    private bool _removeLib;
    /// <summary>
    /// 是否在加载信息中
    /// </summary>
    [ObservableProperty]
    private bool _isLoad;
    /// <summary>
    /// 是否没有游戏版本
    /// </summary>
    [ObservableProperty]
    private bool _gameVersionEmpty;
    /// <summary>
    /// 是否让日志窗口自动显示
    /// </summary>
    [ObservableProperty]
    private bool _logAutoShow;
    /// <summary>
    /// 是否启用自定义启动配置
    /// </summary>
    [ObservableProperty]
    private bool _customJson;

    /// <summary>
    /// 游戏配置是否在加载
    /// </summary>
    private bool _gameLoad;

    partial void OnCustomJsonChanged(bool value)
    {
        if (_gameLoad)
        {
            return;
        }

        _obj.CustomLoader ??= new();
        _obj.CustomLoader.CustomJson = value;
        _obj.Save();
    }

    /// <summary>
    /// 日志相关修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnLogAutoShowChanged(bool value)
    {
        if (_gameLoad)
        {
            return;
        }

        _obj.LogAutoShow = value;
        _obj.Save();
    }
    /// <summary>
    /// 编码类型修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnEncodingChanged(int value)
    {
        if (_gameLoad)
        {
            return;
        }

        _obj.Encoding = value;
        _obj.Save();
    }
    /// <summary>
    /// 是否加载中
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsLoadChanged(bool value)
    {
        if (value)
        {
            Model.Lock();
        }
        else
        {
            Model.Unlock();
        }
    }
    /// <summary>
    /// 游戏内语言修改
    /// </summary>
    /// <param name="value"></param>
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
        //获取语言列表
        var opt = _obj.GetOptions();
        if (opt.TryGetValue(Names.NameLangKey2, out var value1))
        {
            if (value1 == _langList[value])
            {
                return;
            }
            opt[Names.NameLangKey2] = _langList[value];
        }
        else
        {
            opt.Add(Names.NameLangKey2, _langList[value]);
        }

        _obj.SaveOptions(opt);
        Model.Notify(App.Lang("GameEditWindow.Tab1.Text17"));
    }
    /// <summary>
    /// 版本类型修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnVersionTypeChanged(int value)
    {
        GameVersionLoad();
    }
    /// <summary>
    /// 加载器类型修改
    /// </summary>
    /// <param name="value"></param>
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
    /// <summary>
    /// 游戏版本修改
    /// </summary>
    /// <param name="value"></param>
    async partial void OnGameVersionChanged(string value)
    {
        if (_gameLoad)
        {
            return;
        }

        GameVersionEmpty = string.IsNullOrWhiteSpace(GameVersion);
        if (GameVersionEmpty)
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
    /// <summary>
    /// 加载器版本修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnLoaderVersionChanged(string? value)
    {
        if (_gameLoad)
        {
            return;
        }

        _obj.LoaderVersion = value;
        _obj.Save();
    }
    /// <summary>
    /// 游戏分组修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnGroupChanged(string? value)
    {
        if (_gameLoad)
        {
            return;
        }

        GameBinding.MoveGameGroup(_obj, value);
    }
    /// <summary>
    /// 整合包修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnModPackChanged(bool value)
    {
        if (_gameLoad)
        {
            return;
        }

        _obj.ModPack = value;
        _obj.Save();
    }
    /// <summary>
    /// 整合包修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnPIDChanged(string? value)
    {
        if (_gameLoad)
        {
            return;
        }

        _obj.PID = value;
        _obj.Save();
    }
    /// <summary>
    /// 整合包修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnFIDChanged(string? value)
    {
        if (_gameLoad)
        {
            return;
        }

        _obj.FID = value;
        _obj.Save();
    }
    /// <summary>
    /// 加载库修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnOffLibChanged(bool value)
    {
        if (_gameLoad)
        {
            return;
        }

        _obj.CustomLoader ??= new();
        _obj.CustomLoader.OffLib = value;
        _obj.Save();
    }

    /// <summary>
    /// 重新读取自定义启动配置
    /// </summary>
    [RelayCommand]
    public void ReloadJson()
    {
        GameBinding.ReloadJson(_obj);
        JsonList.Clear();
        foreach (var item in _obj.CustomJson)
        {
            JsonList.Add(new(item));
        }
    }
    /// <summary>
    /// 打开自定义启动配置文件夹
    /// </summary>
    [RelayCommand]
    public void OpenJsonPath()
    {
        PathBinding.OpenPath(_obj, PathType.JsonDir);
    }
    /// <summary>
    /// 重新获取语言列表
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task LangReload()
    {
        LangList.Clear();
        _langList.Clear();
        if (_obj.Version == null)
        {
            return;
        }

        IsLoad = true;
        Model.SubTitle = App.Lang("GameEditWindow.Tab1.Info9");
        var list = await Task.Run(() =>
        {
            var ass = _obj.FindAsset();
            if (ass != null)
            {
                return ass.GetIndex().GetLangs();
            }

            return GameLang.GetLangs(null);
        });

        var opt = _obj.GetOptions();
        int a = 0;

        //尝试读取现在的语言
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
        Model.SubTitle = "";

        Model.Notify(App.Lang("GameEditWindow.Tab1.Info16"));
    }
    /// <summary>
    /// 检查模组更新
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task CheckModPackUpdate()
    {
        if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
        {
            Model.Show(App.Lang("GameEditWindow.Tab1.Error3"));
            return;
        }

        Model.Progress(App.Lang("GameEditWindow.Tab1.Info2"));
        //尝试通过ID获取版本号
        if (GameDownloadHelper.TestSourceType(PID, FID) == SourceType.Modrinth)
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
            else if (list[0].Id.ToString() == FID)
            {
                Model.Notify(App.Lang("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await Model.ShowAsync(App.Lang("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                Model.Progress(App.Lang("GameEditWindow.Tab1.Info8"));
                var item = list[0];
                res = await GameBinding.ModPackUpgrade(_obj, item, ProgressUpdate, PackState);
                Model.ProgressClose();
                if (!res)
                {
                    Model.Show(App.Lang("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    Model.Notify(App.Lang("GameEditWindow.Tab1.Info7"));
                    FID = item.Id.ToString();
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
            else if (list.Data.Count == 0)
            {
                Model.Notify(App.Lang("GameEditWindow.Tab1.Info4"));
            }
            else if (list.Data[0].Id.ToString() == FID)
            {
                Model.Notify(App.Lang("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await Model.ShowAsync(App.Lang("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                Model.Progress(App.Lang("GameEditWindow.Tab1.Info8"));
                var item = list.Data[0];
                res = await GameBinding.ModPackUpgrade(_obj, item, ProgressUpdate, PackState);
                Model.ProgressClose();
                if (!res)
                {
                    Model.Show(App.Lang("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    Model.Notify(App.Lang("GameEditWindow.Tab1.Info7"));
                    FID = item.Id.ToString();
                    res = await Model.ShowAsync(string.Format(App.Lang("GameEditWindow.Tab1.Info17"), item.DisplayName));
                    if (res)
                    {
                        GameBinding.SetGameName(_obj, item.DisplayName);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 添加游戏分组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddGroup()
    {
        var res = await Model.InputWithEditAsync(App.Lang("Text.Group"), false);
        if (res.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            Model.Progress(App.Lang("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(res.Text1))
        {
            Model.Progress(App.Lang("AddGameWindow.Tab1.Error3"));
            return;
        }

        Model.Notify(App.Lang("AddGameWindow.Tab1.Info6"));

        GroupList.Add(res.Text1);
    }
    /// <summary>
    /// 获取加载器版本
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task LoaderVersionLoad()
    {
        EnableLoader = false;
        LoaderVersionList.Clear();

        if (_loaderTypeList.Count <= LoaderType || LoaderType == -1)
        {
            return;
        }

        var loader = _loaderTypeList[LoaderType];
        if (loader is Loaders.Normal or Loaders.Custom)
        {
            return;
        }
        IsLoad = true;
        Model.SubTitle = loader switch
        {
            Loaders.Forge => App.Lang("AddGameWindow.Tab1.Info1"),
            Loaders.NeoForge => App.Lang("AddGameWindow.Tab1.Info19"),
            Loaders.Fabric => App.Lang("AddGameWindow.Tab1.Info2"),
            Loaders.Quilt => App.Lang("AddGameWindow.Tab1.Info3"),
            Loaders.OptiFine => App.Lang("AddGameWindow.Tab1.Info16"),
            _ => ""
        };
        var list = loader switch
        {
            Loaders.Forge => await WebBinding.GetForgeVersion(_obj.Version),
            Loaders.NeoForge => await WebBinding.GetNeoForgeVersion(_obj.Version),
            Loaders.Fabric => await WebBinding.GetFabricVersion(_obj.Version),
            Loaders.Quilt => await WebBinding.GetQuiltVersion(_obj.Version),
            Loaders.OptiFine => await WebBinding.GetOptifineVersion(_obj.Version),
            _ => null
        };

        IsLoad = false;
        Model.SubTitle = "";
        if (list == null)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error1"));
            return;
        }

        EnableLoader = true;
        LoaderVersionList.AddRange(list);

        Model.Notify(App.Lang("GameEditWindow.Tab1.Info15"));
    }

    /// <summary>
    /// 加载器支持重新获取
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task LoaderReload()
    {
        if (GameVersionEmpty)
        {
            return;
        }

        _gameLoad = true;

        var loaderType = _loaderTypeList[LoaderType];
        var loaderVersion = LoaderVersion;

        LoaderVersion = null;
        LoaderVersionList.Clear();

        _loaderTypeList.Clear();
        LoaderTypeList.Clear();
        _loaderTypeList.Add(Loaders.Normal);
        LoaderTypeList.Add(Loaders.Normal.GetName());
        _loaderTypeList.Add(Loaders.Custom);
        LoaderTypeList.Add(Loaders.Custom.GetName());

        IsLoad = true;
        Model.SubTitle = App.Lang("AddGameWindow.Tab1.Info4");

        var res = await GameHelper.GetSupportLoader(GameVersion);
        foreach (var item in res.Done)
        {
            _loaderTypeList.Add(item);
            LoaderTypeList.Add(item.GetName());
        }
        foreach (var item in res.Fail)
        {
            Model.Notify(string.Format(App.Lang("AddGameWindow.Tab1.Error19"), item.GetName()));
        }
        if (_loaderTypeList.Contains(loaderType))
        {
            LoaderType = _loaderTypeList.IndexOf(loaderType);
            if (loaderVersion != null)
            {
                LoaderVersionList.Add(loaderVersion);
                LoaderVersion = loaderVersion;
            }
        }

        IsLoad = false;
        Model.SubTitle = "";

        _gameLoad = false;

        Model.Notify(App.Lang("GameEditWindow.Tab1.Info14"));
    }

    /// <summary>
    /// 游戏版本重新获取
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task GameVersionReload()
    {
        _gameLoad = true;

        EnableLoader = false;
        IsLoad = true;
        Model.SubTitle = App.Lang("GameEditWindow.Tab1.Info12");
        var res = await GameBinding.ReloadVersion();
        IsLoad = false;
        Model.SubTitle = "";
        if (!res)
        {
            Model.Show(App.Lang("GameEditWindow.Tab1.Error4"));
            return;
        }

        GameVersionLoad();

        _gameLoad = false;

        Model.Notify(App.Lang("GameEditWindow.Tab1.Info13"));
    }

    /// <summary>
    /// 选择自定义加载器
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SelectLoader()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.Loader);
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

    /// <summary>
    /// 导出游戏实例
    /// </summary>
    private void ExportGame()
    {
        WindowManager.ShowGameExport(_obj);
    }

    /// <summary>
    /// 打开游戏实例日志
    /// </summary>
    private void OpenGameLog()
    {
        WindowManager.ShowGameLog(_obj);
    }

    /// <summary>
    /// 打开配置编辑
    /// </summary>
    private void OpenConfigEdit()
    {
        WindowManager.ShowConfigEdit(_obj);
    }

    /// <summary>
    /// 打开实例路径
    /// </summary>
    private void OpPath()
    {
        PathBinding.OpenPath(_obj, PathType.BasePath);
    }

    /// <summary>
    /// 打开服务器包生成
    /// </summary>
    private void OpenServerPack()
    {
        WindowManager.ShowServerPack(_obj);
    }

    /// <summary>
    /// 删除该游戏实例
    /// </summary>
    private async void Delete()
    {
        if (GameManager.IsGameRun(_obj))
        {
            Model.Show(App.Lang("GameEditWindow.Tab1.Error1"));
            return;
        }

        var res = await Model.ShowAsync(string.Format(
            App.Lang("GameEditWindow.Tab1.Info1"), _obj.Name));
        if (!res)
        {
            return;
        }

        Model.Progress(App.Lang("GameEditWindow.Tab1.Info11"));
        var res1 = await GameBinding.DeleteGame(_obj);
        Model.ProgressClose();
        if (!res1)
        {
            Model.Show(App.Lang("MainWindow.Info37"));
        }
    }

    /// <summary>
    /// 生成游戏实例信息
    /// </summary>
    private async void GenGameInfo()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab1.Info10"));
        await GameBinding.GenGameInfo(_obj);
        Model.ProgressClose();
    }

    /// <summary>
    /// 读取自定义加载器
    /// </summary>
    private async void ReadCustomLoader()
    {
        LoaderInfo = await GameBinding.GetGameLoader(_obj);
    }

    /// <summary>
    /// 获取游戏版本列表
    /// </summary>
    private async void GameVersionLoad()
    {
        var version = GameVersion;
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
        if (GameVersionList.Contains(version))
        {
            GameVersion = version;
        }
    }

    /// <summary>
    /// 重命名游戏实例
    /// </summary>
    private async void Rename()
    {
        var res = await Model.Input(App.Lang("MainWindow.Info23"), _obj.Name);
        if (res.Cancel)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            Model.Show(App.Lang("MainWindow.Error3"));
            return;
        }

        GameBinding.SetGameName(_obj, res.Text1);
    }

    /// <summary>
    /// 游戏实例分组加载
    /// </summary>
    private void GroupLoad()
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    /// <summary>
    /// 进度条更新
    /// </summary>
    /// <param name="size"></param>
    /// <param name="now"></param>
    private void ProgressUpdate(int size, int now)
    {
        Model.ProgressUpdate((double)now / size * 100);
    }

    /// <summary>
    /// 游戏实例配置加载
    /// </summary>
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
        _loaderTypeList.Add(Loaders.Custom);
        LoaderTypeList.Add(Loaders.Custom.GetName());

        //自定义加载器处理
        if (_obj.Loader == Loaders.Custom)
        {
            LoaderType = 1;
            ReadCustomLoader();
            CustomLoader = true;
        }
        else if (_obj.Loader != Loaders.Normal)
        {
            _loaderTypeList.Add(_obj.Loader);
            LoaderTypeList.Add(_obj.Loader.GetName());

            LoaderType = 2;

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

        GameVersionEmpty = string.IsNullOrWhiteSpace(GameVersion);

        LogAutoShow = _obj.LogAutoShow;
        Encoding = _obj.Encoding;
        Group = _obj.GroupName;
        FID = _obj.FID;
        PID = _obj.PID;

        OffLib = _obj.CustomLoader?.OffLib ?? false;
        RemoveLib = _obj.CustomLoader?.RemoveLib ?? false;
        CustomJson = _obj.CustomLoader?.CustomJson ?? false;

        JsonList.Clear();
        foreach (var item in _obj.CustomJson)
        {
            JsonList.Add(new(item));
        }

        GameRun = GameManager.IsGameRun(_obj);

        var opt = _obj.GetOptions();

        //加载语言
        opt.TryGetValue(GuiNames.NameLangKey, out string? lang);

        if (lang != null && !string.IsNullOrWhiteSpace(_obj.Version))
        {
            var list = await Task.Run(() =>
            {
                var version = VersionPath.GetVersion(_obj.Version);
                if (version != null)
                {
                    var ass = AssetsPath.GetIndex(version.AssetIndex);
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
            else
            {
                var list1 = await GameLang.GetLangsAsync(null);
                if (list1.TryGetValue(lang, out var name))
                {
                    LangList.Add(name);
                    _langList.Add(lang);

                    Lang = 0;
                }
            }
        }

        _gameLoad = false;
    }
}
