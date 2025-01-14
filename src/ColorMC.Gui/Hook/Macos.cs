using System;
using System.Diagnostics;

namespace ColorMC.Gui.Hook;

internal static class Macos
{
    public static ulong GetTotalMemory()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sysctl",
                Arguments = "hw.memsize",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (output.StartsWith("hw.memsize:"))
        {
            var parts = output.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && ulong.TryParse(parts[1], out ulong memBytes))
            {
                return memBytes;
            }
        }

        return 0;
    }

    public static ulong GetFreeMemory()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "vm_stat",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        var lines = output.Split('\n');
        ulong freePages = 0;
        ulong pageSize = 4096;

        foreach (var line in lines)
        {
            if (line.StartsWith("Pages free:"))
            {
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 && ulong.TryParse(parts[2].TrimEnd('.'), out ulong pages))
                {
                    freePages = pages;
                }
            }
            else if (line.StartsWith("page size of"))
            {
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4 && ulong.TryParse(parts[3], out ulong size))
                {
                    pageSize = size;
                }
            }
        }

        if (freePages > 0)
        {
            return freePages * pageSize;
        }

        return 0;
    }
}
