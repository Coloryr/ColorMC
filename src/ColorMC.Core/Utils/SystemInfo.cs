using ColorMC.Core.Objs;
using System.Globalization;
using System.Runtime.InteropServices;

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
    public static OsType Os { get; private set; } = OsType.Windows;
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
    public static bool IsArm { get; private set; }
    /// <summary>
    /// 是否为64位处理器
    /// </summary>
    public static bool Is64Bit { get; private set; }

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

        Logs.Info(System);
        Logs.Info(SystemName);
    }
}
