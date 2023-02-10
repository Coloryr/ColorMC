using System.Runtime.InteropServices;

namespace ColorMC.Core.Utils;

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
    public static string System { get; private set; }
    public static int ProcessorCount { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        Logs.Info(LanguageHelper.GetName("Core.System.Info1"));
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
        System = $"Os:{Os} Arch:{SystemArch}";

        Logs.Info(System);
        Logs.Info(SystemName);
    }
}
