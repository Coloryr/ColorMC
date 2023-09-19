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

namespace ColorMC.Gui.UI.Model.Add.AddGame;

public partial class AddGameModel : TopModel
{
    public ObservableCollection<string> GameVersionList { get; init; } = new();
    public ObservableCollection<string> LoaderVersionList { get; init; } = new();

    public List<string> VersionTypeList { get; init; } = LanguageBinding.GetVersionType();
    public ObservableCollection<string> LoaderTypeList { get; init; } = new();

    private List<Loaders> _loaderTypeList = new();

    [ObservableProperty]
    private string _version;
    [ObservableProperty]
    private string? _loaderVersion;

    [ObservableProperty]
    private bool _enableLoader;
    [ObservableProperty]
    private bool _cloudEnable;

    [ObservableProperty]
    private int versionType;
    [ObservableProperty]
    private int loaderType = -1;

    private bool _load = false;

    async partial void OnVersionChanged(string value)
    {
        await VersionSelect();
    }

    async partial void OnLoaderTypeChanged(int value)
    {
        if (_load)
            return;

        await GetLoader();
    }

    partial void OnVersionTypeChanged(int value)
    {
        GameVersionUpdate();
    }

    [RelayCommand]
    public async Task GameCloudDownload()
    {
        Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info9"));
        var list = await GameCloudUtils.GetList();
        Model.ProgressClose();
        if (list == null)
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error9"));
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
        var res = await Model.ShowCombo(App.GetLanguage("AddGameWindow.Tab1.Info10"), list1);
        if (res.Cancel)
        {
            return;
        }

        Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info11"));
        var obj = list[res.Index];
        while (true)
        {
            if (GameBinding.GetGameByName(obj.Name) != null)
            {
                var res1 = await Model.ShowWait(App.GetLanguage("AddGameWindow.Tab1.Info12"));
                if (!res1)
                {
                    Model.ProgressClose();
                    return;
                }
                var res2 = await Model.ShowEdit(App.GetLanguage("AddGameWindow.Tab1.Text2"), obj.Name);
                if (res2.Cancel)
                {
                    return;
                }

                obj.Name = res2.Text1;
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

    [RelayCommand]
    public async Task GetLoader()
    {
        EnableLoader = false;
        if (string.IsNullOrWhiteSpace(Version))
        {
            return;
        }

        var loader = _loaderTypeList[LoaderType];
        LoaderVersionList.Clear();
        switch (loader)
        {
            case Loaders.Normal:

                break;
            case Loaders.Forge:
                Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
                var list = await GameBinding.GetForgeVersion(Version);
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
                list = await GameBinding.GetNeoForgeVersion(Version);
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
                list = await GameBinding.GetFabricVersion(Version);
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
                list = await GameBinding.GetQuiltVersion(Version);
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
    public async Task AddGame()
    {
        ColorMCCore.GameOverwirte = Tab1GameOverwirte;

        if (BaseBinding.IsDownload)
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }

        var name = Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error6"));
            return;
        }

        string? version = Version;
        if (string.IsNullOrWhiteSpace(version))
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error7"));
            return;
        }

        var loader = _loaderTypeList[LoaderType];
        var res = await GameBinding.AddGame(name, version, loader, LoaderVersion?.ToString(), Group);
        if (!res)
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error5"));
        }
        else
        {
            Done();
        }
    }

    [RelayCommand]
    public void AddOnlinePack()
    {
        App.ShowAddModPack();
    }

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

        var item = Version;
        if (string.IsNullOrWhiteSpace(item))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = item;
        }

        Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info4"));
        var list = await GameBinding.GetForgeSupportVersion();
        if (list != null && list.Contains(item))
        {
            _loaderTypeList.Add(Loaders.Forge);
            LoaderTypeList.Add(Loaders.Forge.GetName());
        }

        list = await GameBinding.GetFabricSupportVersion();
        if (list != null && list.Contains(item))
        {
            _loaderTypeList.Add(Loaders.Fabric);
            LoaderTypeList.Add(Loaders.Fabric.GetName());
        }

        list = await GameBinding.GetQuiltSupportVersion();
        if (list != null && list.Contains(item))
        {
            _loaderTypeList.Add(Loaders.Quilt);
            LoaderTypeList.Add(Loaders.Quilt.GetName());
        }
        list = await GameBinding.GetNeoForgeSupportVersion();
        if (list != null && list.Contains(item))
        {
            _loaderTypeList.Add(Loaders.NeoForge);
            LoaderTypeList.Add(Loaders.NeoForge.GetName());
        }
        Model.ProgressClose();
        LoaderType = 0;
        _load = false;
    }

    [RelayCommand]
    public async Task LoadVersion()
    {
        Model.Progress(App.GetLanguage("GameEditWindow.Info1"));
        var res = await GameBinding.ReloadVersion();
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.GetLanguage("GameEditWindow.Error1"));
            return;
        }

        GameVersionUpdate();
    }

    public async void Install(CurseForgeModObj.Data data, CurseForgeObjList.Data data1)
    {
        if (BaseBinding.IsDownload)
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }

        Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info8"));
        var res = await GameBinding.InstallCurseForge(data, data1, Name, Group);
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error8"));
        }
        else
        {
            Done();
        }
    }

    public async void Install(ModrinthVersionObj data, ModrinthSearchObj.Hit data1)
    {
        if (BaseBinding.IsDownload)
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }

        Model.Progress(App.GetLanguage("AddGameWindow.Tab1.Info8"));
        var res = await GameBinding.InstallModrinth(data, data1, Name, Group);
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error8"));
        }
        else
        {
            Done();
        }
    }
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

    private async Task<bool> Tab1GameOverwirte(GameSettingObj obj)
    {
        Model.ProgressClose();
        var test = await Model.ShowWait(
            string.Format(App.GetLanguage("AddGameWindow.Info2"), obj.Name));
        return test;
    }

    private void Done()
    {
        var model = (App.MainWindow?.DataContext as MainModel);
        model?.Model.Notify(App.GetLanguage("AddGameWindow.Tab1.Info7"));
        App.MainWindow?.LoadMain();
        Dispatcher.UIThread.Post(() =>
        {
            WindowClose();
        });
    }
}
