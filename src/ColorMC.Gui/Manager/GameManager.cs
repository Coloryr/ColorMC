using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;
using System.Collections.Generic;

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
    /// 游戏日志
    /// </summary>
    public static readonly Dictionary<string, GameLog> GameLogs = [];

    /// <summary>
    /// 游戏实例是否标星
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>是否被标星</returns>
    public static bool IsStar(GameSettingObj game)
    {
        var config = GameGuiSetting.ReadConfig(game);
        return config.IsStar;
    }

    /// <summary>
    /// 标星游戏实例
    /// </summary>
    /// <param name="game">游戏实例</param>
    public static void AddStar(GameSettingObj game)
    {
        var config = GameGuiSetting.ReadConfig(game);
        config.IsStar = true;
        GameGuiSetting.WriteConfig(game, config);
    }

    /// <summary>
    /// 取消标星
    /// </summary>
    /// <param name="game">游戏实例</param>
    public static void RemoveStar(GameSettingObj game)
    {
        var config = GameGuiSetting.ReadConfig(game);
        config.IsStar = false;
        GameGuiSetting.WriteConfig(game, config);
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
    public static void AddGameLog(GameSettingObj obj, string? data)
    {
        if (GameLogs.TryGetValue(obj.UUID, out var log))
        {
            var item = log.AddLog(data);

            if (item != null && WindowManager.GameLogWindows.TryGetValue(obj.UUID, out var win))
            {
                win.Log(item);
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

        //清空日志
        if (GameLogs.TryGetValue(obj.UUID, out GameLog? value))
        {
            value.Clear();
        }
        else
        {
            GameLogs.Add(obj.UUID, new());
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
        if (GameLogs.TryGetValue(obj.UUID, out var data))
        {
            return data.GetLog(level);
        }

        return null;
    }
}
