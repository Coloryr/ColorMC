using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameModel
{
    /// <summary>
    /// 游戏版本列表
    /// </summary>
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    /// <summary>
    /// 加载器版本列表
    /// </summary>
    public ObservableCollection<string> LoaderVersionList { get; init; } = [];

    /// <summary>
    /// 游戏版本类型
    /// </summary>
    public string[] VersionTypeList { get; init; } = LanguageBinding.GetVersionType();
    /// <summary>
    /// 加载器版本类型
    /// </summary>
    public ObservableCollection<string> LoaderTypeList { get; init; } = [];

    /// <summary>
    /// 游戏版本
    /// </summary>
    [ObservableProperty]
    private string? _version;
    /// <summary>
    /// 加载器版本
    /// </summary>
    [ObservableProperty]
    private string? _loaderVersion;
    [ObservableProperty]
    private string? _loaderLocal;

    /// <summary>
    /// 启用加载器
    /// </summary>
    [ObservableProperty]
    private bool _enableLoader;
    [ObservableProperty]
    private bool _offLib;
    [ObservableProperty]
    private bool _customLoader;
    [ObservableProperty]
    private bool _isLoad;

    /// <summary>
    /// 版本类型
    /// </summary>
    [ObservableProperty]
    private int _versionType;
    /// <summary>
    /// 加载器类型
    /// </summary>
    [ObservableProperty]
    private int _loaderType = -1;

    /// <summary>
    /// 加载器类型列表
    /// </summary>
    private readonly List<Loaders> _loaderTypeList = [];

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
    /// 游戏版本修改
    /// </summary>
    /// <param name="value"></param>
    async partial void OnVersionChanged(string? value)
    {
        if (value != null)
        {
            await VersionSelect();
        }
    }

    /// <summary>
    /// 加载器类型修改
    /// </summary>
    /// <param name="value"></param>
    async partial void OnLoaderTypeChanged(int value)
    {
        if (_load)
            return;

        await GetLoader();
    }

    /// <summary>
    /// 版本类型修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnVersionTypeChanged(int value)
    {
        GameVersionUpdate();
    }

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

        LoaderLocal = file.Item1;
    }

    /// <summary>
    /// 获取加载器版本
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task GetLoader()
    {
        EnableLoader = false;
        if (string.IsNullOrWhiteSpace(Version) || _loaderTypeList.Count == 0)
        {
            return;
        }

        CustomLoader = false;
        var loader = _loaderTypeList[LoaderType];
        LoaderVersionList.Clear();
        switch (loader)
        {
            case Loaders.Normal:
                break;
            case Loaders.Forge:
                IsLoad = true;
                Model.SubTitle = App.Lang("AddGameWindow.Tab1.Info1");
                var list = await WebBinding.GetForgeVersion(Version);
                IsLoad = false;
                Model.SubTitle = "";
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
                Model.SubTitle = App.Lang("AddGameWindow.Tab1.Info19");
                list = await WebBinding.GetNeoForgeVersion(Version);
                IsLoad = false;
                Model.SubTitle = "";
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
                Model.SubTitle = App.Lang("AddGameWindow.Tab1.Info2");
                list = await WebBinding.GetFabricVersion(Version);
                IsLoad = false;
                Model.SubTitle = "";
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
                Model.SubTitle = App.Lang("AddGameWindow.Tab1.Info3");
                list = await WebBinding.GetQuiltVersion(Version);
                IsLoad = false;
                Model.SubTitle = "";
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
                Model.SubTitle = App.Lang("AddGameWindow.Tab1.Info16");
                list = await WebBinding.GetOptifineVersion(Version);
                IsLoad = false;
                Model.SubTitle = "";
                if (list == null)
                {
                    Model.Show(App.Lang("AddGameWindow.Tab1.Error1"));
                    return;
                }

                EnableLoader = true;
                LoaderVersionList.Clear();
                LoaderVersionList.AddRange(list);
                break;
            case Loaders.Custom:
                CustomLoader = true;
                break;
        }

        Model.Notify(App.Lang("AddGameWindow.Tab1.Info22"));
    }

    /// <summary>
    /// 添加游戏
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddGame()
    {
        var name = Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error6"));
            return;
        }

        if (PathHelper.FileHasInvalidChars(name))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error13"));
            return;
        }

        string? version = Version;
        if (string.IsNullOrWhiteSpace(version))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error7"));
            return;
        }

        var loader = _loaderTypeList[LoaderType];

        var game = new GameSettingObj()
        {
            Name = name,
            Version = version,
            Loader = loader,
            LoaderVersion = LoaderVersion?.ToString(),
            GroupName = Group,
            CustomLoader = new()
            {
                OffLib = OffLib
            }
        };

        var res = await GameBinding.AddGame(game, Tab1GameRequest, Tab1GameOverwirte);
        if (!res)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error5"));
        }
        else
        {
            if (game.Loader == Loaders.Custom && !string.IsNullOrWhiteSpace(LoaderLocal))
            {
                var res1 = await GameBinding.SetGameLoader(game, LoaderLocal);
                if (!res1.State)
                {
                    Model.ShowWithOk(App.Lang("AddGameWindow.Tab1.Error18"), () =>
                    {
                        Done(game.UUID);
                    });
                    return;
                }
            }

            Done(game.UUID);
        }
    }

    /// <summary>
    /// 游戏版本刷新
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task VersionSelect()
    {
        _load = true;

        EnableLoader = false;
        _loaderTypeList.Clear();
        LoaderTypeList.Clear();
        LoaderVersionList.Clear();

        _loaderTypeList.Add(Loaders.Normal);
        LoaderTypeList.Add(Loaders.Normal.GetName());

        if (string.IsNullOrWhiteSpace(Version))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = Version;
        }

        IsLoad = true;
        Model.SubTitle = App.Lang("AddGameWindow.Tab1.Info4");

        var loaders = await GameBinding.GetSupportLoader(Version);
        foreach (var item in loaders)
        {
            _loaderTypeList.Add(item);
            LoaderTypeList.Add(item.GetName());
        }

        _loaderTypeList.Add(Loaders.Custom);
        LoaderTypeList.Add(Loaders.Custom.GetName());

        IsLoad = false;
        Model.SubTitle = "";

        LoaderType = 0;
        _load = false;

        Model.Notify(App.Lang("AddGameWindow.Tab1.Info24"));
    }

    /// <summary>
    /// 加载游戏版本
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task LoadVersion()
    {
        _loaderTypeList.Clear();
        LoaderTypeList.Clear();
        EnableLoader = false;
        LoaderVersion = null;
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

        GameVersionUpdate();

        Model.Notify(App.Lang("AddGameWindow.Tab1.Info23"));
    }

    /// <summary>
    /// 游戏版本读取
    /// </summary>
    private async void GameVersionUpdate()
    {
        GameVersionList.Clear();
        switch (VersionType)
        {
            case 0:
                GameVersionList.AddRange(await GameBinding.GetGameVersions(GameType.Release));
                break;
            case 1:
                GameVersionList.AddRange(await GameBinding.GetGameVersions(GameType.Snapshot));
                break;
            case 2:
                GameVersionList.AddRange(await GameBinding.GetGameVersions(GameType.Other));
                break;
        }
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private async Task<bool> Tab1GameOverwirte(GameSettingObj obj)
    {
        Model.ProgressClose();
        var test = await Model.ShowAsync(
            string.Format(App.Lang("AddGameWindow.Info2"), obj.Name));
        return test;
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private Task<bool> Tab1GameRequest(string state)
    {
        Model.ProgressClose();
        return Model.ShowAsync(state);
    }


}
