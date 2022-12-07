using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Utils;
using ShellProgressBar;
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
    private static string Title = "启动游戏";
    private static string Select1 = "选择实例";

    private static GameSettingObj game;
    private static DownloadItem[] Items;
    private static ChildProgressBar[] Bars;
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

        ConsoleUtils.ShowItems(items, Select);
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

    public static void DownloaderUpdate(CoreRunState state)
    {
        if (state == CoreRunState.Start)
        {
            ConsoleUtils.Info("开始下载文件");
            Console.ForegroundColor = ConsoleColor.White;
            Items = new DownloadItem[ConfigUtils.Config.Http.DownloadThread];
            Bars = new ChildProgressBar[ConfigUtils.Config.Http.DownloadThread];
            const int totalTicks = 5;
            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkYellow,
                ProgressCharacter = '─',
                DisplayTimeInRealTime = true
            };
            var childOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Green,
                BackgroundColor = ConsoleColor.Gray,
                ProgressCharacter = '─'
            };
            Bar = new ProgressBar(10, "下载游戏文件", options);
            for (var a = 0; a < Bars.Length; a++)
            {
                Bars[a] = Bar.Spawn(totalTicks, "download", childOptions);
            }
        }
        else
        {
            Bar.Dispose();
        }
    }

    private static object obj = new object();

    public static void DownloadUpdate(int index, DownloadItem item)
    {
        if (item.State == DownloadItemState.Done)
        {
            Items[index] = null;
            Bars[index].MaxTicks = 0;
            Bars[index].Tick();
        }
        else if (item.State != DownloadItemState.Init)
        {
            lock (obj)
            {
                Items[index] = item;
                Bars[index].Message = item.Name;
                Bars[index].MaxTicks = (int)(item.AllSize / 1000);
                Bars[index].Tick((int)(item.NowSize / 1000));
            }
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
