using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
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

    /// <summary>
    /// 版本类型
    /// </summary>
    [ObservableProperty]
    private int versionType;
    /// <summary>
    /// 加载器类型
    /// </summary>
    [ObservableProperty]
    private int loaderType = -1;

    /// <summary>
    /// 加载器类型列表
    /// </summary>
    private readonly List<Loaders> _loaderTypeList = [];

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
        var res = await PathBinding.SelectFile(FileType.Loader);
        if (res == null)
        {
            return;
        }

        LoaderLocal = res;
    }

    [RelayCommand]
    public async Task ServerPackDownload()
    {
        var (Cancel, Text) = await Model.ShowInputOne(App.Lang("AddGameWindow.Tab1.Info13"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error14"));
            return;
        }

        if (!Text.EndsWith('/'))
        {
            Text += '/';
        }

        Model.Progress(App.Lang("AddGameWindow.Tab1.Info14"));
        var res1 = await GameBinding.DownloadServerPack(Model, Name, Group, Text);
        Model.ProgressClose();
        if (!res1.Item1 && res1.Item2 != null)
        {
            Model.Show(res1.Item2!);
        }
        else
        {
            Done();
        }
    }

    /// <summary>
    /// 下载云同步游戏实例
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task GameCloudDownload()
    {
        Model.Progress(App.Lang("AddGameWindow.Tab1.Info9"));
        var list = await GameCloudUtils.GetList();
        Model.ProgressClose();
        if (list == null)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error9"));
            return;
        }
        var list1 = new List<string>();
        list.ForEach(item =>
        {
            if (!string.IsNullOrEmpty(item.Name) && GameBinding.GetGame(item.UUID) == null)
            {
                list1.Add(item.Name);
            }
        });
        var res = await Model.ShowCombo(App.Lang("AddGameWindow.Tab1.Info10"), list1);
        if (res.Cancel)
        {
            return;
        }

        Model.Progress(App.Lang("AddGameWindow.Tab1.Info11"));
        var obj = list[res.Index];
        while (true)
        {
            if (GameBinding.GetGameByName(obj.Name) != null)
            {
                var res1 = await Model.ShowWait(App.Lang("AddGameWindow.Tab1.Info12"));
                if (!res1)
                {
                    Model.ProgressClose();
                    return;
                }
                var (Cancel, Text1) = await Model.ShowEdit(App.Lang("AddGameWindow.Tab1.Text2"), obj.Name);
                if (Cancel)
                {
                    return;
                }

                obj.Name = Text1!;
            }
            else
            {
                break;
            }
        }
        var res3 = await GameBinding.DownloadCloud(obj, Group);
        Model.ProgressClose();
        if (!res3.Item1)
        {
            Model.Show(res3.Item2!);
            return;
        }

        App.ShowGameCloud(GameBinding.GetGame(obj.UUID!)!);
        Done();
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
                Model.Progress(App.Lang("AddGameWindow.Tab1.Info1"));
                var list = await WebBinding.GetForgeVersion(Version);
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
                list = await WebBinding.GetNeoForgeVersion(Version);
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
                list = await WebBinding.GetFabricVersion(Version);
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
                list = await WebBinding.GetQuiltVersion(Version);
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
                list = await WebBinding.GetOptifineVersion(Version);
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
            case Loaders.Custom:
                CustomLoader = true;
                break;
        }
    }

    /// <summary>
    /// 添加游戏
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddGame()
    {
        ColorMCCore.GameOverwirte = Tab1GameOverwirte;
        ColorMCCore.GameRequest = Tab1GameRequest;

        if (BaseBinding.IsDownload)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error4"));
            return;
        }

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
        var res = await GameBinding.AddGame(name, version, loader, LoaderVersion?.ToString(), Group, LoaderLocal, OffLib);
        if (!res)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error5"));
        }
        else
        {
            Done();
        }
    }

    /// <summary>
    /// 添加整合包
    /// </summary>
    [RelayCommand]
    public void AddOnlinePack()
    {
        App.ShowAddModPack();
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

        Model.Progress(App.Lang("AddGameWindow.Tab1.Info4"));

        var loaders = await GameBinding.GetSupportLoader(Version);
        foreach (var item in loaders)
        {
            _loaderTypeList.Add(item);
            LoaderTypeList.Add(item.GetName());
        }

        _loaderTypeList.Add(Loaders.Custom);
        LoaderTypeList.Add(Loaders.Custom.GetName());

        Model.ProgressClose();
        LoaderType = 0;
        _load = false;
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
        Model.Progress(App.Lang("GameEditWindow.Info1"));
        var res = await GameBinding.ReloadVersion();
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("GameEditWindow.Error1"));
            return;
        }

        GameVersionUpdate();
    }

    /// <summary>
    /// 安装整合包
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="data1">数据</param>
    public async void Install(CurseForgeModObj.Data data, CurseForgeObjList.Data data1)
    {
        if (BaseBinding.IsDownload)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error4"));
            return;
        }

        ColorMCCore.GameOverwirte = Tab2GameOverwirte;
        ColorMCCore.GameRequest = Tab2GameRequest;

        Model.Progress(App.Lang("AddGameWindow.Tab1.Info8"));
        var res = await GameBinding.InstallCurseForge(data, data1, Name, Group);
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error8"));
        }
        else
        {
            Done();
        }
    }

    /// <summary>
    /// 安装整合包
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="data1">数据</param>
    public async void Install(ModrinthVersionObj data, ModrinthSearchObj.Hit data1)
    {
        if (BaseBinding.IsDownload)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error4"));
            return;
        }

        ColorMCCore.GameOverwirte = Tab2GameOverwirte;
        ColorMCCore.GameRequest = Tab2GameRequest;

        Model.Progress(App.Lang("AddGameWindow.Tab1.Info8"));
        var res = await GameBinding.InstallModrinth(data, data1, Name, Group, null);
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error8"));
        }
        else
        {
            Done();
        }
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
                GameVersionList.AddRange(await GameBinding.GetGameVersion(true, false, false));
                break;
            case 1:
                GameVersionList.AddRange(await GameBinding.GetGameVersion(false, true, false));
                break;
            case 2:
                GameVersionList.AddRange(await GameBinding.GetGameVersion(false, false, true));
                break;
            case 3:
                GameVersionList.AddRange(await GameBinding.GetGameVersion(true, true, true));
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
        var test = await Model.ShowWait(
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
        return Model.ShowWait(state);
    }

    /// <summary>
    /// 添加完成
    /// </summary>
    private void Done()
    {
        var model = (App.MainWindow?.DataContext as MainModel);
        model?.Model.Notify(App.Lang("AddGameWindow.Tab1.Info7"));
        App.MainWindow?.LoadMain();
        Dispatcher.UIThread.Post(WindowClose);
    }
}
