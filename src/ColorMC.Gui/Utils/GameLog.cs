using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Utils;

public partial class GameLog
{
    private const string s_pattern = @"\[(.*?)\] \[(.*?)(?:\/(.*?))?\]:? \[(.*?)\](?: (.*))?";
    private const string s_pattern1 = @"\[(.*?)\] \[(.*?)(?:\/(.*?))?\]:?";

    [GeneratedRegex(s_pattern)]
    internal static partial Regex MyRegex();

    [GeneratedRegex(s_pattern1)]
    internal static partial Regex MyRegex1();

    private static readonly Regex s_regex = MyRegex();
    private static readonly Regex s_regex1 = MyRegex1();

    public List<GameLogItemObj> Logs { get; init; } = [];

    public void Clear()
    {
        Logs.Clear();
    }

    public GameLogItemObj? AddLog(string? log)
    {
        if (log == null)
        {
            return null;
        }

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

        return list;
    }

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
