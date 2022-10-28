using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core;

public enum ArchEnum
{
    x32,
    x64
}

public enum OsType
{
    Windows,
    Linux,
    MacOS
}

public static class SystemInfo
{
    public static OsType Os { get; private set; }
    public static ArchEnum SystemArch { get; private set; }
    public static string SystemName { get; private set; }
    public static int ProcessorCount { get; private set; }

    public static void Init()
    {
        if (Environment.Is64BitOperatingSystem)
        {
            SystemArch = ArchEnum.x64;
        }
        else
        {
            SystemArch = ArchEnum.x32;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Os = OsType.Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Os = OsType.Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Os = OsType.MacOS;
        }

        SystemName = RuntimeInformation.OSDescription;
        ProcessorCount = Environment.ProcessorCount;
    }
}
