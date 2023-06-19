using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab1Model : GameEditTabModel
{
    public ObservableCollection<string> GameVersionList { get; init; } = new();
    public ObservableCollection<string> LoaderVersionList { get; init; } = new();
    public ObservableCollection<string> GroupList { get; init; } = new();

    private bool load = false;

    [ObservableProperty]
    private string gameVersion;
    [ObservableProperty]
    private string? loaderVersion;
    [ObservableProperty]
    private string? group;
    [ObservableProperty]
    private string? pID;
    [ObservableProperty]
    private string? fID;

    [ObservableProperty]
    private bool enableForge;
    [ObservableProperty]
    private bool enableFabric;
    [ObservableProperty]
    private bool enableQuilt;

    [ObservableProperty]
    private bool selectRelease = true;
    [ObservableProperty]
    private bool selectSnapshot;
    [ObservableProperty]
    private bool selectOther;

    [ObservableProperty]
    private bool selectForge;
    [ObservableProperty]
    private bool selectFabric;
    [ObservableProperty]
    private bool selectQuilt;

    [ObservableProperty]
    private bool modPack;

    [ObservableProperty]
    private bool gameRun;

    [ObservableProperty]
    private bool enableLoader;

    public GameEditTab1Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {
        Load();
    }

    partial void OnSelectForgeChanged(bool value)
    {
        if (value)
        {
            EnableFabric = false;
            SelectFabric = false;
            EnableQuilt = false;
            SelectQuilt = false;

            if (load)
                return;
            Obj.Loader = Loaders.Forge;
            Obj.LoaderVersion = null;
            Obj.Save();

            LoaderVersionLoad();
        }
        else
        {
            EnableLoader = false;
            EnableFabric = true;
            EnableQuilt = true;
            LoaderVersionList.Clear();
        }
    }

    partial void OnSelectFabricChanged(bool value)
    {
        if (value)
        {
            EnableForge = false;
            SelectForge = false;
            EnableQuilt = false;
            SelectQuilt = false;

            if (load)
                return;
            Obj.Loader = Loaders.Fabric;
            Obj.LoaderVersion = null;
            Obj.Save();

            LoaderVersionLoad();
        }
        else
        {
            EnableLoader = false;
            EnableForge = true;
            EnableQuilt = true;
            LoaderVersionList.Clear();
        }
    }

    partial void OnSelectQuiltChanged(bool value)
    {
        if (value)
        {
            EnableForge = false;
            SelectForge = false;
            EnableFabric = false;
            SelectFabric = false;

            if (load)
                return;
            Obj.Loader = Loaders.Quilt;
            Obj.LoaderVersion = null;
            Obj.Save();

            LoaderVersionLoad();
        }
        else
        {
            EnableLoader = false;
            EnableForge = true;
            EnableFabric = true;
            LoaderVersionList.Clear();
        }
    }

    partial void OnGameVersionChanged(string value)
    {
        if (load)
            return;

        LoaderVersion = null;
        LoaderVersionList.Clear();
        SelectForge = false;
        SelectFabric = false;
        SelectQuilt = false;

        Obj.Version = value;
        Obj.Loader = Loaders.Normal;
        Obj.LoaderVersion = null;
        Obj.Save();
    }

    partial void OnLoaderVersionChanged(string? value)
    {
        if (load)
            return;

        Obj.LoaderVersion = value;
        Obj.Save();
    }

    partial void OnGroupChanged(string? value)
    {
        if (load)
            return;

        GameBinding.MoveGameGroup(Obj, value);
    }

    partial void OnModPackChanged(bool value)
    {
        if (load)
            return;

        Obj.ModPack = value;
        Obj.Save();
    }

    partial void OnPIDChanged(string? value)
    {
        if (load)
            return;

        Obj.PID = value;
        Obj.Save();
    }

    partial void OnFIDChanged(string? value)
    {
        if (load)
            return;

        Obj.FID = value;
        Obj.Save();
    }

    [RelayCommand]
    public async void CheckModPackUpdate()
    {
        var window = Con.Window;
        if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
        {
            return;
        }

        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info2"));
        if (Funtcions.CheckNotNumber(PID) || Funtcions.CheckNotNumber(FID))
        {
            var list = await ModrinthAPI.GetFileVersions(PID, Obj.Version, Obj.Loader);
            window.ProgressInfo.Close();
            if (list == null)
            {
                window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info3"));
            }
            else if (list.Count == 0)
            {
                window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info4"));
            }
            else if (list.First().id.ToString() == FID)
            {
                window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await window.OkInfo.ShowWait(App.GetLanguage("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                var item = list.First();
                res = await GameBinding.ModPackUpdate(Obj, item);
                if (!res)
                {
                    window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info7"));
                    FID = item.id.ToString();
                }
            }
        }
        else
        {
            var list = await CurseForgeAPI.GetCurseForgeFiles(PID, Obj.Version);
            window.ProgressInfo.Close();
            if (list == null)
            {
                window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info3"));
            }
            else if (list.data.Count == 0)
            {
                window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info4"));
            }
            else if (list.data.First().id.ToString() == FID)
            {
                window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await window.OkInfo.ShowWait(App.GetLanguage("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                var item = list.data.First();
                res = await GameBinding.ModPackUpdate(Obj, item);
                if (!res)
                {
                    window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info7"));
                    FID = item.id.ToString();
                }
            }
        }
    }

    [RelayCommand]
    public void OpenGameLog()
    {
        App.ShowGameLog(Obj);
    }

    [RelayCommand]
    public async void AddGroup()
    {
        var window = Con.Window;
        await window.InputInfo.ShowOne(App.GetLanguage("AddGameWindow.Tab1.Info5"), false);
        if (window.InputInfo.Cancel)
        {
            return;
        }

        var res = window.InputInfo.Read().Item1;
        if (string.IsNullOrWhiteSpace(res))
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(res))
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error3"));
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info6"));

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    [RelayCommand]
    public async void LoaderVersionLoad()
    {
        var window = Con.Window;

        EnableLoader = false;

        if (SelectForge == true)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info1"));
            EnableFabric = false;
            EnableQuilt = false;

            var list = await GameBinding.GetForgeVersion(Obj.Version);
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
        else if (SelectFabric == true)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info2"));
            EnableForge = false;
            EnableQuilt = false;

            var list = await GameBinding.GetFabricVersion(Obj.Version);
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
        else if (SelectQuilt == true)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info3"));
            EnableForge = false;
            EnableFabric = false;

            var list = await GameBinding.GetQuiltVersion(Obj.Version);
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
    }

    [RelayCommand]
    public async void LoaderReload()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info4"));
        var list = await GameBinding.GetForgeSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            EnableForge = true;
        }

        list = await GameBinding.GetFabricSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            EnableFabric = true;
        }

        list = await GameBinding.GetQuiltSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            EnableQuilt = true;
        }
        window.ProgressInfo.Close();
    }

    [RelayCommand]
    public async void GameVersionReload()
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

        LoaderVersion = null;
        SelectForge = false;
        SelectFabric = false;
        SelectQuilt = false;

        Load1();
    }

    [RelayCommand]
    public async void Delete()
    {
        var window = Con.Window;
        if (BaseBinding.IsGameRun(Obj))
        {
            window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Error1"));
            return;
        }

        var res = await window.OkInfo.ShowWait(string.Format(
            App.GetLanguage("GameEditWindow.Tab1.Info1"), Obj.Name));
        if (!res)
            return;

        var res1 = await GameBinding.DeleteGame(Obj);
        if (!res1)
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Info37"));
        }

        window.Close();

    }

    [RelayCommand]
    public void Open()
    {
        BaseBinding.OpPath(Obj.GetBasePath());
    }

    [RelayCommand]
    public void OpenOptifine()
    {
        App.ShowAdd(Obj, FileType.Optifne);
    }

    [RelayCommand]
    public void OpenServerPack()
    {
        App.ShowServerPack(Obj);
    }

    [RelayCommand]
    public void OpenConfigEdit()
    {
        App.ShowConfigEdit(Obj);
    }

    private void Load1()
    {
        GameVersionList.Clear();
        GameVersionList.AddRange(GameBinding.GetGameVersion(SelectRelease,
            SelectSnapshot, SelectOther));
    }

    private void Load2()
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    public void GameStateChange()
    {
        GameRun = BaseBinding.IsGameRun(Obj);
    }

    public void Load()
    {
        load = true;

        if (Obj.GameType == GameType.Snapshot)
        {
            SelectSnapshot = true;
        }
        else if (Obj.GameType == GameType.Other)
        {
            SelectOther = true;
        }

        if (Obj.Loader != Loaders.Normal)
        {
            switch (Obj.Loader)
            {
                case Loaders.Forge:
                    EnableForge = true;
                    SelectForge = true;
                    break;
                case Loaders.Fabric:
                    EnableFabric = true;
                    SelectFabric = true;
                    break;
                case Loaders.Quilt:
                    EnableQuilt = true;
                    SelectQuilt = true;
                    break;
            }

            EnableLoader = false;
            LoaderVersionList.Clear();
            if (Obj.LoaderVersion != null)
            {
                LoaderVersionList.Add(Obj.LoaderVersion);
            }
        }

        Load1();
        Load2();

        LoaderVersion = Obj.LoaderVersion;
        ModPack = Obj.ModPack;
        GameVersion = Obj.Version;
        Group = Obj.GroupName;
        FID = Obj.FID;
        PID = Obj.PID;

        GameRun = BaseBinding.IsGameRun(Obj);

        load = false;
    }
}
