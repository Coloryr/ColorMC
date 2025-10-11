using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ColorMC.Gui.Hook;

internal static class Macos
{
    // 模拟键盘事件
    [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
    private static extern void CGEventPost(int tap, IntPtr eventRef);

    [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
    private static extern IntPtr CGEventCreateKeyboardEvent(IntPtr source, ushort keyCode, bool keyDown);

    [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
    private static extern void CFRelease(IntPtr obj);

    // 模拟鼠标事件
    [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
    private static extern IntPtr CGEventCreateMouseEvent(IntPtr source, int mouseType, CGPoint point, int mouseButton);

    [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
    private static extern void CGEventSetIntegerValueField(IntPtr eventRef, int field, long value);

    [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
    private static extern CGPoint CGEventGetLocation(IntPtr eventRef);

    // 定义事件类型和结构体
    private enum CGEventType : uint
    {
        LeftMouseDown = 1,
        LeftMouseUp = 2,
        RightMouseDown = 3,
        RightMouseUp = 4,
        MouseMoved = 5,
        LeftMouseDragged = 6,
        RightMouseDragged = 7,
        ScrollWheel = 22,
        KeyDown = 10,
        KeyUp = 11
    }

    private enum CGMouseButton : uint
    {
        Left = 0,
        Right = 1,
        Center = 2
    }

    private enum CGEventTapLocation : uint
    {
        HID = 0,
        Session = 1,
        AnnotatedSession = 2
    }

    private enum CGEventField : int
    {
        DeltaX = 3,  // 对应 kCGMouseEventDeltaX
        DeltaY = 4   // 对应 kCGMouseEventDeltaY
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;
        public CGPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// 获取所有内存大小
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 获取剩余内存大小
    /// </summary>
    /// <returns></returns>
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
