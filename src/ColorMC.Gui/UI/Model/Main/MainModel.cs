using Avalonia.Layout;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel : TopModel, IMainTop
{
    public bool IsLaunch = false;
    public bool IsFirst = true;

    public ObservableCollection<string> GroupList { get; init; } = [];
    public ObservableCollection<GamesModel> GameGroups { get; init; } = [];

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
    private bool _groupEnable;
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
    private Bitmap _head = App.LoadIcon;

    [ObservableProperty]
    private bool _render = true;

    [ObservableProperty]
    private bool _haveUpdate;

    private bool _isNewUpdate;
    private string _updateStr;

    private bool _isGetNewInfo;

    public MainModel(BaseModel model) : base(model)
    {
        App.SkinLoad += App_SkinLoad;

        App.UserEdit += Load1;
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
            Model.Show(App.Lang("Gui.Error38"));
        }
        else
        {
            Model.ShowText(App.Lang("Gui.Info35"), data);
        }
    }

    [RelayCommand]
    public async Task Upgrade()
    {
        var res = await Model.ShowTextWait(App.Lang("Gui.Info5"), _updateStr);
        if (res)
        {
            if (_isNewUpdate)
            {
                BaseBinding.OpUrl("https://colormc.coloryr.com/");
            }
            else
            {
                UpdateChecker.StartUpdate();
            }
        }
    }

    [RelayCommand]
    public void AddGame()
    {
        App.ShowAddGame(null);
    }

    [RelayCommand]
    public void ShowCount()
    {
        App.ShowCount();
    }

    [RelayCommand]
    public void MusicPause()
    {
        if (_isplay)
        {
            BaseBinding.MusicPause();

            Model.Title = App.Lang("MainWindow.Title");
        }
        else
        {
            BaseBinding.MusicPlay();

            Model.Title = App.Lang("MainWindow.Title") + " " + App.Lang("MainWindow.Info33");
        }

        _isplay = !_isplay;
    }

    [RelayCommand]
    public void ShowSkin()
    {
        App.ShowSkin();
    }

    [RelayCommand]
    public void ShowUser()
    {
        App.ShowUser();
    }

    [RelayCommand]
    public void ShowSetting()
    {
        App.ShowSetting(SettingType.Normal);
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
    public void AddUser()
    {
        App.ShowUser(true);
    }

    [RelayCommand]
    public void SetJava()
    {
        App.ShowSetting(SettingType.SetJava);
    }

    [RelayCommand]
    public void OpenWeb1()
    {
        WebBinding.OpenWeb(WebType.Web);
    }

    [RelayCommand]
    public void OpenWeb2()
    {
        WebBinding.OpenWeb(WebType.Minecraft);
    }

    [RelayCommand]
    public void OpenGuide()
    {
        WebBinding.OpenWeb(WebType.Guide);
    }

    [RelayCommand]
    public void OpenNetFrp()
    {
        if (UserBinding.HaveOnline())
        {
            App.ShowNetFrp();
        }
        else
        {
            Model.Show(App.Lang("MainWindow.Info39"));
        }
    }

    private void App_SkinLoad()
    {
        Head = UserBinding.HeadBitmap!;

        IsHeadLoad = false;
    }

    public Task<(bool, string?)> Set(GameItemModel obj)
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

        GroupEnable = true;

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
        GroupEnable = false;
        if (_isCancel)
        {
            return;
        }

        GameBinding.MoveGameGroup(obj.Obj, GroupItem);
    }

    public void MotdLoad()
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 != null && config.Item2.ServerCustom?.Motd == true &&
            !string.IsNullOrWhiteSpace(config.Item2.ServerCustom.IP))
        {
            MotdDisplay = true;

            Server = (config.Item2.ServerCustom.IP, config.Item2.ServerCustom.Port);
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

    public async void Load1()
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
            UserType = user.AuthType.GetName();
        }

        await UserBinding.LoadSkin();
    }

    public void IsDelete()
    {
        Game = null;
        Load();
    }

    public async void LoadDone()
    {
        Load();
        Load1();

        MotdLoad();

        BaseBinding.LoadMusic();

        var config = ConfigBinding.GetAllConfig();
        if (config.Item1?.Http?.CheckUpdate == true)
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

    public void Load()
    {
        IsNotGame = GameBinding.IsNotGame;

        var config = ConfigBinding.GetAllConfig();

        if (config.Item2.ServerCustom?.PlayMusic == true)
        {
            Model.Title = App.Lang("MainWindow.Title") + " " + App.Lang("MainWindow.Info33");
            MusicDisplay = true;
        }
        else
        {
            MusicDisplay = false;
        }

        if (config.Item2.ServerCustom?.LockGame == true)
        {
            GameGroups.Clear();
            GroupList.Clear();
            IsFirst = true;
            var game = GameBinding.GetGame(config.Item2.ServerCustom?.GameName);
            if (game == null)
            {
                IsGameError = true;
                IsOneGame = false;
            }
            else
            {
                IsGameError = false;
                OneGame = new(Model, this, game);
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
        if (IsLaunch || obj.IsLaunch)
            return;

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
        var res = await GameBinding.Launch(Model, game, wait: GuiConfigUtils.Config.CloseBeforeLaunch);
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
            if (SystemInfo.Os != OsType.Android)
            {
                item.IsLaunch = true;
                Launchs.Add(game.UUID, item);
            }

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

    protected override void Close()
    {
        GroupList.Clear();
        foreach (var item in GameGroups)
        {
            item.TopClose();
        }
        GameGroups.Clear();
        Launchs.Clear();
    }
}
