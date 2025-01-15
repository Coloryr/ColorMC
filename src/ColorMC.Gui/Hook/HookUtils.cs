using System.Diagnostics;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.Hook;

public static class HookUtils
{
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
            return Macos.GetTotalMemory() / 1024;
        }

        return 0;
    }

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
            return Macos.GetFreeMemory() / 1024;
        }

        return 0;
    }

    public static void WaitWindowDisplay(Process pr)
    {
        if (SystemInfo.Os == OsType.Windows)
        {
            Win32.WaitWindowDisplay(pr);
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            X11Native.WaitWindowDisplay(pr);
        }
    }

    public static void SetTitle(Process pr, string title)
    {
        if (SystemInfo.Os == OsType.Windows)
        {
            Win32.SetTitle(pr, title);
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            X11Native.SetTitle(pr, title);
        }
    }
}
