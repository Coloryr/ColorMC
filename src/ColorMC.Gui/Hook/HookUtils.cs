using System;
using System.IO;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.Hook;

/// <summary>
/// 系统平台相关
/// </summary>
public static class HookUtils
{
    /// <summary>
    /// 获取内存大小
    /// </summary>
    /// <returns>内存大小MB</returns>
    public static ulong GetMemorySize()
    {
        if (SystemInfo.Os == OsType.Windows)
        {
            return Win32.GetTotalMemory() / 1024 / 1024;
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            return Linux.GetTotalMemory() / 1024;
        }
        else if (SystemInfo.Os == OsType.MacOS)
        {
            return Macos.GetTotalMemory() / 1024 / 1024;
        }

        return 0;
    }

    /// <summary>
    /// 获取剩余内存大小
    /// </summary>
    /// <returns>内存大小MB</returns>
    public static ulong GetFreeSize()
    {
        if (SystemInfo.Os == OsType.Windows)
        {
            return Win32.GetFreeMemory() / 1024 / 1024;
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            return Linux.GetFreeMemory() / 1024;
        }
        else if (SystemInfo.Os == OsType.MacOS)
        {
            return Macos.GetFreeMemory() / 1024 / 1024;
        }

        return 0;
    }

    ///// <summary>
    ///// 等待窗口就绪
    ///// </summary>
    ///// <param name="pr">游戏进程</param>
    //public static void WaitWindowDisplay(Process pr)
    //{
    //    if (SystemInfo.Os == OsType.Windows)
    //    {
    //        Win32.WaitWindowDisplay(pr);
    //    }
    //    else if (SystemInfo.Os == OsType.Linux)
    //    {
    //        X11Hook.WaitWindowDisplay(pr);
    //    }
    //}

    ///// <summary>
    ///// 设置窗口标题
    ///// </summary>
    ///// <param name="pr">游戏进程</param>
    ///// <param name="title">需要设置的标题</param>
    //public static void SetTitle(Process pr, string title)
    //{
    //    if (SystemInfo.Os == OsType.Windows)
    //    {
    //        Win32.SetTitle(pr, title);
    //    }
    //    else if (SystemInfo.Os == OsType.Linux)
    //    {
    //        X11Hook.SetTitle(pr, title);
    //    }
    //}

    public static void RegisterFastLaunch(bool mod)
    {
        if (SystemInfo.Os == OsType.Windows)
        {
            Win32.RegisterProtocolHandler(mod);
        }
    }

    public static void DeleteFastLaunch()
    {
        if (SystemInfo.Os == OsType.Windows)
        {
            Win32.DeleteProtocolHandler();
        }
    }

    /// <summary>
    /// 创建快捷方式
    /// </summary>
    /// <param name="obj"></param>
    public static void CreateLaunch(GameSettingObj obj)
    {
#pragma warning disable CA1416 // 验证平台兼容性
        if (SystemInfo.Os == OsType.Windows)
        {
            Win32.CreateLaunch(obj);
        }
#pragma warning restore CA1416 // 验证平台兼容性
    }
}
