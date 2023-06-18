using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel : ObservableObject, IMainTop
{
    public readonly IUserControl Con;
    private readonly Semaphore semaphore = new(0, 2);
    public ObservableCollection<string> GroupList { get; init; } = new();
    public ObservableCollection<GamesModel> GameGroups { get; init; } = new();

    private readonly Dictionary<string, GameItemModel> Launchs = new();

    public bool launch = false;
    public bool first = true;
    public string Message;

    [ObservableProperty]
    private int live2dWidth = 300;
    [ObservableProperty]
    private int live2dHeight = 300;

    [ObservableProperty]
    private (string, ushort) server;

    [ObservableProperty]
    private string? groupItem;
    [ObservableProperty]
    private string sideButton = "→";
    [ObservableProperty]
    private string userName;
    [ObservableProperty]
    private string authType;

    [ObservableProperty]
    private bool groupEnable;
    [ObservableProperty]
    private bool isNotGame;
    [ObservableProperty]
    private bool motdDisplay;
    [ObservableProperty]
    private bool isGameError;
    [ObservableProperty]
    private bool isOneGame;
    [ObservableProperty]
    private bool sideDisplay = true;
    [ObservableProperty]
    private bool enableButton1;
    [ObservableProperty]
    private bool enableButton2;
    [ObservableProperty]
    private bool isHeadLoad;
    [ObservableProperty]
    private bool musicDisplay;

    [ObservableProperty]
    private GameItemModel? game;
    [ObservableProperty]
    private GameItemModel? oneGame;

    [ObservableProperty]
    private Bitmap head = App.LoadIcon;

    private LoginObj? Obj1;

    private bool isplay = true;

    private bool isCancel;

    public MainModel(IUserControl con)
    {
        Con = con;

        ColorMCCore.GameDownload = GameDownload;
        ColorMCCore.OfflineLaunch = OfflineLaunch;
        ColorMCCore.LaunchP = LaunchP;

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
        if (SideDisplay)
        {
            SideDisplay = false;
            SideButton = "←";
        }
        else
        {
            SideDisplay = true;
            SideButton = "→";
        }
    }

    [RelayCommand]
    public void MusicPause()
    {
        var window = Con.Window;
        if (isplay)
        {
            BaseBinding.MusicPause();

            window.SetTitle(App.GetLanguage("MainWindow.Title"));
        }
        else
        {
            BaseBinding.MusicPlay();

            window.SetTitle(App.GetLanguage("MainWindow.Title") + " " + App.GetLanguage("MainWindow.Info33"));
        }

        isplay = !isplay;
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
    public async void AddGroup()
    {
        var window = Con.Window;
        await window.InputInfo.ShowOne(App.GetLanguage("MainWindow.Info1"), false);
        if (window.InputInfo.Cancel)
        {
            return;
        }

        var res = window.InputInfo.Read().Item1;
        if (string.IsNullOrWhiteSpace(res))
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Error3"));
            return;
        }

        if (!GameBinding.AddGameGroup(res))
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Error4"));
            return;
        }

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    [RelayCommand]
    public void Confirm()
    {
        isCancel = false;
        semaphore.Release();
    }

    [RelayCommand]
    public void Cancel()
    {
        isCancel = true;
        semaphore.Release();
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

    private Task<bool> LaunchP(bool pre)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
            Con.Window.OkInfo.ShowWait(pre ? App.GetLanguage("MainWindow.Info29")
            : App.GetLanguage("MainWindow.Info30")));
    }


    public Task<(bool, string?)> Set(GameItemModel obj)
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

        GroupEnable = true;

        GroupItem = obj.Obj.GroupName;

        return Task.Run(() =>
        {
            semaphore.WaitOne();
            return (isCancel, GroupItem);
        });
    }

    public async void EditGroup(GameItemModel obj)
    {
        await Set(obj);
        GroupEnable = false;
        if (isCancel)
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

    private Task<bool> GameDownload(LaunchState state, GameSettingObj obj)
    {
        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var window = Con.Window;

            return state switch
            {
                LaunchState.LostLib => await window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info5")),
                LaunchState.LostLoader => await window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info6")),
                LaunchState.LostLoginCore => await window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info7")),
                _ => await window.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info4")),
            };
        });
    }

    private void GameLunch(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var window = Con.Window;
            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                switch (state)
                {
                    case LaunchState.Login:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info8"));
                        break;
                    case LaunchState.Check:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info9"));
                        break;
                    case LaunchState.CheckVersion:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info10"));
                        break;
                    case LaunchState.CheckLib:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info11"));
                        break;
                    case LaunchState.CheckAssets:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info12"));
                        break;
                    case LaunchState.CheckLoader:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info13"));
                        break;
                    case LaunchState.CheckLoginCore:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info14"));
                        break;
                    case LaunchState.CheckMods:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info17"));
                        break;
                    case LaunchState.Download:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info15"));
                        break;
                    case LaunchState.JvmPrepare:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info16"));
                        break;
                    case LaunchState.LaunchPre:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info31"));
                        break;
                    case LaunchState.LaunchPost:
                        window.ProgressInfo.NextText(App.GetLanguage("MainWindow.Info32"));
                        break;
                    case LaunchState.End:
                        window.ProgressInfo.Close();
                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case LaunchState.Login:
                        window.Head.Title1 = App.GetLanguage("MainWindow.Info8");
                        break;
                    case LaunchState.Check:
                        window.Head.Title1 = App.GetLanguage("MainWindow.Info9");
                        break;
                    case LaunchState.CheckVersion:
                        window.Head.Title1 = App.GetLanguage("MainWindow.Info10");
                        break;
                    case LaunchState.CheckLib:
                        window.Head.Title1 = App.GetLanguage("MainWindow.Info11");
                        break;
                    case LaunchState.CheckAssets:
                        window.Head.Title1 = App.GetLanguage("MainWindow.Info12");
                        break;
                    case LaunchState.CheckLoader:
                        window.Head.Title1 = App.GetLanguage("MainWindow.Info13");
                        break;
                    case LaunchState.CheckLoginCore:
                        window.Head.Title1 = App.GetLanguage("MainWindow.Info14");
                        break;
                    case LaunchState.CheckMods:
                        window.Head.Title1 = App.GetLanguage("MainWindow.Info17");
                        break;
                    case LaunchState.Download:
                        window.Head.Title1 = App.GetLanguage("MainWindow.Info15");
                        break;
                    case LaunchState.JvmPrepare:
                        window.Head.Title1 = App.GetLanguage("MainWindow.Info16");
                        break;
                    case LaunchState.End:
                        window.Head.Title1 = "";
                        break;
                }
            }
        });
    }

    public async void Load1()
    {
        Obj1 = UserBinding.GetLastUser();

        if (Obj1 == null)
        {
            UserName = App.GetLanguage("MainWindow.Info36");
            AuthType = App.GetLanguage("MainWindow.Info35");
        }
        else
        {
            UserName = Obj1.UserName;
            AuthType = Obj1.AuthType.GetName();
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
            var window = Con.Window;
            window.OkInfo.Show(App.GetLanguage("MainWindow.Info27"));
        }

#if !DEBUG
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
            first = true;
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

        if (config.Item2?.ServerCustom?.PlayMusic == true)
        {
            var window = Con.Window;
            window.SetTitle(App.GetLanguage("MainWindow.Title") + " " + App.GetLanguage("MainWindow.Info33"));
            MusicDisplay = true;
        }
        else
        {
            MusicDisplay = false;
        }

        if (config.Item2 != null && config.Item2.ServerCustom?.LockGame == true)
        {
            GameGroups.Clear();
            GroupList.Clear();
            first = true;
            var game = GameBinding.GetGame(config.Item2.ServerCustom?.GameName);
            if (game == null)
            {
                IsGameError = true;
                IsOneGame = false;
            }
            else
            {
                IsGameError = false;
                OneGame = new(Con, this, game);
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
            if (first)
            {
                first = false;
                GamesModel? DefaultGroup = null;

                foreach (var item in list)
                {
                    if (item.Key == " ")
                    {
                        DefaultGroup = new(Con, this, " ", App.GetLanguage("MainWindow.Info20"), item.Value);
                        if (list.Count > 0)
                        {
                            DefaultGroup.Expander = false;
                        }
                        last ??= DefaultGroup.Find(uuid);
                    }
                    else
                    {
                        var group = new GamesModel(Con, this, item.Key, item.Key, item.Value);
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
                    var group = new GamesModel(Con, this, item.Key, item.Key, item.Value);
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

    private Task<bool> OfflineLaunch(LoginObj login)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            var window = Con.Window;
            return window.OkInfo.ShowWait(string.Format(
                App.GetLanguage("MainWindow.Info21"), login.UserName));
        });
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
        if (launch)
            return;

        ColorMCCore.GameLaunch = GameLunch;

        var window = Con.Window;
        launch = true;
        UpdateLaunch();
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            window.ProgressInfo.Show(App.GetLanguage("MainWindow.Info3"));
        }
        var item = Game!;
        var game = item.Obj;
        item.IsLaunch = false;
        item.IsLoad = true;
        window.NotifyInfo.Show(App.GetLanguage(string.Format(App.GetLanguage("MainWindow.Info28"), game.Name)));
        var res = await GameBinding.Launch(game);
        window.Head.Title1 = null;
        item.IsLoad = false;
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            await window.ProgressInfo.CloseAsync();
        }
        if (res.Item1 == false)
        {
            window.OkInfo.Show(res.Item2!);
        }
        else
        {
            window.NotifyInfo.Show(App.GetLanguage("MainWindow.Info2"));
            Launchs.Add(game.UUID, item);
            item.IsLaunch = true;

            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                window.ProgressInfo.Show(App.GetLanguage("MainWindow.Info26"));
            }
        }
        launch = false;
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
            if (BaseBinding.IsGameRun(Game.Obj) || launch)
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
}
