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

public partial class AddGameTab1Model : AddGameTabModel
{
    public ObservableCollection<string> GameVersionList { get; init; } = new();
    public ObservableCollection<string> LoaderVersionList { get; init; } = new();


    [ObservableProperty]
    private string version;
    [ObservableProperty]
    private string loaderVersion;


    [ObservableProperty]
    private bool enableLoader;
    [ObservableProperty]
    private bool enableForge;
    [ObservableProperty]
    private bool enableFabric;
    [ObservableProperty]
    private bool enableQuilt;
    [ObservableProperty]
    private bool selectForge;
    [ObservableProperty]
    private bool selectFabric;
    [ObservableProperty]
    private bool selectQuilt;
    [ObservableProperty]
    private bool selectRelease = true;
    [ObservableProperty]
    private bool selectSnapshot;
    [ObservableProperty]
    private bool selectOther;

    private bool change = false;

    public AddGameTab1Model(IUserControl con) : base(con)
    {
        Update();
    }

    partial void OnVersionChanged(string value)
    {
        VersionSelect();
    }

    async partial void OnSelectForgeChanged(bool value)
    {
        if (change)
            return;

        var window = Con.Window;
        if (value)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info1"));
            EnableFabric = false;
            EnableQuilt = false;

            var list = await GameBinding.GetForgeVersion(Version);
            window.ProgressInfo.Close();
            if (list == null)
            {
                window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
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
            EnableQuilt = true;
        }
    }

    async partial void OnSelectFabricChanged(bool value)
    {
        if (change)
            return;

        var window = Con.Window;
        if (value)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info2"));
            EnableForge = false;
            EnableQuilt = false;

            var list = await GameBinding.GetFabricVersion(Version);
            window.ProgressInfo.Close();
            if (list == null)
            {
                window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
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
            EnableQuilt = true;
        }
    }

    async partial void OnSelectQuiltChanged(bool value)
    {
        if (change)
            return;

        var window = Con.Window;
        if (value)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info3"));
            EnableForge = false;
            EnableFabric = false;

            var list = await GameBinding.GetQuiltVersion(Version);
            window.ProgressInfo.Close();
            if (list == null)
            {
                window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error1"));
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
        var window = Con.Window;
        window.ProgressInfo.Close();
        var test = await window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("AddGameWindow.Info2"), obj.Name));
        return test;
    }


    [RelayCommand]
    public async void GetLoader()
    {
        EnableLoader = false;

        var item = Version;
        if (string.IsNullOrWhiteSpace(item))
        {
            return;
        }

        var window = Con.Window;
        if (SelectForge == true)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info1"));
            EnableFabric = false;
            EnableQuilt = false;

            var list = await GameBinding.GetForgeVersion(item);
            window.ProgressInfo.Close();
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
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info2"));
            EnableForge = false;
            EnableQuilt = false;

            var list = await GameBinding.GetFabricVersion(item);
            window.ProgressInfo.Close();
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
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info3"));
            EnableForge = false;
            EnableFabric = false;

            var list = await GameBinding.GetQuiltVersion(item);
            window.ProgressInfo.Close();
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
    public async void Add()
    {
        ColorMCCore.GameOverwirte = GameOverwirte;

        var window = Con.Window;
        if (BaseBinding.IsDownload)
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }

        var name = Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error6"));
            return;
        }

        string? version = Version;
        if (string.IsNullOrWhiteSpace(version))
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error7"));
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
        }

        var res = await GameBinding.AddGame(name, version, loader, LoaderVersion, Group);
        if (!res)
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error5"));
        }
        else
        {
            App.MainWindow?.Window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info7"));
            App.MainWindow?.Load();

            window.Close();
        }
    }

    [RelayCommand]
    public void AddPack()
    {
        App.ShowAddModPack();
    }

    [RelayCommand]
    public async void VersionSelect()
    {
        change = true;

        EnableForge = false;
        EnableFabric = false;
        EnableQuilt = false;

        SelectForge = false;
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

        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info4"));
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
        window.ProgressInfo.Close();

        change = false;
    }

    [RelayCommand]
    public async void LoadVersion()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Info1"));
        var res = await GameBinding.ReloadVersion();
        window.ProgressInfo.Close();
        if (!res)
        {
            window.OkInfo.Show(App.GetLanguage("GameEditWindow.Error1"));
            return;
        }

        Update();
    }


    public async void Install(CurseForgeObjList.Data.LatestFiles data, CurseForgeObjList.Data data1)
    {
        var window = Con.Window;
        if (BaseBinding.IsDownload)
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }

        window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info8"));
        var res = await GameBinding.InstallCurseForge(data, data1, Name, Group);
        window.ProgressInfo.Close();
        if (!res)
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error8"));
        }
        else
        {
            App.MainWindow?.Window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info7"));
            App.MainWindow?.Load();
            window.Close();
        }
    }

    public async void Install(ModrinthVersionObj data, ModrinthSearchObj.Hit data1)
    {
        var window = Con.Window;
        if (BaseBinding.IsDownload)
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }

        window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info8"));
        var res = await GameBinding.InstallModrinth(data, data1, Name, Group);
        window.ProgressInfo.Close();
        if (!res)
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error8"));
        }
        else
        {
            App.MainWindow?.Window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info7"));
            App.MainWindow?.Load();
            window.Close();
        }
    }
    private void Update()
    {
        GameVersionList.Clear();
        GameVersionList.AddRange(GameBinding.GetGameVersion(SelectRelease,
            SelectSnapshot, SelectOther));
    }
}
