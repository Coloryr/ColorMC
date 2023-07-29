using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel : BaseModel, IMainTop
{
    public bool IsLaunch = false;
    public bool IsFirst = true;

    public ObservableCollection<string> GroupList { get; init; } = new();
    public ObservableCollection<GamesModel> GameGroups { get; init; } = new();

    private readonly Semaphore _semaphore = new(0, 2);
    private readonly Dictionary<string, GameItemModel> Launchs = new();

    private LoginObj? _user;
    private bool _isplay = true;
    private bool _isCancel;

    [ObservableProperty]
    private int _live2dWidth = 300;
    [ObservableProperty]
    private int _live2dHeight = 300;

    [ObservableProperty]
    private (string, ushort) _server;

    [ObservableProperty]
    private string? _groupItem;
    [ObservableProperty]
    private string _sideButton = "→";
    [ObservableProperty]
    private string _userName;
    [ObservableProperty]
    private string _authType;
    [ObservableProperty]
    private string _message;

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
    private bool _sideDisplay = true;
    [ObservableProperty]
    private bool _enableButton1;
    [ObservableProperty]
    private bool _enableButton2;
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
    private Dock _mirror1 = Dock.Left;
    [ObservableProperty]
    private HorizontalAlignment _mirror2 = HorizontalAlignment.Left;
    [ObservableProperty]
    private HorizontalAlignment _mirror3 = HorizontalAlignment.Right;

    public MainModel(IUserControl con) : base(con)
    {
        App.SkinLoad += App_SkinLoad;

        App.UserEdit += Load1;
    }

    partial void OnGameChanged(GameItemModel? value)
    {
        UpdateLaunch();
    }

    [RelayCommand]
    public void SideChange()
    {
        var config = ConfigBinding.GetAllConfig();

        if (SideDisplay)
        {
            SideDisplay = false;
        }
        else
        {
            SideDisplay = true;
        }

        SideButtonChange(SideDisplay);

        if (config.Item2.Gui.WindowStateSave)
        {
            ConfigBinding.SetMainHide(SideDisplay);
        }
    }

    [RelayCommand]
    public void MusicPause()
    {
        if (_isplay)
        {
            BaseBinding.MusicPause();

            Window.SetTitle(App.GetLanguage("MainWindow.Title"));
        }
        else
        {
            BaseBinding.MusicPlay();

            Window.SetTitle(App.GetLanguage("MainWindow.Title") + " " + App.GetLanguage("MainWindow.Info33"));
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
    public void AddGame()
    {
        App.ShowAddGame();
    }

    [RelayCommand]
    public void EditGame()
    {
        if (Game != null)
        {
            App.ShowGameEdit(Game.Obj);
        }
    }

    [RelayCommand]
    public void ShowSetting()
    {
        App.ShowSetting(SettingType.Normal);
    }

    [RelayCommand]
    public async Task AddGroup()
    {
        var (Cancel, Text) = await ShowOne(App.GetLanguage("MainWindow.Info1"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            Show(App.GetLanguage("MainWindow.Error3"));
            return;
        }

        if (!GameBinding.AddGameGroup(Text))
        {
            Show(App.GetLanguage("MainWindow.Error4"));
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
    public void Launch()
    {
        if (Game != null)
        {
            Launch(Game);
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
        _user = UserBinding.GetLastUser();

        if (_user == null)
        {
            UserName = App.GetLanguage("MainWindow.Info36");
            AuthType = App.GetLanguage("MainWindow.Info35");
        }
        else
        {
            UserName = _user.UserName;
            AuthType = _user.AuthType.GetName();
        }

        IsHeadLoad = true;

        await UserBinding.LoadSkin();
    }

    public void IsDelete()
    {
        Game = null;
        Load();
    }

    public void Open()
    {
        Load();
        Load1();

        if (BaseBinding.CheckOldDir())
        {
            Show(App.GetLanguage("MainWindow.Info27"));
        }

#if DEBUG
        if (ConfigBinding.GetAllConfig().Item1?.Http?.CheckUpdate == true)
        {
            UpdateChecker.Check();
        }
#endif

        MotdLoad();

        BaseBinding.LoadMusic();

        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 != null && config.Item2.ServerCustom?.LockGame == true)
        {
            IsFirst = true;
            var game = GameBinding.GetGame(config.Item2.ServerCustom?.GameName);
            if (game != null)
            {
                BaseBinding.ServerPackCheck(game);
            }
        }
    }

    public void Load()
    {
        IsNotGame = GameBinding.IsNotGame;

        var config = ConfigBinding.GetAllConfig();

        if (config.Item2.ServerCustom?.PlayMusic == true)
        {
            Window.SetTitle(App.GetLanguage("MainWindow.Title") + " " + App.GetLanguage("MainWindow.Info33"));
            MusicDisplay = true;
        }
        else
        {
            MusicDisplay = false;
        }

        if (config.Item2.Gui.WindowStateSave)
        {
            SideDisplay = config.Item2.Gui.MainDisplay;
        }

        Mirror();

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
                OneGame = new(Control, this, game);
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
                        DefaultGroup = new(Control, this, " ", App.GetLanguage("MainWindow.Info20"), item.Value);
                        if (list.Count > 0)
                        {
                            DefaultGroup.Expander = false;
                        }
                        last ??= DefaultGroup.Find(uuid);
                    }
                    else
                    {
                        var group = new GamesModel(Control, this, item.Key, item.Key, item.Value);
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
                    var group = new GamesModel(Control, this, item.Key, item.Key, item.Value);
                    GameGroups.Add(group);
                    if (list.Count > 0)
                    {
                        group.Expander = false;
                    }
                    last ??= group.Find(uuid);
                }

                Select(last);
            }
        }
    }

    public void GameClose(string uuid)
    {
        if (Launchs.Remove(uuid, out var con))
        {
            if (Game?.Obj?.UUID == uuid)
            {
                UpdateLaunch();
            }
            con.IsLaunch = false;
        }
    }

    public async void Launch(GameItemModel obj)
    {
        if (IsLaunch || obj.IsLaunch)
            return;

        IsLaunch = true;
        UpdateLaunch();
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            Progress(App.GetLanguage("MainWindow.Info3"));
        }
        var item = Game!;
        var game = item.Obj;
        item.IsLaunch = false;
        item.IsLoad = true;
        Notify(App.GetLanguage(string.Format(App.GetLanguage("MainWindow.Info28"), game.Name)));
        var res = await GameBinding.Launch(Window, game);
        Window.Head.Title1 = null;
        item.IsLoad = false;
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            await ProgressCloseAsync();
        }
        if (res.Item1 == false)
        {
            Show(res.Item2!);
        }
        else
        {
            Notify(App.GetLanguage("MainWindow.Info2"));
            Launchs.Add(game.UUID, item);
            item.IsLaunch = true;

            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                Progress(App.GetLanguage("MainWindow.Info26"));
            }
        }
        IsLaunch = false;
        UpdateLaunch();
    }

    private void UpdateLaunch()
    {
        if (Game == null)
        {
            EnableButton1 = false;
            EnableButton2 = false;
        }
        else
        {
            if (BaseBinding.IsGameRun(Game.Obj) || IsLaunch)
            {
                EnableButton1 = false;
            }
            else
            {
                EnableButton1 = true;
            }
            EnableButton2 = true;
        }
    }

    public void ChangeModel()
    {
        OnPropertyChanged("ModelChange");
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

    public void Mirror()
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2.Gui.WindowMirror)
        {
            Mirror1 = Dock.Right;
            Mirror2 = HorizontalAlignment.Right;
            Mirror3 = HorizontalAlignment.Left;
        }
        else
        {
            Mirror1 = Dock.Left;
            Mirror2 = HorizontalAlignment.Left;
            Mirror3 = HorizontalAlignment.Right;
        }
        SideButtonChange(SideDisplay);
    }

    private void SideButtonChange(bool open)
    {
        var config = ConfigBinding.GetAllConfig();
        if (open)
        {
            SideButton = config.Item2.Gui.WindowMirror ? "→" : "←";
        }
        else
        {
            SideButton = config.Item2.Gui.WindowMirror ? "←" : "→";
        }
    }
}
