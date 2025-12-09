using ColorMC.Core.Objs;
using SharpCompress.Archives.Zip;
using SharpCompress.Common.Zip;

namespace ColorMC.Core.Utils;

/// <summary>
/// 其他函数
/// </summary>
public static class FunctionUtils
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
    /// 通过MavenId获取文件路径
    /// </summary>
    /// <param name="input">MavenId</param>
    /// <returns>路径</returns>
    public static string VersionNameToFile(string input)
    {
        var arg = input.Split(':');
        if (arg.Length > 3)
        {
            return $"{arg[1]}-{arg[2]}-{arg[3]}.jar";
        }
        else if (arg.Length > 2)
        {
            return $"{arg[1]}-{arg[2]}.jar";
        }
        else
        {
            return arg[0];
        }
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

    /// <summary>
    /// 检查文本是否包含乱码字符
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool HasGarbledText(string text)
    {
        if (text.Contains('?') || text.Contains('�') ||
            text.Contains("ï»¿") || text.Contains('Ã') || text.Contains('Â'))
        {
            return true;
        }

        foreach (var c in text)
        {
            if (char.IsControl(c) && c != '\t' && c != '\r' && c != '\n' && c != ' ')
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断压缩里面是为可解压文件
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public static bool IsFile(ZipEntry entry)
    {
        return entry.Key != null && !entry.IsDirectory && (entry.Size != 0 || entry.CompressedSize != 0);
    }

    /// <summary>
    /// 获取压缩包内的文件
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static ZipArchiveEntry? GetEntry(this ZipArchive archive, string name)
    {
        return archive.Entries.FirstOrDefault(item => item.Key == name);
    }
}
