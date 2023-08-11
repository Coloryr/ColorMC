using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameTab1Model : AddGameControlModel
{
    public ObservableCollection<string> GameVersionList { get; init; } = new();
    public ObservableCollection<string> LoaderVersionList { get; init; } = new();

    [ObservableProperty]
    private string _version;
    [ObservableProperty]
    private string _loaderVersion;

    [ObservableProperty]
    private bool _enableLoader;

    [ObservableProperty]
    private bool _enableForge;
    [ObservableProperty]
    private bool _enableNeoForge;
    [ObservableProperty]
    private bool _enableFabric;
    [ObservableProperty]
    private bool _enableQuilt;

    [ObservableProperty]
    private bool _selectForge;
    [ObservableProperty]
    private bool _selectNeoForge;
    [ObservableProperty]
    private bool _selectFabric;
    [ObservableProperty]
    private bool _selectQuilt;

    [ObservableProperty]
    private bool _selectRelease = true;
    [ObservableProperty]
    private bool _selectSnapshot;
    [ObservableProperty]
    private bool _selectOther;

    private bool _change = false;

    public AddGameTab1Model(IUserControl con) : base(con)
    {
        Update();
    }

    async partial void OnVersionChanged(string value)
    {
        await VersionSelect();
    }

    async partial void OnSelectForgeChanged(bool value)
    {
        if (_change)
            return;

        if (value)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
            EnableFabric = false;
            EnableNeoForge = false;
            EnableQuilt = false;

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
        }
        else
        {
            EnableLoader = false;
            LoaderVersionList.Clear();
            EnableFabric = true;
            EnableNeoForge = true;
            EnableQuilt = true;
        }
    }

    async partial void OnSelectNeoForgeChanged(bool value)
    {
        if (_change)
            return;

        if (value)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
            EnableFabric = false;
            EnableForge = false;
            EnableQuilt = false;

            var list = await GameBinding.GetNeoForgeVersion(Version);
            ProgressClose();
            if (list == null)
            {
                Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
                return;
            }

            EnableLoader = true;
            LoaderVersionList.Clear();
            LoaderVersionList.AddRange(list);
        }
        else
        {
            EnableLoader = false;
            LoaderVersionList.Clear();
            EnableFabric = true;
            EnableForge = true;
            EnableQuilt = true;
        }
    }

    async partial void OnSelectFabricChanged(bool value)
    {
        if (_change)
            return;


        if (value)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info2"));
            EnableForge = false;
            EnableNeoForge = false;
            EnableQuilt = false;

            var list = await GameBinding.GetFabricVersion(Version);
            ProgressClose();
            if (list == null)
            {
                Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
                return;
            }

            EnableLoader = true;
            LoaderVersionList.Clear();
            LoaderVersionList.AddRange(list);
        }
        else
        {
            EnableLoader = false;
            LoaderVersionList.Clear();
            EnableForge = true;
            EnableNeoForge = true;
            EnableQuilt = true;
        }
    }

    async partial void OnSelectQuiltChanged(bool value)
    {
        if (_change)
            return;

        if (value)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info3"));
            EnableForge = false;
            EnableNeoForge = false;
            EnableFabric = false;

            var list = await GameBinding.GetQuiltVersion(Version);
            ProgressClose();
            if (list == null)
            {
                Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
                return;
            }

            EnableLoader = true;
            LoaderVersionList.Clear();
            LoaderVersionList.AddRange(list);
        }
        else
        {
            EnableLoader = false;
            LoaderVersionList.Clear();
            EnableForge = true;
            EnableNeoForge = true;
            EnableFabric = true;
        }
    }

    partial void OnSelectReleaseChanged(bool value)
    {
        Update();
    }

    partial void OnSelectSnapshotChanged(bool value)
    {
        Update();
    }

    partial void OnSelectOtherChanged(bool value)
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

        var item = Version;
        if (string.IsNullOrWhiteSpace(item))
        {
            return;
        }

        if (SelectForge == true)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
            EnableFabric = false;
            EnableNeoForge = false;
            EnableQuilt = false;

            var list = await GameBinding.GetForgeVersion(item);
            ProgressClose();
            if (list == null)
            {
                return;
            }

            EnableLoader = true;
            LoaderVersionList.Clear();
            LoaderVersionList.AddRange(list);
        }
        else if (SelectFabric == true)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info2"));
            EnableForge = false;
            EnableNeoForge = false;
            EnableQuilt = false;

            var list = await GameBinding.GetFabricVersion(item);
            ProgressClose();
            if (list == null)
            {
                return;
            }

            EnableLoader = true;
            LoaderVersionList.Clear();
            LoaderVersionList.AddRange(list);
        }
        else if (SelectQuilt == true)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info3"));
            EnableForge = false;
            EnableNeoForge = false;
            EnableFabric = false;

            var list = await GameBinding.GetQuiltVersion(item);
            ProgressClose();
            if (list == null)
            {
                return;
            }

            EnableLoader = true;
            LoaderVersionList.Clear();
            LoaderVersionList.AddRange(list);
        }
        else if (SelectNeoForge == true)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info3"));
            EnableForge = false;
            EnableQuilt = false;
            EnableFabric = false;

            var list = await GameBinding.GetNeoForgeVersion(item);
            ProgressClose();
            if (list == null)
            {
                return;
            }

            EnableLoader = true;
            LoaderVersionList.Clear();
            LoaderVersionList.AddRange(list);
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

        Loaders loader = Loaders.Normal;
        if (!string.IsNullOrWhiteSpace(LoaderVersion))
        {
            if (SelectForge)
            {
                loader = Loaders.Forge;
            }
            else if (SelectFabric)
            {
                loader = Loaders.Fabric;
            }
            else if (SelectQuilt)
            {
                loader = Loaders.Quilt;
            }
            else if (SelectNeoForge)
            {
                loader = Loaders.NeoForge;
            }
        }

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
        _change = true;

        EnableForge = false;
        EnableNeoForge = false;
        EnableFabric = false;
        EnableQuilt = false;

        SelectForge = false;
        SelectNeoForge = false;
        SelectFabric = false;
        SelectQuilt = false;

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
            EnableForge = true;
        }

        list = await GameBinding.GetFabricSupportVersion();
        if (list != null && list.Contains(item))
        {
            EnableFabric = true;
        }

        list = await GameBinding.GetQuiltSupportVersion();
        if (list != null && list.Contains(item))
        {
            EnableQuilt = true;
        }
        list = await GameBinding.GetNeoForgeSupportVersion();
        if (list != null && list.Contains(item))
        {
            EnableNeoForge = true;
        }
        ProgressClose();

        _change = false;
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
        GameVersionList.AddRange(GameBinding.GetGameVersion(SelectRelease,
            SelectSnapshot, SelectOther));
    }

    public override void Close()
    {
        GameVersionList.Clear();
        LoaderVersionList.Clear();
    }
}
