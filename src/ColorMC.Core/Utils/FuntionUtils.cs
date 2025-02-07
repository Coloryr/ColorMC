using ColorMC.Core.Objs;

namespace ColorMC.Core.Utils;

/// <summary>
/// 其他函数
/// </summary>
public static class FuntionUtils
{
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
    /// 通过MavenId获取文件路径
    /// </summary>
    /// <param name="input">MavenId</param>
    /// <returns>路径</returns>
    public static string VersionNameToPath(string input)
    {
        var arg = input.Split(':');
        string path = "";
        foreach (var item in arg[0].Split('.'))
        {
            path += item + '/';
        }
        if (arg.Length > 3)
        {
            path += $"{arg[1]}/{arg[2]}/{arg[1]}-{arg[2]}-{arg[3]}.jar";
        }
        else
        {
            path += $"{arg[1]}/{arg[2]}/{arg[1]}-{arg[2]}.jar";
        }

        return path;
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
        });
    }

    /// <summary>
    /// 添加或更新字典
    /// </summary>
    /// <param name="dic">源字典</param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    public static void AddOrUpdate(this Dictionary<LibVersionObj, string> dic,
        LibVersionObj key, string value)
    {
        foreach (var item in dic)
        {
            if (item.Key.Equals(key))
            {
                dic.Remove(item.Key);
                break;
            }
        }

        dic.Add(key, value);
    }
}
