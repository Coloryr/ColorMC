using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class MainControl : UserControl, IUserControl
{
    private readonly List<GamesControl> Groups = new();
    private readonly Dictionary<string, GameControl> Launchs = new();
    private readonly GamesControl DefaultGroup = new();

    public GameControl? Obj { get; private set; }

    private bool launch = false;
    private bool first = true;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public MainControl()
    {
        InitializeComponent();

        ColorMCCore.GameLaunch = GameLunch;
        ColorMCCore.GameDownload = GameDownload;
        ColorMCCore.OfflineLaunch = OfflineLaunch;
        ColorMCCore.LaunchP = LaunchP;

        Grid3.PointerPressed += Grid3_PointerPressed;
        ScrollViewer1.PointerPressed += ScrollViewer1_PointerPressed;

        App.UserEdit += OnUserEdit;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);

        KeyDown += MainControl_KeyDown;
    }

    private void MainControl_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {

        }
    }

    private Task<bool> LaunchP(bool pre)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
            Window.OkInfo.ShowWait(pre ? App.GetLanguage("MainWindow.Info29")
            : App.GetLanguage("MainWindow.Info30")));
    }

    private void ScrollViewer1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(ScrollViewer1).Properties.IsLeftButtonPressed)
        {
            GameItemSelect(null);
        }
    }

    private void Grid3_PointerPressed(object? sender, PointerEventArgs e)
    {
        App.ShowAddGame();
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(BaseBinding.DrapType))
        {
            return;
        }
        if (e.Data.Contains(DataFormats.Text))
        {
            Grid2.IsVisible = true;
            Label1.Content = App.GetLanguage("Gui.Info6");
        }
        else if (e.Data.Contains(DataFormats.Files))
        {
            Grid2.IsVisible = true;
            Label1.Content = App.GetLanguage("Gui.Info7");
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(BaseBinding.DrapType))
        {
            return;
        }
        Grid2.IsVisible = false;
        if (e.Data.Contains(DataFormats.Text))
        {
            var str = e.Data.GetText();
            if (str?.StartsWith("authlib-injector:yggdrasil-server:") == true)
            {
                App.ShowUser(str);
            }
        }
        else if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files == null || files.Count() > 1)
                return;

            var item = files.First().GetPath();
            if (item == null)
                return;
            if (item.EndsWith(".zip") || item.EndsWith(".mrpack"))
            {
                App.ShowAddGame(item);
            }
        }
    }

    private Task<bool> OfflineLaunch(LoginObj login)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            var window = App.FindRoot(VisualRoot);
            return window.OkInfo.ShowWait(string.Format(
                App.GetLanguage("MainWindow.Info21"), login.UserName));
        });
    }

    public async void Launch(bool debug)
    {
        if (launch)
            return;

        var window = App.FindRoot(VisualRoot);
        launch = true;
        ItemInfo.SetLaunch(true);
        ItemInfo.UpdateLaunch();
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            window.ProgressInfo.Show(App.GetLanguage("MainWindow.Info3"));
        }
        var item = Obj!;
        var game = item.Obj;
        item.SetLaunch(false);
        item.SetLoad(true);
        window.NotifyInfo.Show(App.GetLanguage(string.Format(App.GetLanguage("MainWindow.Info28"), game.Name)));
        var res = await GameBinding.Launch(game, debug);
        window.Head.Title1 = null;
        item.SetLoad(false);
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            await window.ProgressInfo.CloseAsync();
        }
        if (res.Item1 == false)
        {
            item.SetLaunch(false);
            window.OkInfo.Show(res.Item2!);
        }
        else
        {
            window.NotifyInfo.Show(App.GetLanguage("MainWindow.Info2"));
            Launchs.Add(game.UUID, item);
            item.SetLaunch(true);

            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                window.ProgressInfo.Show(App.GetLanguage("MainWindow.Info26"));
            }
        }
        launch = false;
        ItemInfo.SetLaunch(false);
        ItemInfo.UpdateLaunch();
    }

    public void GameClose(string uuid)
    {
        if (Launchs.Remove(uuid, out var con))
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (Obj?.Obj?.UUID == uuid)
                {
                    ItemInfo.SetGame(Obj?.Obj);
                }
                con.SetLaunch(false);
            });
        }
    }

    private void OnUserEdit()
    {
        Dispatcher.UIThread.Post(Load1);
    }

    public void Closed()
    {
        App.UserEdit -= OnUserEdit;

        ColorMCCore.GameLaunch = null;
        ColorMCCore.GameDownload = null;
        ColorMCCore.OfflineLaunch = null;

        App.MainWindow = null;

        App.Close();
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("MainWindow.Title"));

        Load();
        Load1();

        if (BaseBinding.CheckOldDir())
        {
            var window = App.FindRoot(VisualRoot);
            window.OkInfo.Show(App.GetLanguage("MainWindow.Info27"));
        }

#if !DEBUG
        if (ConfigBinding.GetAllConfig().Item1?.Http?.CheckUpdate == true)
        {
            UpdateChecker.Check();
        }
#endif
        //UpdateChecker.Check();

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

    public void MotdLoad()
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 != null && config.Item2.ServerCustom?.Motd == true &&
            !string.IsNullOrWhiteSpace(config.Item2.ServerCustom.IP))
        {
            ServerMotdControl1.IsVisible = true;
            ServerMotdControl1.Load(config.Item2.ServerCustom.IP, config.Item2.ServerCustom.Port);
        }
        else
        {
            ServerMotdControl1.IsVisible = false;
        }
    }

    public void GameItemSelect(GameControl? obj)
    {
        Obj?.SetSelect(false);
        Obj = obj;
        if (obj != null)
        {
            obj.SetSelect(true);
            ItemInfo.SetGame(obj.Obj);
        }
        else
        {
            ItemInfo.SetGame(null);
        }
    }

    private Task<bool> GameDownload(LaunchState state, GameSettingObj obj)
    {
        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var window = App.FindRoot(VisualRoot);

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
            var window = App.FindRoot(VisualRoot);
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
                }
            }
        });
    }

    private void Load1()
    {
        ItemInfo.SetUser(UserBinding.GetLastUser());
    }

    public void IsDelete()
    {
        Obj = null;
        ItemInfo.SetGame(null);
    }

    public void Load()
    {
        Dispatcher.UIThread.Post(ItemInfo.Load);

        var nogame = GameBinding.IsNotGame;

        Grid3.IsVisible = nogame;

        var config = ConfigBinding.GetAllConfig();

        if (config.Item2 != null && config.Item2.ServerCustom?.LockGame == true)
        {
            first = true;
            var game = GameBinding.GetGame(config.Item2.ServerCustom?.GameName);
            if (game == null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    GameGroups.Children.Clear();

                    var item = new Grid
                    {
                        Background = Brush.Parse("#EEEEEE")
                    };
                    var item1 = new Label
                    {
                        Content = App.GetLanguage("MainWindow.Info18")
                    };

                    item.Children.Add(item1);

                    GameGroups.VerticalAlignment = VerticalAlignment.Center;
                    GameGroups.HorizontalAlignment = HorizontalAlignment.Center;

                    GameGroups.Children.Add(item);
                });
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    GameGroups.Children.Clear();

                    var item = new GameControl();
                    item.SetItem(game);
                    item.DoubleTapped += Item_DoubleTapped;

                    GameItemSelect(item);

                    GameGroups.VerticalAlignment = VerticalAlignment.Center;
                    GameGroups.HorizontalAlignment = HorizontalAlignment.Center;

                    GameGroups.Children.Add(item);
                });
            }
        }
        else
        {
            var list = GameBinding.GetGameGroups();
            var uuid = ConfigBinding.GetLastLaunch();
            GameControl? last = null;
            if (first)
            {
                first = false;
                GameGroups.VerticalAlignment = VerticalAlignment.Top;
                GameGroups.HorizontalAlignment = HorizontalAlignment.Stretch;
                DefaultGroup.SetWindow(this);
                if (list.Count > 0)
                {
                    DefaultGroup.Expander = false;
                }

                foreach (var item in list)
                {
                    if (item.Key == " ")
                    {
                        DefaultGroup.SetItems(item.Value);
                        DefaultGroup.SetName(" ", App.GetLanguage("MainWindow.Info20"));
                        last ??= DefaultGroup.Find(uuid);
                    }
                    else
                    {
                        var group = new GamesControl();
                        group.SetItems(item.Value);
                        group.SetName(item.Key, item.Key);
                        group.SetWindow(this);
                        Groups.Add(group);
                        if (list.Count > 0)
                        {
                            group.Expander = false;
                        }
                        last ??= group.Find(uuid);
                    }
                }

                Dispatcher.UIThread.Post(() =>
                {
                    GameGroups.Children.Clear();
                    foreach (var item in Groups)
                    {
                        GameGroups.Children.Add(item);
                    }
                    GameGroups.Children.Add(DefaultGroup);
                    GameItemSelect(last);
                });
            }
            else
            {
                var remove = new List<GamesControl>();
                DefaultGroup.SetItems(list[DefaultGroup.Group]);
                last ??= DefaultGroup.Find(uuid);
                list.Remove(DefaultGroup.Group);
                foreach (var item in Groups)
                {
                    if (!list.TryGetValue(item.Group, out var value))
                    {
                        remove.Add(item);
                    }
                    else
                    {
                        item.SetItems(value);
                        list.Remove(item.Group);
                    }
                }
                foreach (var item in remove)
                {
                    Groups.Remove(item);
                }
                foreach (var item in list)
                {
                    var group = new GamesControl();
                    group.SetItems(item.Value);
                    group.SetName(item.Key, item.Key);
                    group.SetWindow(this);
                    Groups.Add(group);
                    if (list.Count > 0)
                    {
                        group.Expander = false;
                    }
                    last ??= group.Find(uuid);
                }

                Dispatcher.UIThread.Post(() =>
                {
                    GameGroups.Children.Clear();
                    foreach (var item in Groups)
                    {
                        GameGroups.Children.Add(item);
                        last ??= item.Find(uuid);
                    }
                    GameGroups.Children.Add(DefaultGroup);
                    GameItemSelect(last);
                });
            }
        }
    }

    private void Item_DoubleTapped(object? sender, TappedEventArgs e)
    {
        Launch(false);
    }

    public async void EditGroup(GameSettingObj obj)
    {
        await Group.Set(obj);
        Group.Close();
        if (Group.Cancel)
        {
            return;
        }

        var res = Group.Read();
        GameBinding.MoveGameGroup(obj, res);
    }

    public async Task AddGroup()
    {
        var window = App.FindRoot(VisualRoot);
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
    }

    public async void DeleteGame(GameSettingObj obj, bool force)
    {
        var window = App.FindRoot(VisualRoot);
        if (!force)
        {
            var res = await window.OkInfo.ShowWait(
                string.Format(App.GetLanguage("MainWindow.Info19"), obj.Name));
            if (!res)
                return;
        }

        var res1 = await GameBinding.DeleteGame(obj);
        if (!res1)
        {
            window.ProgressInfo.Show(App.GetLanguage("MainWindow.Info37"));
        }
    }

    public async void Rename(GameSettingObj obj)
    {
        var window = App.FindRoot(VisualRoot);
        await window.InputInfo.ShowEdit(App.GetLanguage("MainWindow.Info23"), obj.Name);
        if (window.InputInfo.Cancel)
            return;
        var data = window.InputInfo.Read().Item1;
        if (string.IsNullOrWhiteSpace(data))
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Error3"));
            return;
        }

        GameBinding.SetGameName(obj, data);
    }

    public async void Copy(GameSettingObj obj)
    {
        var window = App.FindRoot(VisualRoot);
        await window.InputInfo.ShowEdit(App.GetLanguage("MainWindow.Info23"),
            obj.Name + App.GetLanguage("MainWindow.Info24"));
        if (window.InputInfo.Cancel)
            return;
        var data = window.InputInfo.Read().Item1;
        if (string.IsNullOrWhiteSpace(data))
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Error3"));
            return;
        }

        var res = await GameBinding.CopyGame(obj, data);
        if (!res)
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Error5"));
            return;
        }
        else
        {
            window.NotifyInfo.Show(App.GetLanguage("MainWindow.Info25"));
        }
    }

    public async Task<bool> Closing()
    {
        var windows = App.FindRoot(VisualRoot);
        if (launch)
        {
            var res = await windows.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info34"));
            if (res)
            {
                return false;
            }
            return true;
        }

        if (BaseBinding.IsGameRuning())
        {
            App.Hide();
            return true;
        }

        return false;
    }
}
