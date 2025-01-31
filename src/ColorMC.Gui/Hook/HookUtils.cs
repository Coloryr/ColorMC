using System.Diagnostics;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.Hook;

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

    /// <summary>
    /// 等待窗口就绪
    /// </summary>
    /// <param name="pr">游戏进程</param>
    public static void WaitWindowDisplay(Process pr)
    {
        if (SystemInfo.Os == OsType.Windows)
        {
            Win32.WaitWindowDisplay(pr);
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            X11Hook.WaitWindowDisplay(pr);
        }
    }

    /// <summary>
    /// 设置窗口标题
    /// </summary>
    /// <param name="pr">游戏进程</param>
    /// <param name="title">需要设置的标题</param>
    public static void SetTitle(Process pr, string title)
    {
        if (SystemInfo.Os == OsType.Windows)
        {
            Win32.SetTitle(pr, title);
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            X11Hook.SetTitle(pr, title);
        }
    }
}
