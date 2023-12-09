using ColorMC.Core.Objs;
using System.Text.RegularExpressions;

namespace ColorMC.Core.Utils;

/// <summary>
/// 其他函数
/// </summary>
public static partial class FuntionUtils
{
    [GeneratedRegex("[^0-9]+")]
    private static partial Regex Regex1();

    [GeneratedRegex("^[a-zA-Z0-9]+$")]
    private static partial Regex Regex2();

    /// <summary>
    /// 检查是否为数字
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool CheckNotNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;
        return Regex1().IsMatch(input);
    }

    /// <summary>
    /// 检查是否为英文数字
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool CheckIs(string input)
    { 
        return Regex2().IsMatch(input);
    }

    /// <summary>
    /// Tick转时间
    /// </summary>
    /// <param name="unixTimeStamp"></param>
    /// <returns></returns>
    public static DateTime MillisecondsToDataTime(long unixTimeStamp)
    {
        var start = new DateTime(1970, 1, 1) +
            TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        return start.AddMilliseconds(unixTimeStamp);
    }

    /// <summary>
    /// 时间戳
    /// </summary>
    public static int GetTime()
    {
        var tick = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).Ticks;
        return (int)(tick / 10000000);
    }

    /// <summary>
    /// 新的UUID
    /// </summary>
    /// <returns></returns>
    public static string NewUUID()
    {
        return Guid.NewGuid().ToString().ToLower();
    }

    /// <summary>
    /// 版本名
    /// </summary>
    public static LibVersionObj MakeVersionObj(string name)
    {
        var arg = name.Split(":");
        if (arg.Length < 3)
        {
            return new()
            {
                Name = name
            };
        }
        if (arg.Length > 3)
        {
            return new()
            {
                Pack = arg[0],
                Name = arg[1],
                Verison = arg[2],
                Extr = arg[3]
            };
        }

        return new()
        {
            Pack = arg[0],
            Name = arg[1],
            Verison = arg[2]
        };
    }

    /// <summary>
    /// 执行内存回收
    /// </summary>
    public static void RunGC()
    {
        Task.Run(() =>
        {
            Task.Delay(1000).Wait();
            GC.Collect();
            GC.Collect();
        });
    }
}
