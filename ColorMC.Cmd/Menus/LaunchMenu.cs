using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ColorMC.Cmd.Menus;

public static class LaunchMenu
{
    private const string Title = "启动游戏";
    private const string Select1 = "选择实例";

    private static GameSettingObj game;
    private static DownloadItem[] Items;
    private static ProgressBar Bar;

    public static void Show()
    {
        var list = InstancesPath.Games;
        var items = new List<string>();
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);
        items.Add("返回");
        if (list.Count == 0)
        {
            ConsoleUtils.ShowTitle1("没有实例");
            ConsoleUtils.Keep();
            MainMenu.Show();
            return;
        }
        else
        {
            ConsoleUtils.ShowTitle1(Select1);
            list.ForEach(item => items.Add("[" + item.Name + "|" + item.Version + "]"));
        }

        ConsoleUtils.SetItems(items, Select);
    }

    private static void Select(int index)
    {
        switch (index)
        {
            case 0:
                MainMenu.Show();
                break;
            default:
                var list = AuthDatabase.Auths;
                if (list.Count == 0)
                {
                    ConsoleUtils.ShowTitle1("没有账户");
                    ConsoleUtils.Keep();
                    break;
                }
                game = InstancesPath.Games.ToArray()[index - 1];
                SelectUserMenu.Show();
                break;
        }
    }

    public static void SelectItem(LoginObj obj)
    {
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle("启动游戏");
        ConsoleUtils.ShowTitle1(game.Name);
        CoreMain.GameLaunch = GameLaunch;
        CoreMain.GameDownload = GameDownload;
        CoreMain.DownloadItemStateUpdate = DownloadUpdate;
        CoreMain.DownloaderUpdate = DownloaderUpdate;
        CoreMain.ProcessLog = ProcessLog;

        if (obj.AuthType != AuthType.Offline)
        {
            ConsoleUtils.Info1("正在刷新登录");

            var (State, State1, Obj, Message) = AuthHelper.RefreshToken(obj).Result;

            if (State1 != LoginState.Done)
            {
                ConsoleUtils.Error($"{State.GetName()}刷新登录错误");
                ConsoleUtils.Error(Message);
                ConsoleUtils.Keep();
                return;
            }

            AuthDatabase.SaveAuth(Obj);
            obj = Obj;
        }
        var res = game?.StartGame(obj).Result;
        if (res == null)
        {
            ConsoleUtils.Error("游戏启动失败");
            game = null;
            ConsoleUtils.Keep();
            Show();
        }
        else
        {
            ConsoleUtils.Ok($"游戏于进程:{res.ProcessName} {res.Id}");
            res.Exited += Res_Exited;
            Console.CursorVisible = true;
            while (true)
            {
                var data = Console.ReadLine();
                if (res.HasExited)
                {
                    Show();
                    return;
                }
                res.StandardInput.WriteLine(data);
            }
        }
    }

    private static void Res_Exited(object? sender, EventArgs e)
    {
        ConsoleUtils.Info1("游戏退出，按回车返回");
    }

    public static void DownloaderUpdate(CoreRunState state)
    {
        if (state == CoreRunState.Start)
        {
            ConsoleUtils.Info("开始下载文件");
            Console.ForegroundColor = ConsoleColor.White;
            Items = new DownloadItem[ConfigUtils.Config.Http.DownloadThread];
            Bar = new ProgressBar(ConfigUtils.Config.Http.DownloadThread);
        }
        else
        {
            Bar.Dispose();
        }
    }

    public static void DownloadUpdate(int index, DownloadItem item)
    {
        if (item.State == DownloadItemState.Done)
        {
            Items[index] = null;
            Bar.Done(index, $"{item.Name} 下载完成");
        }
        else if (item.State != DownloadItemState.Init)
        {
            Items[index] = item;
            Bar.SetName(index, item.Name);
            Bar.SetAllSize(index, item.AllSize);
            Bar.SetValue(index, item.NowSize);
        }
    }

    public static void GameLaunch(GameSettingObj obj, LaunchState state)
    {
        ConsoleUtils.Info1($"{state.GetName()}");
    }

    public static bool GameDownload(GameSettingObj obj)
    {
        return ConsoleUtils.YesNo("游戏启动缺少必要文件，是否下载");
    }

    public static void ProcessLog(Process? p, string? m)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(m);
    }
}
