using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏实例
/// 从头新建
/// </summary>
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
    public string[] VersionTypeList { get; init; } = LanguageUtils.GetVersionType();
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
    /// <summary>
    /// 自定义加载器位置
    /// </summary>
    [ObservableProperty]
    private string? _loaderLocal;

    /// <summary>
    /// 启用加载器
    /// </summary>
    [ObservableProperty]
    private bool _enableLoader;
    /// <summary>
    /// 启用加载器
    /// </summary>
    [ObservableProperty]
    private bool _enableLoaderVersion;
    /// <summary>
    /// 后加载原版运行库
    /// </summary>
    [ObservableProperty]
    private bool _offLib;
    /// <summary>
    /// 自定义加载器
    /// </summary>
    [ObservableProperty]
    private bool _customLoader;
    /// <summary>
    /// 是否正在加载中
    /// </summary>
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

    /// <summary>
    /// 正在加载状态改变
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
        var file = await PathBinding.SelectFileAsync(top, FileType.Loader);
        if (file.Path == null)
        {
            return;
        }

        LoaderLocal = file.Path;
    }

    /// <summary>
    /// 获取加载器版本
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task GetLoader()
    {
        EnableLoader = false;
        EnableLoaderVersion = false;
        if (string.IsNullOrWhiteSpace(Version) || _loaderTypeList.Count == 0)
        {
            return;
        }

        CustomLoader = false;
        var loader = _loaderTypeList[LoaderType];
        LoaderVersionList.Clear();
        //无加载器不需要获取
        if (loader == Loaders.Normal)
        {
            return;
        }
        else if (loader == Loaders.Custom)
        {
            CustomLoader = true;
            return;
        }
        //根据加载器类型获取版本信息
        List<string>? list = null;
        IsLoad = true;
        switch (loader)
        {
            case Loaders.Forge:
                Model.SubTitle = LanguageUtils.Get("AddGameWindow.Tab1.Text24");
                list = await WebBinding.GetForgeVersionAsync(Version);
                break;
            case Loaders.NeoForge:
                Model.SubTitle = LanguageUtils.Get("AddGameWindow.Tab1.Text38");
                list = await WebBinding.GetNeoForgeVersionAsync(Version);
                break;
            case Loaders.Fabric:
                Model.SubTitle = LanguageUtils.Get("AddGameWindow.Tab1.Text25");
                list = await WebBinding.GetFabricVersionAsync(Version);
                break;
            case Loaders.Quilt:
                Model.SubTitle = LanguageUtils.Get("AddGameWindow.Tab1.Text26");
                list = await WebBinding.GetQuiltVersionAsync(Version);
                break;
            case Loaders.OptiFine:
                Model.SubTitle = LanguageUtils.Get("AddGameWindow.Tab1.Text37");
                list = await WebBinding.GetOptifineVersionAsync(Version);
                break;
        }

        IsLoad = false;
        Model.SubTitle = "";
        if (list == null)
        {
            Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text44"));
            return;
        }

        EnableLoader = true;
        EnableLoaderVersion = true;
        LoaderVersionList.Clear();
        LoaderVersionList.AddRange(list);

        Model.Notify(LanguageUtils.Get("AddGameWindow.Tab1.Text40"));
    }

    /// <summary>
    /// 添加游戏
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddGame()
    {
        //检测输入内容合法性
        var name = Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text48"));
            return;
        }

        if (PathHelper.FileHasInvalidChars(name))
        {
            Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text52"));
            return;
        }

        string? version = Version;
        if (string.IsNullOrWhiteSpace(version))
        {
            Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text49"));
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

        var res = await GameBinding.AddGameAsync(game, new OverGameGui(Model));
        if (!res)
        {
            Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text47"));
        }
        else
        {
            //自定义加载器还需要加载其他信息
            if (game.Loader == Loaders.Custom && !string.IsNullOrWhiteSpace(LoaderLocal))
            {
                var res1 = await game.SetGameLoaderAsync(LoaderLocal);
                if (!res1.State)
                {
                    Model.ShowWithOk(LanguageUtils.Get("AddGameWindow.Tab1.Text54"), () =>
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
        EnableLoaderVersion = false;
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
        Model.SubTitle = LanguageUtils.Get("AddGameWindow.Tab1.Text27");

        var res = await GameHelper.GetSupportLoaderAsync(Version);
        foreach (var item in res.Done)
        {
            _loaderTypeList.Add(item);
            LoaderTypeList.Add(item.GetName());
        }
        foreach (var item in res.Fail)
        {
            Model.Notify(string.Format(LanguageUtils.Get("AddGameWindow.Tab1.Text55"), item.GetName()));
        }
        _loaderTypeList.Add(Loaders.Custom);
        LoaderTypeList.Add(Loaders.Custom.GetName());

        IsLoad = false;
        Model.SubTitle = "";

        LoaderType = 0;
        _load = false;
        EnableLoader = true;

        Model.Notify(LanguageUtils.Get("AddGameWindow.Tab1.Text42"));
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
        EnableLoaderVersion = false;
        LoaderVersion = null;
        IsLoad = true;
        Model.SubTitle = LanguageUtils.Get("GameEditWindow.Tab1.Text36");
        var res = await GameBinding.ReloadVersionAsync();
        IsLoad = false;
        Model.SubTitle = "";
        if (!res)
        {
            Model.Show(LanguageUtils.Get("GameEditWindow.Tab1.Text45"));
            return;
        }

        GameVersionUpdate();

        Model.Notify(LanguageUtils.Get("AddGameWindow.Tab1.Text41"));
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
                GameVersionList.AddRange(await GameHelper.GetGameVersionsAsync(GameType.Release));
                break;
            case 1:
                GameVersionList.AddRange(await GameHelper.GetGameVersionsAsync(GameType.Snapshot));
                break;
            case 2:
                GameVersionList.AddRange(await GameHelper.GetGameVersionsAsync(GameType.Other));
                break;
        }
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private Task<bool> GameRequestAsync(string state)
    {
        Model.ProgressClose();
        return Model.ShowAsync(state);
    }
}
