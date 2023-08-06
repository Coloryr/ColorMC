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

public partial class GameEditTab1Model : GameEditModel
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
    public void ExportGame()
    {
        App.ShowGameExport(Obj);
    }

    [RelayCommand]
    public async Task CheckModPackUpdate()
    {
        if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
        {
            Show(App.GetLanguage("GameEditWindow.Tab1.Error3"));
            return;
        }

        Progress(App.GetLanguage("GameEditWindow.Tab1.Info2"));
        if (Funtions.CheckNotNumber(PID) || Funtions.CheckNotNumber(FID))
        {
            var list = await ModrinthAPI.GetFileVersions(PID, Obj.Version, Obj.Loader);
            ProgressClose();
            if (list == null)
            {
                Show(App.GetLanguage("GameEditWindow.Tab1.Info3"));
            }
            else if (list.Count == 0)
            {
                Notify(App.GetLanguage("GameEditWindow.Tab1.Info4"));
            }
            else if (list[0].id.ToString() == FID)
            {
                Notify(App.GetLanguage("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await ShowWait(App.GetLanguage("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                Progress(App.GetLanguage("GameEditWindow.Tab1.Info8"));
                var item = list[0];
                res = await GameBinding.ModPackUpdate(Obj, item);
                ProgressClose();
                if (!res)
                {
                    Show(App.GetLanguage("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    Notify(App.GetLanguage("GameEditWindow.Tab1.Info7"));
                    FID = item.id.ToString();
                }
            }
        }
        else
        {
            var list = await CurseForgeAPI.GetCurseForgeFiles(PID, Obj.Version);
            ProgressClose();
            if (list == null)
            {
                Show(App.GetLanguage("GameEditWindow.Tab1.Info3"));
            }
            else if (list.data.Count == 0)
            {
                Notify(App.GetLanguage("GameEditWindow.Tab1.Info4"));
            }
            else if (list.data[0].id.ToString() == FID)
            {
                Notify(App.GetLanguage("GameEditWindow.Tab1.Info5"));
            }
            else
            {
                var res = await ShowWait(App.GetLanguage("GameEditWindow.Tab1.Info6"));
                if (!res)
                {
                    return;
                }

                Progress(App.GetLanguage("GameEditWindow.Tab1.Info8"));
                var item = list.data[0];
                res = await GameBinding.ModPackUpdate(Obj, item);
                ProgressClose();
                if (!res)
                {
                    Show(App.GetLanguage("GameEditWindow.Tab1.Error2"));
                }
                else
                {
                    Notify(App.GetLanguage("GameEditWindow.Tab1.Info7"));
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
        var (Cancel, Text1) = await ShowOne(App.GetLanguage("AddGameWindow.Tab1.Info5"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text1))
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(Text1))
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Error3"));
            return;
        }

        Notify(App.GetLanguage("AddGameWindow.Tab1.Info6"));

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    [RelayCommand]
    public async Task LoaderVersionLoad()
    {
        EnableLoader = false;

        if (SelectForge == true)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
            EnableNeoForge = false;
            EnableFabric = false;
            EnableQuilt = false;

            var list = await GameBinding.GetForgeVersion(Obj.Version);
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
        else if (SelectNeoForge == true)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info1"));
            EnableForge = false;
            EnableFabric = false;
            EnableQuilt = false;

            var list = await GameBinding.GetNeoForgeVersion(Obj.Version);
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
        else if (SelectFabric == true)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info2"));
            EnableNeoForge = false;
            EnableForge = false;
            EnableQuilt = false;

            var list = await GameBinding.GetFabricVersion(Obj.Version);
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
        else if (SelectQuilt == true)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab1.Info3"));
            EnableForge = false;
            EnableNeoForge = false;
            EnableFabric = false;

            var list = await GameBinding.GetQuiltVersion(Obj.Version);
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
    }

    [RelayCommand]
    public async Task LoaderReload()
    {
        Progress(App.GetLanguage("AddGameWindow.Tab1.Info4"));
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
        ProgressClose();
    }

    [RelayCommand]
    public async Task GameVersionReload()
    {
        Progress(App.GetLanguage("GameEditWindow.Info1"));
        var res = await GameBinding.ReloadVersion();
        ProgressClose();
        if (!res)
        {
            Show(App.GetLanguage("GameEditWindow.Error1"));
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
        if (BaseBinding.IsGameRun(Obj))
        {
            Show(App.GetLanguage("GameEditWindow.Tab1.Error1"));
            return;
        }

        var res = await ShowWait(string.Format(
            App.GetLanguage("GameEditWindow.Tab1.Info1"), Obj.Name));
        if (!res)
            return;

        var res1 = await GameBinding.DeleteGame(Obj);
        if (!res1)
        {
            Show(App.GetLanguage("MainWindow.Info37"));
        }

        Window.Close();

    }

    [RelayCommand]
    public void Open()
    {
        PathBinding.OpPath(Obj, PathType.BasePath);
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
