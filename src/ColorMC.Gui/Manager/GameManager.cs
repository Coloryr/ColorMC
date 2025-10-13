using System.Collections.Generic;
using System.IO;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 游戏实例管理器
/// </summary>
public static class GameManager
{
    /// <summary>
    /// 正在运行的游戏
    /// </summary>
    public static readonly List<string> RunGames = [];

    /// <summary>
    /// 界面设置
    /// </summary>
    private readonly static Dictionary<string, GameGuiSettingObj> s_datas = [];

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
    public static void AddGameLog(GameSettingObj obj, GameLogItemObj? data)
    {
        if (data != null && WindowManager.GameLogWindows.TryGetValue(obj.UUID, out var win))
        {
            win.Log(data);
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
    public static void StartGame(GameSettingObj obj)
    {
        RunGames.Add(obj.UUID);
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
}
