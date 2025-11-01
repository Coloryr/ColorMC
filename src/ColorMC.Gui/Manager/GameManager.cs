using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 游戏实例管理器
/// </summary>
public static class GameManager
{
    /// <summary>
    /// 正在运行的游戏实例
    /// </summary>
    public static readonly HashSet<string> RunGames = [];

    /// <summary>
    /// 正在下载资源的游戏实例
    /// </summary>
    public static readonly HashSet<string> AddRunGames = [];

    /// <summary>
    /// 界面设置
    /// </summary>
    private readonly static Dictionary<string, GameGuiSettingObj> s_datas = [];

    /// <summary>
    /// 游戏是否链接到ColorMC
    /// </summary>
    private readonly static Dictionary<string, bool> s_gameConnect = [];

    /// <summary>
    /// 游戏实例启动取消操作
    /// </summary>
    private readonly static Dictionary<string, CancellationTokenSource> s_gameCancel = [];

    /// <summary>
    /// 设置ColorMCASM链接状态
    /// </summary>
    /// <param name="uuid">游戏实例</param>
    /// <param name="connect">是否链接</param>
    public static void SetConnect(string uuid, bool connect)
    {
        s_gameConnect[uuid] = false;
    }

    /// <summary>
    /// 游戏实例开始添加资源<br/>
    /// 锁定该游戏实例无法删除
    /// </summary>
    /// <param name="uuid">游戏实例</param>
    public static void StartAdd(string uuid)
    {
        AddRunGames.Add(uuid);
    }

    /// <summary>
    /// 游戏实例结束添加资源
    /// </summary>
    /// <param name="uuid">游戏实例</param>
    public static void StopAdd(string uuid)
    {
        AddRunGames.Remove(uuid);
    }

    /// <summary>
    /// 游戏实例是否正在添加资源
    /// </summary>
    /// <param name="game">游戏实例</param>
    public static bool IsAdd(GameSettingObj game)
    {
        return AddRunGames.Contains(game.UUID);
    }

    /// <summary>
    /// 获取游戏实例对应的界面设置
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static GameGuiSettingObj ReadConfig(GameSettingObj obj)
    {
        if (s_datas.TryGetValue(obj.UUID, out var setting))
        {
            return setting;
        }

        var path = obj.GetBasePath();
        var dir = Path.Combine(path, GuiNames.NameGameGuiConfigFile);

        if (File.Exists(dir))
        {
            try
            {
                using var data = PathHelper.OpenRead(dir);
                var obj1 = JsonUtils.ToObj(data, JsonGuiType.GameGuiSettingObj);
                if (obj1 != null)
                {
                    obj1.Log ??= MakeLog();
                    obj1.Mod ??= MakeMod();
                    obj1.ModName ??= [];
                    s_datas.Add(obj.UUID, obj1);
                    return obj1;
                }
            }
            catch
            {

            }
        }

        return Make();
    }

    /// <summary>
    /// 保存游戏实例对应的界面设置
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="config">配置</param>
    public static void WriteConfig(GameSettingObj obj, GameGuiSettingObj config)
    {
        if (!s_datas.TryAdd(obj.UUID, config))
        {
            s_datas[obj.UUID] = config;
        }

        var dir = Path.Combine(obj.GetBasePath(), GuiNames.NameGameGuiConfigFile);

        ConfigSave.AddItem(ConfigSaveObj.Build($"GameLogSetting:{obj.UUID}", dir,
            config, JsonGuiType.GameGuiSettingObj));
    }

    //创建基础配置
    private static GameGuiSettingObj Make()
    {
        return new()
        {
            Log = MakeLog(),
            Mod = MakeMod(),
            ModName = []
        };
    }

    private static GameLogSettingObj MakeLog()
    {
        return new()
        {
            Auto = true,
            EnableDebug = true,
            EnableError = true,
            EnableInfo = true,
            EnableNone = true,
            EnableWarn = true,
            WordWrap = true
        };
    }

    private static GameModSettingObj MakeMod()
    {
        return new()
        {
            EnableModId = true,
            EnableLoader = true,
            EnableSide = true,
            EnableVersion = true,
            EnableName = true
        };
    }

    /// <summary>
    /// 游戏实例是否标星
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>是否被标星</returns>
    public static bool IsStar(GameSettingObj game)
    {
        var config = ReadConfig(game);
        return config.IsStar;
    }

    /// <summary>
    /// 标星游戏实例
    /// </summary>
    /// <param name="game">游戏实例</param>
    public static void AddStar(GameSettingObj game)
    {
        var config = ReadConfig(game);
        config.IsStar = true;
        WriteConfig(game, config);
    }

    /// <summary>
    /// 取消标星
    /// </summary>
    /// <param name="game">游戏实例</param>
    public static void RemoveStar(GameSettingObj game)
    {
        var config = ReadConfig(game);
        config.IsStar = false;
        WriteConfig(game, config);
    }

    /// <summary>
    /// 游戏实例是否在运行
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static bool IsGameRun(GameSettingObj obj)
    {
        return RunGames.Contains(obj.UUID);
    }

    /// <summary>
    /// 是否有游戏在运行
    /// </summary>
    public static bool IsGameRuning()
    {
        return RunGames.Count > 0;
    }

    /// <summary>
    /// 游戏进程有日志
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    public static void AddGameLog(GameLogEventArgs args)
    {
        if (args.Log != null)
        {
            //给系统日志填充内容
            if (args.Log.LogType != GameSystemLog.None)
            {
                args.Log.Log = LanguageUtils.Get("Core.Info28");
            }

            if (WindowManager.GameLogWindows.TryGetValue(args.Game.UUID, out var win))
            {
                win.Log(args.Log);
            }
        }
    }

    /// <summary>
    /// 游戏退出
    /// </summary>
    /// <param name="uuid">游戏实例</param>
    public static void GameExit(GameSettingObj obj)
    {
        RunGames.Remove(obj.UUID);
    }

    /// <summary>
    /// 强制停止游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void KillGame(GameSettingObj obj)
    {
        ColorMCCore.KillGame(obj.UUID);
    }

    /// <summary>
    /// 游戏实例启动
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static CancellationToken StartGame(GameSettingObj obj)
    {
        RunGames.Add(obj.UUID);
        if (s_gameCancel.TryGetValue(obj.UUID, out var temp))
        {
            temp.Cancel();
            temp.Dispose();
        }

        var cancel = new CancellationTokenSource();
        s_gameCancel[obj.UUID] = cancel;

        return cancel.Token;
    }

    /// <summary>
    /// 清理游戏日志
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ClearGameLog(GameSettingObj obj)
    {
        if (WindowManager.GameLogWindows.TryGetValue(obj.UUID, out var win))
        {
            win.ClearLog();
        }
    }

    /// <summary>
    /// 获取游戏日志
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="level">日志等级</param>
    /// <returns>日志列表</returns>
    public static List<GameLogItemObj>? GetGameLog(GameSettingObj obj, LogLevel level)
    {
        if (ColorMCCore.GetGameRuntimeLog(obj) is { } log)
        {
            return log.GetLog(level);
        }

        return null;
    }

    /// <summary>
    /// 设置方块
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="block">方块ID</param>
    public static void SetGameBlock(GameSettingObj game, string block)
    {
        var config = ReadConfig(game);
        config.Block = block;
        WriteConfig(game, config);
    }

    /// <summary>
    /// 获取方块
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>方块ID</returns>
    public static string GetGameBlock(GameSettingObj game)
    {
        var config = ReadConfig(game);
        return config.Block;
    }

    /// <summary>
    /// 取消启动游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void CancelLaunch(GameSettingObj obj)
    {
        if (s_gameCancel.TryGetValue(obj.UUID, out var cancel))
        {
            if (!cancel.IsCancellationRequested)
            {
                cancel.Cancel();
            }
        }
    }

    /// <summary>
    /// 游戏进程已通过netty连接启动器
    /// </summary>
    /// <param name="uuid">游戏实例</param>
    public static void GameConnect(string uuid)
    {
        s_gameConnect[uuid] = true;
    }

    /// <summary>
    /// 游戏进程是否已经链接
    /// </summary>
    /// <param name="uuid">游戏实例</param>
    /// <returns></returns>
    public static bool IsConnect(string uuid)
    {
        if (s_gameConnect.TryGetValue(uuid, out var value))
        {
            return value;
        }

        return false;
    }

    /// <summary>
    /// 游戏进程启动后
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="handel"></param>
    public static void StartGameHandel(GameSettingObj obj, GameHandel handel)
    {
        new Thread(() =>
        {
            try
            {
                var conf = obj.Window;

                do
                {
                    if (handel.IsExit || IsConnect(obj.UUID))
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }
                while (true);

                if (handel.IsExit)
                {
                    return;
                }

                var config = GuiConfigUtils.Config.Input;

                //启用手柄支持
                if (config.Enable && !config.Disable && !handel.IsOutAdmin)
                {
                    GameJoystick.Start(obj, handel);
                }

                //修改窗口标题
                if (string.IsNullOrWhiteSpace(conf?.GameTitle))
                {
                    return;
                }

                var ran = new Random();
                int i = 0;
                var list = new List<string>();
                var list1 = conf.GameTitle.Split('\n');

                foreach (var item in list1)
                {
                    var temp = item.Trim();
                    if (string.IsNullOrWhiteSpace(temp))
                    {
                        continue;
                    }

                    list.Add(temp);
                }
                if (list.Count == 0)
                {
                    return;
                }

                Thread.Sleep(1000);

                //循环设置窗口标题
                do
                {
                    string title1 = "";
                    if (conf.RandomTitle)
                    {
                        title1 = list[ran.Next(list.Count)];
                    }
                    else
                    {
                        i++;
                        if (i >= list.Count)
                        {
                            i = 0;
                        }
                        title1 = list[i];
                    }

                    LaunchSocketUtils.SetTitle(obj, title1);

                    if (!conf.CycTitle || conf.TitleDelay <= 0 || handel.IsExit)
                    {
                        break;
                    }

                    Thread.Sleep(conf.TitleDelay);
                }
                while (!ColorMCGui.IsClose && !handel.IsExit);
            }
            catch
            {

            }
        })
        {
            Name = "ColorMC Game " + handel.UUID + " Handel",
            IsBackground = true
        }.Start();
    }
}
