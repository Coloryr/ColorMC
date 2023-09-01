using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameTab1Model : AddGameControlModel
{
    public ObservableCollection<string> GameVersionList { get; init; } = new();
    public ObservableCollection<string> LoaderVersionList { get; init; } = new();

    public List<string> VersionTypeList { get; init; } = LanguageBinding.GetVersionType();
    public ObservableCollection<string> LoaderTypeList { get; init; } = new();

    private List<Loaders> _loaderTypeList = new();

    [ObservableProperty]
    private string _version;
    [ObservableProperty]
    private string _loaderVersion;

    [ObservableProperty]
    private bool _enableLoader;

    [ObservableProperty]
    private int versionType;
    [ObservableProperty]
    private int loaderType;

    private bool _load = false;

    public AddGameTab1Model(IUserControl con) : base(con)
    {
        Update();
    }

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
        Update();
    }

    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        ProgressClose();
        var test = await ShowWait(
            string.Format(App.GetLanguage("AddGameWindow.Info2"), obj.Name));
        return test;
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
                Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
                var list = await GameBinding.GetForgeVersion(Version);
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
                list = await GameBinding.GetNeoForgeVersion(Version);
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
                list = await GameBinding.GetFabricVersion(Version);
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
                list = await GameBinding.GetQuiltVersion(Version);
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
    public async Task Add()
    {
        ColorMCCore.GameOverwirte = GameOverwirte;

        if (BaseBinding.IsDownload)
        {
            Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }

        var name = Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            Show(App.GetLanguage("AddGameWindow.Tab1.Error6"));
            return;
        }

        string? version = Version;
        if (string.IsNullOrWhiteSpace(version))
        {
            Show(App.GetLanguage("AddGameWindow.Tab1.Error7"));
            return;
        }

        var loader = _loaderTypeList[LoaderType];
        var res = await GameBinding.AddGame(name, version, loader, LoaderVersion, Group);
        if (!res)
        {
            Show(App.GetLanguage("AddGameWindow.Tab1.Error5"));
        }
        else
        {
            App.MainWindow?.Window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info7"));
            App.MainWindow?.LoadMain();

            Window.Close();
        }
    }

    [RelayCommand]
    public void AddPack()
    {
        App.ShowAddModPack();
    }

    [RelayCommand]
    public async Task VersionSelect()
    {
        _load = true;

        EnableLoader = false;
        LoaderVersionList.Clear();

        var item = Version;
        if (string.IsNullOrWhiteSpace(item))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = item;
        }

        Progress(App.GetLanguage("AddGameWindow.Tab1.Info4"));
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
        ProgressClose();

        _load = false;
    }

    [RelayCommand]
    public async Task LoadVersion()
    {
        Progress(App.GetLanguage("GameEditWindow.Info1"));
        var res = await GameBinding.ReloadVersion();
        ProgressClose();
        if (!res)
        {
            Show(App.GetLanguage("GameEditWindow.Error1"));
            return;
        }

        Update();
    }

    public async void Install(CurseForgeModObj.Data data, CurseForgeObjList.Data data1)
    {
        if (BaseBinding.IsDownload)
        {
            Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }

        Progress(App.GetLanguage("AddGameWindow.Tab1.Info8"));
        var res = await GameBinding.InstallCurseForge(data, data1, Name, Group);
        ProgressClose();
        if (!res)
        {
            Show(App.GetLanguage("AddGameWindow.Tab1.Error8"));
        }
        else
        {
            App.MainWindow?.Window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info7"));
            App.MainWindow?.LoadMain();
            Window.Close();
        }
    }

    public async void Install(ModrinthVersionObj data, ModrinthSearchObj.Hit data1)
    {
        if (BaseBinding.IsDownload)
        {
            Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }

        Progress(App.GetLanguage("AddGameWindow.Tab1.Info8"));
        var res = await GameBinding.InstallModrinth(data, data1, Name, Group);
        ProgressClose();
        if (!res)
        {
            Show(App.GetLanguage("AddGameWindow.Tab1.Error8"));
        }
        else
        {
            App.MainWindow?.Window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info7"));
            App.MainWindow?.LoadMain();
            Window.Close();
        }
    }
    private void Update()
    {
        GameVersionList.Clear();
        switch (VersionType)
        {
            case 0:
                GameVersionList.AddRange(GameBinding.GetGameVersion(true, false, false));
                break;
            case 1:
                GameVersionList.AddRange(GameBinding.GetGameVersion(false, true, false));
                break;
            case 2:
                GameVersionList.AddRange(GameBinding.GetGameVersion(false, false, true));
                break;
            case 3:
                GameVersionList.AddRange(GameBinding.GetGameVersion(true, true, true));
                break;
        }
    }

    public override void Close()
    {
        _load = true;
        GameVersionList.Clear();
        LoaderVersionList.Clear();
    }
}
