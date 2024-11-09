using System;
using System.Collections.Generic;
using System.IO;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;
using Newtonsoft.Json;

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
    public static readonly Dictionary<string, GameLog> GameLogs = [];

    /// <summary>
    /// 标星实例
    /// </summary>
    public static readonly List<string> StarGames = [];

    private static string s_file;

    public static void Init(string path)
    {
        s_file = Path.GetFullPath(path + "/star.json");
        LoadState();
    }

    private static void LoadState()
    {
        if (File.Exists(s_file))
        {
            try
            {
                var data = PathHelper.ReadText(s_file);
                if (data == null)
                {
                    return;
                }
                var state = JsonConvert.DeserializeObject<List<string>>(data);
                if (state == null)
                {
                    return;
                }

                StarGames.AddRange(state);
            }
            catch (Exception e)
            {
                Logs.Error("", e);
            }
        }
    }

    private static void SaveStar()
    {
        ConfigSave.AddItem(new()
        {
            Name = "ColorMC_Star",
            File = s_file,
            Obj = StarGames
        });
    }

    public static bool IsStar(string uuid)
    {
        return StarGames.Contains(uuid);
    }

    public static void AddStar(string uuid)
    {
        if (!StarGames.Contains(uuid))
        {
            StarGames.Add(uuid);
            SaveStar();
        }
    }

    public static void RemoveStar(string uuid)
    {
        if (StarGames.Remove(uuid))
        {
            SaveStar();
        }
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
        if (GameLogs.TryGetValue(uuid, out GameLog? value))
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
