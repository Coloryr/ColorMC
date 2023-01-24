using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class MainWindow : Window
{
    private readonly List<GamesControl> Groups = new();
    private GamesControl DefaultGroup;
    private GameControl? Obj;
    private Dictionary<GameSettingObj, GameControl> Launchs = new();

    public delegate void UserEditHandler();
    public static event UserEditHandler UserEdit;

    private LaunchState Last;

    public MainWindow()
    {
        InitializeComponent();

        Icon = App.Icon;

        Rectangle1.MakeResizeDrag(this);

        ItemInfo.SetWindow(this);

        Load();
        Load1();

        CoreMain.GameLaunch = GameLunch;
        CoreMain.GameDownload = GameDownload;

        UserEdit += MainWindow_OnUserEdit;
        Opened += MainWindow_Opened;
        Closed += MainWindow_Closed;

        App.PicUpdate += Update;

        Update();
    }

    public async void Launch(bool debug)
    {
        Info1.Show(Localizer.Instance["MainWindow.Launch"]);
        var res = await GameBinding.Launch(Obj!.Obj, debug);
        Info1.Close();
        if (res.Item1 == false)
        {
            switch (Last)
            {
                case LaunchState.LoginFail:
                    Info.Show(Localizer.Instance["MainWindow.Error1"]);
                    break;
                case LaunchState.JvmError:
                    Info.Show(Localizer.Instance["MainWindow.Error2"]);
                    break;
                default:
                    Info.Show(res.Item2!);
                    break;
            }
        }
        else
        {
            Launchs.Add(Obj.Obj, Obj);
            Obj.SetLaunch(true);
            Info2.Show("已启动");
        }
    }

    public static void OnUserEdit()
    {
        if (UserEdit != null)
        {
            UserEdit();
        }
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

        App.MainWindow = null;
        CoreMain.GameLaunch = null;
        CoreMain.GameDownload = null;

        App.Close();
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        ServerMotdControl1.Load("color.coloryr.xyz", 25565);

        ItemInfo.Expander1.MakeTran();
    }

    public void GameItemSelect(GameControl? obj)
    {
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
            switch (state)
            {
                default:
                case LaunchState.LostGame:
                case LaunchState.LostVersion:
                    return await Info.ShowWait(Localizer.Instance["MainWindow.Lost1"]);
                case LaunchState.LostLib:
                    return await Info.ShowWait(Localizer.Instance["MainWindow.Lost2"]);
                case LaunchState.LostLoader:
                    return await Info.ShowWait(Localizer.Instance["MainWindow.Lost3"]);
                case LaunchState.LostLoginCore:
                    return await Info.ShowWait(Localizer.Instance["MainWindow.Lost4"]);
            }
        });
    }

    private void GameLunch(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Last = state;
            switch (state)
            {
                case LaunchState.Login:
                    Info1.NextText(Localizer.Instance["MainWindow.Check1"]);
                    break;
                case LaunchState.Check:
                    Info1.NextText(Localizer.Instance["MainWindow.Check2"]);
                    break;
                case LaunchState.CheckVersion:
                    Info1.NextText(Localizer.Instance["MainWindow.Check3"]);
                    break;
                case LaunchState.CheckLib:
                    Info1.NextText(Localizer.Instance["MainWindow.Check4"]);
                    break;
                case LaunchState.CheckAssets:
                    Info1.NextText(Localizer.Instance["MainWindow.Check5"]);
                    break;
                case LaunchState.CheckLoader:
                    Info1.NextText(Localizer.Instance["MainWindow.Check6"]);
                    break;
                case LaunchState.CheckLoginCore:
                    Info1.NextText(Localizer.Instance["MainWindow.Check7"]);
                    break;
                case LaunchState.CheckMods:
                    Info1.NextText(Localizer.Instance["MainWindow.Check10"]);
                    break;
                case LaunchState.Download:
                    Info1.NextText(Localizer.Instance["MainWindow.Check8"]);
                    break;
                case LaunchState.JvmPrepare:
                    Info1.NextText(Localizer.Instance["MainWindow.Check9"]);
                    break;
            }
        });
    }

    private void Load1()
    {
        ItemInfo.SetUser(UserBinding.GetLastUser());
    }

    public void Load()
    {
        Task.Run(async () =>
        {
            var list = GameBinding.GetGameGroups();

            Groups.Clear();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
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
                        DefaultGroup.SetName(Localizer.Instance["MainWindow.DefaultGroup"]);
                    });
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        var group = new GamesControl();
                        group.SetItems(item.Value);
                        group.SetName(item.Key);
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
        });
    }

    public void Update()
    {
        App.Update(this, Image_Back, Grid1);
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
        await Info3.ShowOne(Localizer.Instance["MainWindow.Info1"], false);
        Info3.Close();
        if (Info3.Cancel)
        {
            return;
        }

        var res = Info3.Read().Item1;
        if (string.IsNullOrWhiteSpace(res))
        {
            Info1.Show(Localizer.Instance["MainWindow.Error4"]);
            return;
        }

        if (!GameBinding.AddGameGroup(res))
        {
            Info1.Show(Localizer.Instance["MainWindow.Error5"]);
            return;
        }
    }

    public async void DeleteGame(GameSettingObj obj)
    {
        var res = await Info.ShowWait(string.Format("是否要删除实例 {0}", obj.Name));
        if (!res)
            return;

        await GameBinding.DeleteGame(obj);
    }
}
