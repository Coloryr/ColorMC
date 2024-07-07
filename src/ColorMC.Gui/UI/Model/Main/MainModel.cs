using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Utils;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel : TopModel, IMainTop
{
    public const string SwitchView = "SwitchView";

    public bool IsLaunch;
    public bool IsFirst = true;

    public ObservableCollection<string> GroupList { get; init; } = [];
    public ObservableCollection<GamesModel> GameGroups { get; init; } = [];

    public bool IsPhone { get; } = SystemInfo.Os == OsType.Android;

    private static readonly string[] _side =
    [
        "/Resource/Icon/left.svg",
        "/Resource/Icon/menu.svg"
    ];

    private readonly Semaphore _semaphore = new(0, 2);
    private readonly Dictionary<string, GameItemModel> Launchs = [];

    private bool _isplay = true;
    private bool _isCancel;

    [ObservableProperty]
    private int _live2dWidth = 300;
    [ObservableProperty]
    private int _live2dHeight = 300;
    [ObservableProperty]
    private HorizontalAlignment _l2dPos = HorizontalAlignment.Right;
    [ObservableProperty]
    private VerticalAlignment _l2dPos1 = VerticalAlignment.Top;

    [ObservableProperty]
    private (string, ushort) _server;

    [ObservableProperty]
    private string? _groupItem;
    [ObservableProperty]
    private string _message;
    [ObservableProperty]
    private string _userId;
    [ObservableProperty]
    private string _userType;

    [ObservableProperty]
    private bool _isNotGame;
    [ObservableProperty]
    private bool _motdDisplay;
    [ObservableProperty]
    private bool _isGameError;
    [ObservableProperty]
    private bool _isOneGame;
    [ObservableProperty]
    private bool _isHeadLoad;
    [ObservableProperty]
    private bool _musicDisplay;

    [ObservableProperty]
    private GameItemModel? _game;
    [ObservableProperty]
    private GameItemModel? _oneGame;

    [ObservableProperty]
    private Bitmap _head = ImageManager.LoadIcon;

    [ObservableProperty]
    private bool _render = true;

    [ObservableProperty]
    private bool _haveUpdate;

    [ObservableProperty]
    private bool _topSide = true;
    [ObservableProperty]
    private bool _topSide1 = true;
    [ObservableProperty]
    private bool _topSide2 = false;
    [ObservableProperty]
    private string _sidePath = _side[0];

    [ObservableProperty]
    private string _gameSearchText;
    [ObservableProperty]
    private bool _gameSearch;
    [ObservableProperty]
    private bool _lowFps;

    [ObservableProperty]
    private string _helloText;

    [ObservableProperty]
    private float _musicVolume;

    private bool _isNewUpdate;
    private string _updateStr;

    private bool _isGetNewInfo;

    public MainModel(BaseModel model) : base(model)
    {
        App.SkinLoad += App_SkinLoad;
        App.UserEdit += LoadUser;
    }

    partial void OnGameSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            foreach (var item in GameGroups)
            {
                item.DisplayAll();
            }
        }
        else
        {
            foreach (var item in GameGroups)
            {
                item.Display(value);
            }
        }
    }

    partial void OnTopSideChanged(bool value)
    {
        if (value)
        {
            SidePath = _side[1];
        }
        else
        {
            SidePath = _side[0];
        }
    }

    [RelayCommand]
    public void OpenSide()
    {
        if (TopSide)
        {
            TopSide1 = true;
            TopSide = false;
        }
        else
        {
            TopSide1 = false;
            TopSide = true;
        }
    }

    [RelayCommand]
    public async Task NewInfo()
    {
        if (_isGetNewInfo)
        {
            return;
        }
        _isGetNewInfo = true;

        var data = await WebBinding.GetNewLog();
        if (data == null)
        {
            Model.Show(App.Lang("MainWindow.Error1"));
        }
        else
        {
            Model.ShowText(App.Lang("MainWindow.Info40"), data);
        }

        _isGetNewInfo = false;
    }

    [RelayCommand]
    public async Task Upgrade()
    {
        var res = await Model.ShowTextWait(App.Lang("Text.Update"), _updateStr);
        if (res)
        {
            if (_isNewUpdate)
            {
                WebBinding.OpenWeb(WebType.ColorMCDownload);
            }
            else
            {
                UpdateChecker.StartUpdate();
            }
        }
    }

    [RelayCommand]
    public void ShowCount()
    {
        WindowManager.ShowCount();
    }

    [RelayCommand]
    public void MusicPause()
    {
        if (_isplay)
        {
            BaseBinding.MusicPause();

            Model.Title = App.Lang("Name");
        }
        else
        {
            BaseBinding.MusicPlay();

            Model.Title = App.Lang("Name") + " " + App.Lang("MainWindow.Info33");
        }

        _isplay = !_isplay;
    }

    [RelayCommand]
    public void ShowSkin()
    {
        WindowManager.ShowSkin();
    }

    [RelayCommand]
    public void ShowUser()
    {
        WindowManager.ShowUser();
    }

    [RelayCommand]
    public void ShowSetting()
    {
        WindowManager.ShowSetting(SettingType.Normal);
    }

    [RelayCommand]
    public async Task AddGroup()
    {
        var (Cancel, Text) = await Model.ShowInputOne(App.Lang("MainWindow.Info1"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            Model.Show(App.Lang("MainWindow.Error3"));
            return;
        }

        if (!GameBinding.AddGameGroup(Text))
        {
            Model.Show(App.Lang("MainWindow.Error4"));
            return;
        }

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    [RelayCommand]
    public void Confirm()
    {
        _isCancel = false;
        _semaphore.Release();
    }

    [RelayCommand]
    public void Cancel()
    {
        _isCancel = true;
        _semaphore.Release();
    }

    [RelayCommand]
    public async Task OpenGuide()
    {
        var res = await Model.ShowWait(App.Lang("SettingWindow.Tab7.Info3"));
        WebBinding.OpenWeb(res ? WebType.Guide1 : WebType.Guide);
    }

    [RelayCommand]
    public void OpenNetFrp()
    {
        if (UserBinding.HaveOnline())
        {
            WindowManager.ShowNetFrp();
        }
        else
        {
            Model.Show(App.Lang("MainWindow.Error6"));
        }
    }

    private void App_SkinLoad()
    {
        Head = ImageManager.HeadBitmap!;

        IsHeadLoad = false;
    }

    public void Search()
    {
        GameSearch = true;
        GameSearchText = "";
    }

    public void SearchClose()
    {
        GameSearch = false;
        GameSearchText = "";
    }

    public Task<(bool, string?)> Set(GameItemModel obj)
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

        DialogHost.Show(this, "MainCon");

        GroupItem = obj.Obj.GroupName;

        return Task.Run(() =>
        {
            _semaphore.WaitOne();
            return (_isCancel, GroupItem);
        });
    }

    public async void EditGroup(GameItemModel obj)
    {
        await Set(obj);
        DialogHost.Close("MainCon");

        if (_isCancel)
        {
            return;
        }

        GameBinding.MoveGameGroup(obj.Obj, GroupItem);
    }

    public void LoadMotd()
    {
        var config = GuiConfigUtils.Config.ServerCustom;
        if (config != null && config?.Motd == true &&
            !string.IsNullOrWhiteSpace(config?.IP))
        {
            MotdDisplay = true;

            Server = (config.IP, config.Port);
        }
        else
        {
            MotdDisplay = false;
        }
    }

    public void Select(GameItemModel? obj)
    {
        if (Game != null)
        {
            Game.IsSelect = false;
        }
        Game = obj;
        if (Game != null)
        {
            Game.IsSelect = true;
        }
    }

    public async void LoadUser()
    {
        IsHeadLoad = true;

        var user = UserBinding.GetLastUser();

        if (user == null)
        {
            UserId = App.Lang("MainWindow.Info36");
            UserType = App.Lang("MainWindow.Info35");
        }
        else
        {
            UserId = user.UserName;

            if (GuiConfigUtils.Config.ServerCustom.LockLogin && user.AuthType != AuthType.OAuth)
            {
                bool find = false;
                foreach (var item in GuiConfigUtils.Config.ServerCustom.LockLogins)
                {
                    if (item.Type == user.AuthType && item.Data == user.Text1)
                    {
                        UserType = item.Name;
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    UserType = App.Lang("MainWindow.Error7");
                }
            }
            else
            {
                UserType = user.AuthType.GetName();
            }
        }

        await UserBinding.LoadSkin();
    }

    public void IsDelete()
    {
        Game = null;
        LoadGameItem();
    }

    public async void LoadDone()
    {
        LoadGameItem();
        LoadUser();

        LoadMotd();

        BaseBinding.LoadMusic();

        var config = GuiConfigUtils.Config;
        var config1 = ConfigUtils.Config;
        if (config.Live2D?.LowFps == true)
        {
            LowFps = true;
        }
        if (config1.Http?.CheckUpdate == true)
        {
            var data = await UpdateChecker.Check();
            if (!data.Item1)
            {
                return;
            }
            HaveUpdate = true;
            _isNewUpdate = data.Item2 || ColorMCGui.IsAot;
            _updateStr = data.Item3!;
        }
    }

    public void LoadGameItem()
    {
        IsNotGame = GameBinding.IsNotGame;

        var config = GuiConfigUtils.Config.ServerCustom;
        if (config?.PlayMusic == true)
        {
            Model.Title = App.Lang("Name") + " " + App.Lang("MainWindow.Info33");
            MusicDisplay = true;
        }
        else
        {
            MusicDisplay = false;
        }

        if (config?.LockGame == true)
        {
            GameGroups.Clear();
            GroupList.Clear();
            IsFirst = true;
            var game = GameBinding.GetGame(config?.GameName);
            if (game == null)
            {
                IsGameError = true;
                IsOneGame = false;
            }
            else
            {
                IsGameError = false;
                OneGame = new(Model, this, game)
                {
                    OneGame = true
                };
                IsOneGame = true;
            }
        }
        else
        {
            IsGameError = false;
            IsOneGame = false;
            var list = GameBinding.GetGameGroups();
            var uuid = ConfigBinding.GetLastLaunch();
            GameItemModel? last = null;
            if (IsFirst)
            {
                IsFirst = false;
                GamesModel? DefaultGroup = null;

                foreach (var item in list)
                {
                    if (item.Key == " ")
                    {
                        DefaultGroup = new(Model, this, " ", App.Lang("MainWindow.Info20"), item.Value);
                        if (list.Count > 0)
                        {
                            DefaultGroup.Expander = false;
                        }
                        last ??= DefaultGroup.Find(uuid);
                    }
                    else
                    {
                        var group = new GamesModel(Model, this, item.Key, item.Key, item.Value);
                        GameGroups.Add(group);
                        if (list.Count > 0)
                        {
                            group.Expander = false;
                        }
                        last ??= group.Find(uuid);
                    }
                }

                if (DefaultGroup != null)
                {
                    GameGroups.Add(DefaultGroup);
                }
                Select(last);
            }
            else
            {
                var list1 = new List<GamesModel>(GameGroups);
                foreach (var item in list1)
                {
                    if (list.TryGetValue(item.Key, out var value))
                    {
                        item.SetItems(value);
                        list.Remove(item.Key);
                    }
                    else
                    {
                        GameGroups.Remove(item);
                    }
                }
                foreach (var item in list)
                {
                    var group = new GamesModel(Model, this, item.Key, item.Key, item.Value);
                    GameGroups.Add(group);
                    if (list.Count > 0)
                    {
                        group.Expander = false;
                    }
                    last ??= group.Find(uuid);
                }

                foreach (var item in list1)
                {
                    last ??= item.Find(uuid);
                }

                Select(last);
            }
        }

        OnPropertyChanged(SwitchView);
    }

    public void GameClose(string uuid)
    {
        if (Launchs.Remove(uuid, out var con))
        {
            con.IsLaunch = false;
        }
    }

    public async void Launch(GameItemModel obj)
    {
        Select(obj);

        IsLaunch = true;
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            Model.Progress(App.Lang("MainWindow.Info3"));
        }
        var item = Game!;
        var game = item.Obj;
        item.IsLaunch = false;
        item.IsLoad = true;
        Model.Notify(App.Lang(string.Format(App.Lang("MainWindow.Info28"), game.Name)));
        var res = await GameBinding.Launch(Model, game, hide: GuiConfigUtils.Config.CloseBeforeLaunch);
        Model.Title1 = null;
        item.IsLoad = false;
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            Model.ProgressClose();
        }
        if (res.Item1 == false)
        {
            Model.Show(res.Item2!);
        }
        else
        {
            Model.Notify(App.Lang("MainWindow.Info2"));

            item.IsLaunch = true;
            Launchs.Add(game.UUID, item);

            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                Model.Progress(App.Lang("MainWindow.Info26"));
            }
        }
        IsLaunch = false;
    }

    public void ChangeModel()
    {
        OnPropertyChanged("ModelChange");
    }

    public void ChangeModelDone()
    {
        OnPropertyChanged("ModelChangeDone");
    }

    public void DeleteModel()
    {
        OnPropertyChanged("ModelDelete");
    }

    public void ShowMessage(string message)
    {
        Message = message;
        OnPropertyChanged("ModelText");
    }

    public override void Close()
    {
        GroupList.Clear();
        foreach (var item in GameGroups)
        {
            item.Close();
        }
        GameGroups.Clear();
        Launchs.Clear();
    }
}
