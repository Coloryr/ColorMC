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
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab1Model : GameEditTabModel
{
    public ObservableCollection<string> GameVersionList { get; init; } = new();
    public ObservableCollection<string> LoaderVersionList { get; init; } = new();
    public ObservableCollection<string> GroupList { get; init; } = new();

    private bool _load = false;

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
    private bool _enableForge;
    [ObservableProperty]
    private bool _enableNeoForge;
    [ObservableProperty]
    private bool _enableFabric;
    [ObservableProperty]
    private bool _enableQuilt;

    [ObservableProperty]
    private bool _selectRelease = true;
    [ObservableProperty]
    private bool _selectSnapshot;
    [ObservableProperty]
    private bool _selectOther;

    [ObservableProperty]
    private bool _selectForge;
    [ObservableProperty]
    private bool _selectNeoForge;
    [ObservableProperty]
    private bool _selectFabric;
    [ObservableProperty]
    private bool _selectQuilt;

    [ObservableProperty]
    private bool _modPack;

    [ObservableProperty]
    private bool _gameRun;

    [ObservableProperty]
    private bool _enableLoader;

    public GameEditTab1Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {
        Load();
    }

    async partial void OnSelectForgeChanged(bool value)
    {
        if (value)
        {
            EnableNeoForge = false;
            EnableFabric = false;
            EnableQuilt = false;
            SelectNeoForge = false;
            SelectFabric = false;
            SelectQuilt = false;

            if (_load)
                return;
            Obj.Loader = Loaders.Forge;
            Obj.LoaderVersion = null;
            Obj.Save();

            await LoaderVersionLoad();
        }
        else
        {
            EnableLoader = false;
            EnableNeoForge = true;
            EnableFabric = true;
            EnableQuilt = true;
            LoaderVersionList.Clear();

            if (SelectFabric == false && SelectQuilt == false && SelectNeoForge == false)
            {
                Obj.Loader = Loaders.Normal;
                Obj.LoaderVersion = null;
                Obj.Save();
            }
        }
    }

    async partial void OnSelectNeoForgeChanged(bool value)
    {
        if (value)
        {
            EnableForge = false;
            EnableFabric = false;
            EnableQuilt = false;
            SelectForge = false;
            SelectFabric = false;
            SelectQuilt = false;

            if (_load)
                return;
            Obj.Loader = Loaders.NeoForge;
            Obj.LoaderVersion = null;
            Obj.Save();

            await LoaderVersionLoad();
        }
        else
        {
            EnableLoader = false;
            EnableForge = true;
            EnableFabric = true;
            EnableQuilt = true;
            LoaderVersionList.Clear();

            if (SelectFabric == false && SelectQuilt == false && SelectForge == false)
            {
                Obj.Loader = Loaders.Normal;
                Obj.LoaderVersion = null;
                Obj.Save();
            }
        }
    }

    async partial void OnSelectFabricChanged(bool value)
    {
        if (value)
        {
            EnableForge = false;
            EnableNeoForge = false;
            EnableQuilt = false;
            SelectForge = false;
            SelectQuilt = false;
            SelectNeoForge = false;

            if (_load)
                return;
            Obj.Loader = Loaders.Fabric;
            Obj.LoaderVersion = null;
            Obj.Save();

            await LoaderVersionLoad();
        }
        else
        {
            EnableLoader = false;
            EnableForge = true;
            EnableQuilt = true;
            EnableNeoForge = true;
            LoaderVersionList.Clear();

            if (SelectForge == false && SelectQuilt == false && SelectNeoForge == false)
            {
                Obj.Loader = Loaders.Normal;
                Obj.LoaderVersion = null;
                Obj.Save();
            }
        }
    }

    async partial void OnSelectQuiltChanged(bool value)
    {
        if (value)
        {
            EnableForge = false;
            EnableNeoForge = false;
            EnableFabric = false;
            SelectForge = false;
            SelectNeoForge = false;
            SelectFabric = false;

            if (_load)
                return;
            Obj.Loader = Loaders.Quilt;
            Obj.LoaderVersion = null;
            Obj.Save();

            await LoaderVersionLoad();
        }
        else
        {
            EnableLoader = false;
            EnableForge = true;
            EnableNeoForge = true;
            EnableFabric = true;
            LoaderVersionList.Clear();

            if (SelectForge == false && SelectFabric == false && SelectNeoForge == false)
            {
                Obj.Loader = Loaders.Normal;
                Obj.LoaderVersion = null;
                Obj.Save();
            }
        }
    }

    partial void OnGameVersionChanged(string value)
    {
        if (_load)
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
        if (_load)
            return;

        Obj.LoaderVersion = value;
        Obj.Save();
    }

    partial void OnGroupChanged(string? value)
    {
        if (_load)
            return;

        GameBinding.MoveGameGroup(Obj, value);
    }

    partial void OnModPackChanged(bool value)
    {
        if (_load)
            return;

        Obj.ModPack = value;
        Obj.Save();
    }

    partial void OnPIDChanged(string? value)
    {
        if (_load)
            return;

        Obj.PID = value;
        Obj.Save();
    }

    partial void OnFIDChanged(string? value)
    {
        if (_load)
            return;

        Obj.FID = value;
        Obj.Save();
    }

    [RelayCommand]
    public async Task CheckModPackUpdate()
    {
        var window = Con.Window;
        if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
        {
            window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Error3"));
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

                window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info8"));
                var item = list.First();
                res = await GameBinding.ModPackUpdate(Obj, item);
                window.ProgressInfo.Close();
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

                window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab1.Info8"));
                var item = list.data.First();
                res = await GameBinding.ModPackUpdate(Obj, item);
                window.ProgressInfo.Close();
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
    public async Task AddGroup()
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
    public async Task LoaderVersionLoad()
    {
        var window = Con.Window;

        EnableLoader = false;

        if (SelectForge == true)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info1"));
            EnableNeoForge = false;
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
        else if (SelectNeoForge == true)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info1"));
            EnableForge = false;
            EnableFabric = false;
            EnableQuilt = false;

            var list = await GameBinding.GetNeoForgeVersion(Obj.Version);
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
            EnableNeoForge = false;
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
            EnableNeoForge = false;
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
    public async Task LoaderReload()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info4"));
        var list = await GameBinding.GetForgeSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            EnableForge = true;
        }

        list = await GameBinding.GetNeoForgeSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            EnableNeoForge = true;
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
    public async Task GameVersionReload()
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
    public async Task Delete()
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
        BaseBinding.OpPath(Obj, PathType.BasePath);
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
        _load = true;

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
                case Loaders.NeoForge:
                    EnableNeoForge = true;
                    SelectNeoForge = true;
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

        _load = false;
    }
}
