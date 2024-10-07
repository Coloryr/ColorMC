using System;
using System.Collections.Generic;
using System.Text;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Manager;

public static class GameManager
{
    /// <summary>
    /// 正在运行的游戏
    /// </summary>
    public static readonly List<string> RunGames = [];
    /// <summary>
    /// 游戏日志
    /// </summary>
    public static readonly Dictionary<string, GameLogObj> GameLogs = [];

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

    public static void GameExit(string uuid)
    {
        RunGames.Remove(uuid);
    }

    /// <summary>
    /// 强制停止游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void StopGame(GameSettingObj obj)
    {
        ColorMCCore.KillGame(obj.UUID);
    }

    public static void StartGame(GameSettingObj obj)
    {
        RunGames.Add(obj.UUID);
    }

    public static void ClearGameLog(string uuid)
    {
        if (WindowManager.GameLogWindows.TryGetValue(uuid, out var win))
        {
            win.ClearLog();
        }

        //清空日志
        if (GameLogs.TryGetValue(uuid, out GameLogObj? value))
        {
            value.Clear();
        }
        else
        {
            GameLogs.Add(uuid, new());
        }
    }

    public static List<GameLogItemObj>? GetGameLog(string uuid, LogLevel level)
    {
        if (GameLogs.TryGetValue(uuid, out var data))
        {
            return data.GetLog(level);
        }

        return null;
    }
}
