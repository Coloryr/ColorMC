using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 游戏日志处理
/// </summary>
public partial class GameLog
{
    //正则
    private const string s_pattern = @"\[(.*?)\] \[(.*?)(?:\/(.*?))?\]:? \[(.*?)\](?: (.*))?";
    private const string s_pattern1 = @"\[(.*?)\] \[(.*?)(?:\/(.*?))?\]:?";

    [GeneratedRegex(s_pattern)]
    internal static partial Regex MyRegex();

    [GeneratedRegex(s_pattern1)]
    internal static partial Regex MyRegex1();

    private static readonly Regex s_regex = MyRegex();
    private static readonly Regex s_regex1 = MyRegex1();

    /// <summary>
    /// 当前进程游戏日志
    /// </summary>
    public ConcurrentBag<GameLogItemObj> Logs { get; init; } = [];

    /// <summary>
    /// 清理当前进程游戏日志
    /// </summary>
    public void Clear()
    {
        Logs.Clear();
    }

    /// <summary>
    /// 添加日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public GameLogItemObj? AddLog(string? log)
    {
        if (log == null)
        {
            return null;
        }

        //正则拆分
        var data = log.Trim();
        var match = s_regex.Match(data);
        GameLogItemObj? item1 = null;
        if (match.Success)
        {
            if (match.Groups.Count == 6)
            {
                string time = match.Groups[1].Value;
                string thread = match.Groups[2].Value;
                string level = match.Groups[3].Value;
                string category = match.Groups[4].Value;

                item1 = new GameLogItemObj()
                {
                    Time = time,
                    Thread = thread,
                    Category = category,
                    Log = log,
                    Level = GetLevel(level)
                };
            }
        }
        else
        {
            var match1 = s_regex1.Match(data);
            if (match1.Success)
            {
                if (match1.Groups.Count == 4)
                {
                    string time = match1.Groups[1].Value;
                    string thread = match1.Groups[2].Value;
                    string level = match1.Groups[3].Value;

                    item1 = new GameLogItemObj()
                    {
                        Time = time,
                        Thread = thread,
                        Log = log,
                        Level = GetLevel(level)
                    };
                }
            }
        }

        item1 ??= new GameLogItemObj()
        {
            Log = log,
            Level = LogLevel.None
        };

        Logs.Add(item1);

        return item1;
    }

    /// <summary>
    /// 获取某个等级的日志列表
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<GameLogItemObj> GetLog(LogLevel type)
    {
        var list = new List<GameLogItemObj>();
        foreach (var item in Logs)
        {
            if ((item.Level & type) == item.Level)
            {
                list.Add(item);
            }
        }

        list.Reverse();

        return list;
    }

    /// <summary>
    /// 判断日志等级
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private static LogLevel GetLevel(string level)
    {
        level = level.ToLower();
        return level switch
        {
            "info" => LogLevel.Info,
            "warn" => LogLevel.Warn,
            "error" => LogLevel.Error,
            "fatal" => LogLevel.Error,
            "debug" => LogLevel.Debug,
            _ => LogLevel.None,
        };
    }
}
