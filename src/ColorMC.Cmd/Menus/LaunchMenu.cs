using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using System.Diagnostics;

namespace ColorMC.Cmd.Menus;

public static class LaunchMenu
{
    private const string Title = "启动游戏";
    private const string Select1 = "选择实例";

    private static GameSettingObj? game;

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
        ColorMCCore.GameLaunch = GameLaunch;
        ColorMCCore.GameDownload = GameDownload;
        ColorMCCore.ProcessLog = ProcessLog;

        if (obj.AuthType != AuthType.Offline)
        {
            ConsoleUtils.Info1("正在刷新登录");

            var (State, State1, Obj, Message, Ex) = BaseAuth.RefreshToken(obj).Result;

            if (State1 != LoginState.Done)
            {
                ConsoleUtils.Error($"{State.GetName()}刷新登录错误");
                ConsoleUtils.Error(Message);
                ConsoleUtils.Keep();
                return;
            }

            Obj!.Save();
            obj = Obj!;
        }
        var res = game?.StartGame(obj!).Result;
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

    public static void GameLaunch(GameSettingObj obj, LaunchState state)
    {
        ConsoleUtils.Info1($"{state.GetName()}");
    }

    public static Task<bool> GameDownload(LaunchState state, GameSettingObj obj)
    {
        return Task.Run(() =>
        {
            return ConsoleUtils.YesNo("游戏启动缺少必要文件，是否下载");
        });
    }

    public static void ProcessLog(Process? p, string? m)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(m);
    }
}
