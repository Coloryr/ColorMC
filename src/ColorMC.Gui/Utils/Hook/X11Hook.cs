using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using X11;

namespace ColorMC.Gui.Utils.Hook;

internal partial class X11Hook
{
    // X11 相关的常量和函数声明
    public const int PropModeReplace = 0;

    public const int ClientMessage = 33;
    public const long NoEventMask = 0L;
    public const long SubstructureRedirectMask = 0x00000001L;
    public const long SubstructureNotifyMask = 0x00000002L;
    public const long StructureNotifyMask = 0x00000004L;

    public const ulong MWM_HINTS_DECORATIONS = 1L << 1;

    public const int KeyPress = 2;
    public const int KeyRelease = 3;

    public const uint MouseLeft = 1;
    public const uint MouseMid = 2;
    public const uint MouseRight = 3;
    public const uint MouseUp = 4;
    public const uint MouseDown = 5;
    public const uint MouseX1 = 8;
    public const uint MouseX2 = 9;

    [StructLayout(LayoutKind.Sequential)]
    public struct XWMHints
    {
        public long flags;
        public bool input;
        public int initialState;
        public IntPtr iconPixmap;
        public IntPtr iconWindow;
        public int iconX;
        public int iconY;
        public IntPtr iconMask;
        public IntPtr windowGroup;
    }

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct XRectangle
    {
        public short x;
        public short y;
        public short width;
        public short height;
    }

    public enum ShapeKind : int
    {
        ShapeBounding = 0,
        ShapeClip = 1,
        ShapeInput = 2
    }

    public enum ShapeOp : int
    {
        ShapeSet = 0,
        ShapeUnion = 1,
        ShapeIntersect = 2,
        ShapeSubtract = 3,
        ShapeInvert = 4
    }

    [LibraryImport("libXext")]
    public static partial void XShapeCombineRectangles(IntPtr display, IntPtr window,
           ShapeKind kind, int xOff, int yOff, ref XRectangle rectangles,
           int n_rects, ShapeOp op, int ordering);

    [LibraryImport("libX11", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int XGetWindowProperty(IntPtr display, IntPtr w, Atom property, long long_offset,
        long long_length, [MarshalAs(UnmanagedType.Bool)] bool delete, Atom req_type, out Atom actual_type_return,
        out int actual_format_return, out long nitems_return, out long bytes_after_return, out IntPtr prop_return);

    internal static bool WaitWindowDisplay(Process pr)
    {
        IntPtr display = Xlib.XOpenDisplay(null!);
        if (display == IntPtr.Zero)
        {
            return false;
        }

        bool windowFound = false;
        var atom = Xlib.XInternAtom(display, "_NET_WM_PID", false);
        while (!windowFound && !pr.HasExited)
        {
            if (FindWindow(display, atom, pr.Id) == IntPtr.Zero)
            {
                Thread.Sleep(500);
            }
            else
            {
                windowFound = true;
                break;
            }
        }

        Xlib.XCloseDisplay(display);

        return windowFound;
    }

    internal static void SetTitle(Process pr, string title)
    {
        IntPtr display = Xlib.XOpenDisplay(null!);
        if (display == IntPtr.Zero)
        {
            return;
        }

        var atom = Xlib.XInternAtom(display, "_NET_WM_PID", false);
        var window = FindWindow(display, atom, pr.Id);
        if (window != IntPtr.Zero)
        {
            Xlib.XStoreName(display, window, title);

            Atom _NET_WM_NAME = Xlib.XInternAtom(display, "_NET_WM_NAME", false);
            Atom UTF8_STRING = Xlib.XInternAtom(display, "UTF8_STRING", false);

            var temp = Encoding.UTF8.GetBytes(title);

            GCHandle handle = GCHandle.Alloc(temp, GCHandleType.Pinned);
            try
            {
                IntPtr pointer = handle.AddrOfPinnedObject();
                Xlib.XChangeProperty(display, window, _NET_WM_NAME, UTF8_STRING, 8,
                           (int)PropertyMode.Replace, pointer, temp.Length);
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free(); // 一定要释放句柄
            }
        }
        Xlib.XCloseDisplay(display);
    }

    private static List<IntPtr>? GetWindows(IntPtr display, IntPtr window)
    {
        // 获取根窗口的所有子窗口
        IntPtr returnedRoot = IntPtr.Zero, returnedParent = IntPtr.Zero;
        if (Xlib.XQueryTree(display, window, ref returnedRoot, ref returnedParent, out var childWindows) == 0)
        {
            return null;
        }

        return childWindows;
    }

    private static IntPtr FindWindow(IntPtr display, Atom atom, int pid)
    {
        // 获取根窗口的 ID
        IntPtr rootWindow = Xlib.XDefaultRootWindow(display);

        return FindWindow(display, rootWindow, atom, pid);
    }

    private static IntPtr FindWindow(IntPtr display, IntPtr windows, Atom atom, int pid)
    {
        var list = GetWindows(display, windows);
        if (list == null)
        {
            return IntPtr.Zero;
        }
        foreach (var item in list)
        {
            // 尝试获取 _NET_WM_PID 属性
            if (XGetWindowProperty(display, item, atom, 0, 1, false, Atom.None, out Atom actualType,
                out int actualFormat, out long nitems, out long bytesAfter, out nint propPid) == 0 && propPid != IntPtr.Zero)
            {
                // 检查获取到的 PID 是否与我们的 PID 匹配
                if (nitems == 1)
                {
                    int windowPid = Marshal.ReadInt32(propPid);
                    Xlib.XFree(propPid);

                    if (windowPid == pid)
                    {
                        return item;
                    }
                }
            }

            var child = FindWindow(display, item, atom, pid);
            if (child != IntPtr.Zero)
            {
                return child;
            }
        }

        return IntPtr.Zero;
    }
}
