using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏日志处理
/// </summary>
public partial class GameRuntimeLog
{
    //正则
    [GeneratedRegex(@"\[(.*?)\] \[(.*?)(?:\/(.*?))?\]:? \[(.*?)\](?: (.*))?")]
    private static partial Regex LogRegex();
    [GeneratedRegex(@"\[(.*?)\] \[(.*?)(?:\/(.*?))?\]:?")]
    private static partial Regex LogRegex1();

    /// <summary>
    /// 当前进程游戏日志
    /// </summary>
    private readonly ConcurrentBag<GameLogItemObj> _logs = [];
    /// <summary>
    /// 日志文件
    /// </summary>
    public string? File { get; init; }

    /// <summary>
    /// 清理当前进程游戏日志
    /// </summary>
    public void Clear()
    {
        _logs.Clear();
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
        var match = LogRegex().Match(data);
        GameLogItemObj? item1 = null;
        if (match is { Success: true, Groups.Count: 6 })
        {
            string time = match.Groups[1].Value;
            string thread = match.Groups[2].Value;
            string level = match.Groups[3].Value;
            string category = match.Groups[4].Value;

            item1 = new GameLogItemObj()
            {
                TimeSpan = DateTime.Now,
                Time = time,
                Thread = thread,
                Category = category,
                Log = log,
                Level = GetLevel(level)
            };
        }
        else
        {
            var match1 = LogRegex1().Match(data);
            if (match1 is { Success: true, Groups.Count: 4 })
            {
                string time = match1.Groups[1].Value;
                string thread = match1.Groups[2].Value;
                string level = match1.Groups[3].Value;

                item1 = new GameLogItemObj()
                {
                    TimeSpan = DateTime.Now,
                    Time = time,
                    Thread = thread,
                    Log = log,
                    Level = GetLevel(level)
                };
            }
        }

        item1 ??= new GameLogItemObj()
        {
            TimeSpan = DateTime.Now,
            Log = log,
            Level = LogLevel.None
        };

        _logs.Add(item1);

        return item1;
    }

    /// <summary>
    /// 获取某个等级的日志列表
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<GameLogItemObj> GetLog(LogLevel type)
    {
        var list = _logs.Where(item => (item.Level & type) == item.Level).ToList();

        list.Sort(GameLogItemObjComparer.Instance);
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
