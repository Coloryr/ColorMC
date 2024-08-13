using System.Globalization;
using System.Runtime.InteropServices;
using ColorMC.Core.Objs;

namespace ColorMC.Core.Utils;

/// <summary>
/// 系统信息
/// </summary>
public static class SystemInfo
{
    /// <summary>
    /// 当前语言
    /// </summary>
    public static CultureInfo CultureInfo { get; private set; } = CultureInfo.InstalledUICulture;
    /// <summary>
    /// 系统类型
    /// </summary>
#if DEBUG
    public static OsType Os { get; set; } = OsType.Windows;
#else
    public static OsType Os { get; private set; } = OsType.Windows;
#endif
    /// <summary>
    /// 系统进制
    /// </summary>
    public static ArchEnum SystemArch { get; private set; } = ArchEnum.x86_64;
    /// <summary>
    /// 系统名
    /// </summary>
    public static string SystemName { get; private set; }
    /// <summary>
    /// 系统名
    /// </summary>
    public static string System { get; private set; }
    /// <summary>
    /// 是否为Arm处理器
    /// </summary>
#if DEBUG
    public static bool IsArm { get; set; }
#else
    public static bool IsArm { get; private set; }
#endif
    /// <summary>
    /// 是否为64位处理器
    /// </summary>
    public static bool Is64Bit { get; private set; }
    /// <summary>
    /// 是否为Windows11
    /// </summary>
    public static bool IsWin11 { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        IsArm = RuntimeInformation.OSArchitecture == Architecture.Arm ||
                RuntimeInformation.OSArchitecture == Architecture.Arm64;

        if (Is64Bit = Environment.Is64BitOperatingSystem)
        {
            if (IsArm)
            {
                SystemArch = ArchEnum.aarch64;
            }
            else
            {
                SystemArch = ArchEnum.x86_64;
            }
        }
        else
        {
            if (IsArm)
            {
                SystemArch = ArchEnum.arm;
            }
            else
            {
                SystemArch = ArchEnum.x86;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Os = OsType.Windows;

            var os = Environment.OSVersion;
            var version = os.Version;

            if (version.Major > 10 || (version.Major == 10 && version.Build >= 22000))
            {
                IsWin11 = true;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Os = OsType.Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Os = OsType.MacOS;
        }

        if (OperatingSystem.IsAndroid())
        {
            Os = OsType.Android;
        }

        SystemName = RuntimeInformation.OSDescription;
        System = $"Os:{Os} Arch:{SystemArch}";
    }
}
