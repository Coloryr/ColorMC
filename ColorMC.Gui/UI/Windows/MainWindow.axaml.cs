using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class MainWindow : Window
{
    private readonly List<GamesControl> Groups = new();
    private GamesControl DefaultGroup;
    private GameSettingObj? Obj;

    public delegate void UserEditHandler();
    public static event UserEditHandler UserEdit;

    public MainWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;

        ItemInfo.SetWindow(this);

        Task.Run(Load);
        Task.Run(Load1);

        CoreMain.GameLaunch = GameLunch;
        CoreMain.GameDownload = GameDownload;

        UserEdit += MainWindow_OnUserEdit;
        Opened += MainWindow_Opened;
        Closed += MainWindow_Closed;

        Update();
    }

    public async void Launch(bool debug)
    {
        Info1.Show("正在启动游戏");
        var res = await GameBinding.Launch(Obj, debug);
        if (res.Item1 == false)
        {
            Info1.Close();
            Info.Show(res.Item2!);
        }
    }

    public static void OnUserEdit()
    {
        if (UserEdit != null)
        {
            UserEdit();
        }
    }

    private void MainWindow_OnUserEdit()
    {
        Task.Run(Load1);
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        App.MainWindow = null;
        CoreMain.GameLaunch = null;
        CoreMain.GameDownload = null;
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        ItemInfo.Expander1.MakeTran();
    }

    public void GameItemSelect(GameSettingObj? obj)
    {
        Obj = obj;
        ItemInfo.SetGame(obj);
    }

    private async Task<bool> GameDownload(LaunchState state, GameSettingObj obj)
    {
        switch (state)
        {
            default:
            case LaunchState.LostGame:
            case LaunchState.LostVersion:
                return await Info.ShowWait("是否下载缺失的游戏文件");
            case LaunchState.LostLib:
                return await Info.ShowWait("是否下载缺失的运行库文件");
            case LaunchState.LostLoader:
                return await Info.ShowWait("是否下载缺失的Mod加载器");
            case LaunchState.LostLoginCore:
                return await Info.ShowWait("是否下载缺失的外置登陆器");
        } 
    }

    private void GameLunch(GameSettingObj obj, LaunchState state)
    {
        switch (state)
        {
            case LaunchState.Login:
                Info1.NextText("正在登录账户");
                break;
            case LaunchState.Check:
                Info1.NextText("正在检测游戏文件");
                break;
            case LaunchState.CheckVersion:
                Info1.NextText("正在检测游戏核心文件");
                break;
            case LaunchState.CheckLib:
                Info1.NextText("正在检测游戏运行库");
                break;
            case LaunchState.CheckAssets:
                Info1.NextText("正在检测游戏资源文件");
                break;
            case LaunchState.CheckLoader:
                Info1.NextText("正在检测游戏Mod加载器");
                break;
            case LaunchState.CheckLoginCore:
                Info1.NextText("正在检测游戏外置登陆器");
                break;
            case LaunchState.Download:
                Info1.NextText("正在下载所需文件");
                break;
            case LaunchState.JvmPrepare:
                Info1.NextText("正在准备Jvm参数");
                break;

            case LaunchState.LoginFail:
                Info1.Close();
                Info.Show("账户登录失败");
                break;
        }
    }

    private void Load1()
    {
        Dispatcher.UIThread.Post(() => { ItemInfo.SetUser(UserBinding.GetLastUser()); });
    }

    private async void Load()
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
                    DefaultGroup.SetName("默认分组");
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
    }

    public void Update()
    {
        App.Update(this, Image_Back, Rectangle1);
    }
}
