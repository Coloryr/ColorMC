using System;
using System.IO;

namespace ColorMC.Gui.Hook;

internal static class Linux
{
    /// <summary>
    /// 获取内存大小
    /// </summary>
    /// <returns></returns>
    public static ulong GetTotalMemory()
    {
        const string memInfoPath = "/proc/meminfo";
        if (File.Exists(memInfoPath))
        {
            var lines = File.ReadAllLines(memInfoPath);
            foreach (var line in lines)
            {
                if (line.StartsWith("MemTotal:"))
                {
                    var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && ulong.TryParse(parts[1], out ulong memKb))
                    {
                        return memKb;
                    }
                }
            }
        }

        return 0;
    }

    /// <summary>
    /// 获取剩余内存大小
    /// </summary>
    /// <returns></returns>
    public static ulong GetFreeMemory()
    {
        const string memInfoPath = "/proc/meminfo";
        if (File.Exists(memInfoPath))
        {
            var lines = File.ReadAllLines(memInfoPath);
            foreach (var line in lines)
            {
                if (line.StartsWith("MemFree:"))
                {
                    var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && ulong.TryParse(parts[1], out ulong memKb))
                    {
                        return memKb;
                    }
                }
            }
        }

        return 0;
    }
}
