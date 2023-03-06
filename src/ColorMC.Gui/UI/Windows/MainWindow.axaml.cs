using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class MainWindow : Window, IBaseWindow
{
    private readonly List<GamesControl> Groups = new();
    private readonly Dictionary<GameSettingObj, GameControl> Launchs = new();
    private GamesControl DefaultGroup;
    private GameControl? Obj;

    private bool launch = false;
    private bool first = true;
    private LaunchState Last;

    Info3Control IBaseWindow.Info3 => Info3;

    Info1Control IBaseWindow.Info1 => Info1;

    Info4Control IBaseWindow.Info => Info;

    Info2Control IBaseWindow.Info2 => Info2;

    public MainWindow()
    {
        InitializeComponent();

        this.Init();
        Icon = App.Icon;
        Border1.MakeResizeDrag(this);

        ItemInfo.SetWindow(this);

        Load();
        Load1();

        ColorMCCore.GameLaunch = GameLunch;
        ColorMCCore.GameDownload = GameDownload;
        ColorMCCore.LoginFailLaunch = LoginFailLaunch;

        Button1.Click += Button1_Click;

        App.UserEdit += MainWindow_OnUserEdit;
        Opened += MainWindow_Opened;
        Closed += MainWindow_Closed;

        App.PicUpdate += Update;

        Update();

        Grid1.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        Grid1.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        Grid1.AddHandler(DragDrop.DropEvent, Drop);

        if (BaseBinding.IsLaunch())
        {
            Dispatcher.UIThread.Post(async () =>
            {
                await Info.ShowOk(App.GetLanguage("MainWindow.Info22"));
                Close();
            });
        }

        Activated += Window_Activated;

        if (BaseBinding.CheckOldDir())
        {
            Info.Show(App.GetLanguage("MainWindow.Info27"));
        }
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
        else if (e.Data.Contains(DataFormats.FileNames))
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
        else if (e.Data.Contains(DataFormats.FileNames))
        {
            var files = e.Data.GetFileNames();
            if (files == null || files.Count() > 1)
                return;

            var item = files.First();
            if (item.EndsWith(".zip") || item.EndsWith(".mrpack"))
            {
                App.ShowAddGame(item);
            }
        }
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        App.LastWindow = this;
    }

    private Task<bool> LoginFailLaunch(LoginObj login)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            return Info.ShowWait(string.Format(
                App.GetLanguage("MainWindow.Info21"), login.UserName));
        });
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        ItemInfo.Display();
    }

    public async void Launch(bool debug)
    {
        if (launch)
            return;

        launch = true;
        ItemInfo.SetLaunch(true);
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            Info1.Show(App.GetLanguage("MainWindow.Info3"));
        }
        var item = Obj!;
        var game = item.Obj;
        item.SetLaunch(false);
        item.SetLoad(true);
        Info2.Show(App.GetLanguage(string.Format(App.GetLanguage("MainWindow.Info28"), game.Name)));
        var res = await GameBinding.Launch(game, debug);
        Head.Title1 = null;
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            await Info1.Close();
        }
        if (res.Item1 == false)
        {
            item.SetLoad(false);
            item.SetLaunch(false);
            switch (Last)
            {
                case LaunchState.LoginFail:
                    Info.Show(App.GetLanguage("MainWindow.Error1"));
                    break;
                case LaunchState.JavaError:
                    Info.Show(App.GetLanguage("MainWindow.Error2"));
                    break;
                default:
                    Info.Show(res.Item2!);
                    break;
            }
        }
        else
        {
            Launchs.Add(game, item);
            item.SetLaunch(true);

            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                Info1.Show(App.GetLanguage("MainWindow.Info26"));
            }
        }
        launch = false;
        ItemInfo.SetLaunch(false);
    }

    public void GameClose(GameSettingObj obj)
    {
        if (Launchs.Remove(obj, out var con))
        {
            Dispatcher.UIThread.Post(() =>
            {
                con.SetLaunch(false);
            });
        }
    }

    private void MainWindow_OnUserEdit()
    {
        Dispatcher.UIThread.Post(Load1);
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;
        App.UserEdit -= MainWindow_OnUserEdit;

        ColorMCCore.GameLaunch = null;
        ColorMCCore.GameDownload = null;
        ColorMCCore.LoginFailLaunch = null;

        App.Close();
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
#if !DEBUG
        if (ConfigBinding.GetAllConfig().Item1?.Http?.CheckUpdate == true)
        {
            ColorMCGui.InitDone();
        }
#endif

        MotdLoad();
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
            return state switch
            {
                LaunchState.LostLib => await Info.ShowWait(App.GetLanguage("MainWindow.Info5")),
                LaunchState.LostLoader => await Info.ShowWait(App.GetLanguage("MainWindow.Info6")),
                LaunchState.LostLoginCore => await Info.ShowWait(App.GetLanguage("MainWindow.Info7")),
                _ => await Info.ShowWait(App.GetLanguage("MainWindow.Info4")),
            };
        });
    }

    private void GameLunch(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Last = state;
            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                switch (state)
                {
                    case LaunchState.Login:
                        Info1.NextText(App.GetLanguage("MainWindow.Info8"));
                        break;
                    case LaunchState.Check:
                        Info1.NextText(App.GetLanguage("MainWindow.Info9"));
                        break;
                    case LaunchState.CheckVersion:
                        Info1.NextText(App.GetLanguage("MainWindow.Info10"));
                        break;
                    case LaunchState.CheckLib:
                        Info1.NextText(App.GetLanguage("MainWindow.Info11"));
                        break;
                    case LaunchState.CheckAssets:
                        Info1.NextText(App.GetLanguage("MainWindow.Info12"));
                        break;
                    case LaunchState.CheckLoader:
                        Info1.NextText(App.GetLanguage("MainWindow.Info13"));
                        break;
                    case LaunchState.CheckLoginCore:
                        Info1.NextText(App.GetLanguage("MainWindow.Info14"));
                        break;
                    case LaunchState.CheckMods:
                        Info1.NextText(App.GetLanguage("MainWindow.Info17"));
                        break;
                    case LaunchState.Download:
                        Info1.NextText(App.GetLanguage("MainWindow.Info15"));
                        break;
                    case LaunchState.JvmPrepare:
                        Info1.NextText(App.GetLanguage("MainWindow.Info16"));
                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case LaunchState.Login:
                        Head.Title1 = App.GetLanguage("MainWindow.Info8");
                        break;
                    case LaunchState.Check:
                        Head.Title1 = App.GetLanguage("MainWindow.Info9");
                        break;
                    case LaunchState.CheckVersion:
                        Head.Title1 = App.GetLanguage("MainWindow.Info10");
                        break;
                    case LaunchState.CheckLib:
                        Head.Title1 = App.GetLanguage("MainWindow.Info11");
                        break;
                    case LaunchState.CheckAssets:
                        Head.Title1 = App.GetLanguage("MainWindow.Info12");
                        break;
                    case LaunchState.CheckLoader:
                        Head.Title1 = App.GetLanguage("MainWindow.Info13");
                        break;
                    case LaunchState.CheckLoginCore:
                        Head.Title1 = App.GetLanguage("MainWindow.Info14");
                        break;
                    case LaunchState.CheckMods:
                        Head.Title1 = App.GetLanguage("MainWindow.Info17");
                        break;
                    case LaunchState.Download:
                        Head.Title1 = App.GetLanguage("MainWindow.Info15");
                        break;
                    case LaunchState.JvmPrepare:
                        Head.Title1 = App.GetLanguage("MainWindow.Info16");
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

        Task.Run(async () =>
        {
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
                        item.SetSelect(true);
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
                if (first)
                {
                    first = false;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        GameGroups.VerticalAlignment = VerticalAlignment.Top;
                        GameGroups.HorizontalAlignment = HorizontalAlignment.Stretch;

                        DefaultGroup = new();
                    });
                    DefaultGroup.SetWindow(this);
                    foreach (var item in list)
                    {
                        if (item.Key == " ")
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                DefaultGroup.SetItems(item.Value);
                                DefaultGroup.SetName(" ", App.GetLanguage("MainWindow.Info20"));
                            });
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                var group = new GamesControl();
                                group.SetItems(item.Value);
                                group.SetName(item.Key, item.Key);
                                group.SetWindow(this);
                                Groups.Add(group);
                            });
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
                    });
                }
                else
                {
                    var remove = new List<GamesControl>();
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        DefaultGroup.SetItems(list[DefaultGroup.Group]);
                    });
                    list.Remove(DefaultGroup.Group);
                    foreach (var item in Groups)
                    {
                        if (!list.TryGetValue(item.Group, out var value))
                        {
                            remove.Add(item);
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                item.SetItems(value);
                            });
                            list.Remove(item.Group);
                        }
                    }
                    foreach (var item in remove)
                    {
                        Groups.Remove(item);
                    }
                    foreach (var item in list)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            var group = new GamesControl();
                            group.SetItems(item.Value);
                            group.SetName(item.Key, item.Key);
                            group.SetWindow(this);
                            Groups.Add(group);
                        });
                    }

                    Dispatcher.UIThread.Post(() =>
                    {
                        GameGroups.Children.Clear();
                        foreach (var item in Groups)
                        {
                            GameGroups.Children.Add(item);
                        }
                        GameGroups.Children.Add(DefaultGroup);
                    });
                }
            }
        });
    }

    private void Item_DoubleTapped(object? sender, TappedEventArgs e)
    {
        Launch(false);
    }

    public void Update()
    {
        App.Update(this, Image_Back, Border1, Border2);
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
        await Info3.ShowOne(App.GetLanguage("MainWindow.Info1"), false);
        if (Info3.Cancel)
        {
            return;
        }

        var res = Info3.Read().Item1;
        if (string.IsNullOrWhiteSpace(res))
        {
            Info.Show(App.GetLanguage("MainWindow.Error3"));
            return;
        }

        if (!GameBinding.AddGameGroup(res))
        {
            Info.Show(App.GetLanguage("MainWindow.Error4"));
            return;
        }
    }

    public async void DeleteGame(GameSettingObj obj)
    {
        var res = await Info.ShowWait(
            string.Format(App.GetLanguage("MainWindow.Info19"), obj.Name));
        if (!res)
            return;

        await GameBinding.DeleteGame(obj);
    }

    public async void Rename(GameSettingObj obj)
    {
        await Info3.ShowEdit(App.GetLanguage("MainWindow.Info23"), obj.Name);
        if (Info3.Cancel)
            return;
        var data = Info3.Read().Item1;
        if (string.IsNullOrWhiteSpace(data))
        {
            Info.Show(App.GetLanguage("MainWindow.Error3"));
            return;
        }

        BaseBinding.SetGameName(obj, data);
    }

    public async void Copy(GameSettingObj obj)
    {
        await Info3.ShowEdit(App.GetLanguage("MainWindow.Info23"),
            obj.Name + App.GetLanguage("MainWindow.Info24"));
        if (Info3.Cancel)
            return;
        var data = Info3.Read().Item1;
        if (string.IsNullOrWhiteSpace(data))
        {
            Info.Show(App.GetLanguage("MainWindow.Error3"));
            return;
        }

        var res = await BaseBinding.CopyGame(obj, data);
        if (!res)
        {
            Info.Show(App.GetLanguage("MainWindow.Error5"));
            return;
        }
        else
        {
            Info2.Show(App.GetLanguage("MainWindow.Info25"));
        }
    }
}
