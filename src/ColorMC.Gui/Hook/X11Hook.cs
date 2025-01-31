using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ColorMC.Gui.Hook;

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

    public enum Atom : ulong
    {
        None = 0,
        Primary = 1,
        Secondary = 2,
        Arc = 3,
        Atom = 4,
        Bitmap = 5,
        Cardinal = 6,
        Colormap = 7,
        Cursor = 8,
        CutBuffer0 = 9,
        CutBuffer1 = 10,
        CutBuffer2 = 11,
        CutBuffer3 = 12,
        CutBuffer4 = 13,
        CutBuffer5 = 14,
        CutBuffer6 = 15,
        CutBuffer7 = 16,
        Drawable = 17,
        Font = 18,
        Integer = 19,
        Pixmap = 20,
        Point = 21,
        Rectangle = 22,
        ResourceManager = 23,
        RgbColorMap = 24,
        RgbBestMap = 25,
        RgbBlueMap = 26,
        RgbDefaultMap = 27,
        RgbGrayMap = 28,
        RgbGreenMap = 29,
        RgbRedMap = 30,
        String = 31,
        Visualid = 32,
        Window = 33,
        WmCommand = 34,
        WmHints = 35,
        WmClientMachine = 36,
        WmIconName = 37,
        WmIconSize = 38,
        WmName = 39,
        WmNormalHints = 40,
        WmSizeHints = 41,
        WmZoomHints = 42,
        MinSpace = 43,
        NormSpace = 44,
        MaxSpace = 45,
        EndSpace = 46,
        SuperscriptX = 47,
        SuperscriptY = 48,
        SubscriptX = 49,
        SubscriptY = 50,
        UnderlinePosition = 51,
        UnderlineThickness = 52,
        StrikeoutAscent = 53,
        StrikeoutDescent = 54,
        ItalicAngle = 55,
        XHeight = 56,
        QuadWidth = 57,
        Weight = 58,
        PointSize = 59,
        Resolution = 60,
        Copyright = 61,
        Notice = 62,
        FontName = 63,
        FamilyName = 64,
        FullName = 65,
        CapHeight = 66,
        WmClass = 67,
        WmTransientFor = 68,
        LastPredefined = 68
    }

    public enum Status : int
    {
        Failure = 0,
    }

    public enum PropertyMode : int
    {
        Replace = 0,
        Prepend = 1,
        Append = 2
    }

    [LibraryImport("libXext")]
    public static partial void XShapeCombineRectangles(IntPtr display, IntPtr window,
           ShapeKind kind, int xOff, int yOff, ref XRectangle rectangles,
           int n_rects, ShapeOp op, int ordering);

    [LibraryImport("libX11", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int XGetWindowProperty(IntPtr display, IntPtr w, Atom property, long long_offset,
        long long_length, [MarshalAs(UnmanagedType.Bool)] bool delete, Atom req_type, out Atom actual_type_return,
        out int actual_format_return, out long nitems_return, out long bytes_after_return, out IntPtr prop_return);


    [LibraryImport("libX11", StringMarshalling = StringMarshalling.Utf8)]
    public static partial Atom XInternAtom(IntPtr display, string name, [MarshalAs(UnmanagedType.Bool)] bool only_if_exists);

    /// <summary>
    /// Initiate a connection to the name X session.
    /// (or respect the DISPLAY environment variable if the display parameter is null).
    /// </summary>
    /// <param name="display">X session connection string in format hostname:number.screen_number</param>
    /// <returns></returns>
    [LibraryImport("libX11", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr XOpenDisplay(string display);

    /// <summary>
    /// Free an unmanaged display pointer (created with XOpenDisplay)
    /// </summary>
    /// <param name="display">Display pointer to free</param>
    /// <returns>Zero on failure</returns>
    [LibraryImport("libX11")]
    public static partial Status XCloseDisplay(IntPtr display);

    [LibraryImport("libX11", StringMarshalling = StringMarshalling.Utf8)]
    public static partial Status XStoreName(IntPtr display, IntPtr window, string window_name);

    [LibraryImport("libX11")]
    public static partial int XChangeProperty(IntPtr display, IntPtr window, Atom property, Atom type, int format, int mode, IntPtr data, int nelements);

    [LibraryImport("libX11")]
    private static partial int XQueryTree(IntPtr display, IntPtr window, ref IntPtr WinRootReturn,
        ref IntPtr WinParentReturn, ref IntPtr ChildrenReturn, ref uint nChildren);

    /// <summary>
    /// Retrieve the Window ID corresponding to the displays root window.
    /// </summary>
    /// <param name="display">Pointer to an open X display</param>
    /// <returns></returns>
    [LibraryImport("libX11")]
    public static partial IntPtr XDefaultRootWindow(IntPtr display);

    [LibraryImport("libX11")]
    public static partial void XFree(IntPtr data);

    internal static bool WaitWindowDisplay(Process pr)
    {
        IntPtr display = XOpenDisplay(null!);
        if (display == IntPtr.Zero)
        {
            return false;
        }

        bool windowFound = false;
        var atom = XInternAtom(display, "_NET_WM_PID", false);
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

        XCloseDisplay(display);

        return windowFound;
    }

    internal static void SetTitle(Process pr, string title)
    {
        IntPtr display = XOpenDisplay(null!);
        if (display == IntPtr.Zero)
        {
            return;
        }

        var atom = XInternAtom(display, "_NET_WM_PID", false);
        var window = FindWindow(display, atom, pr.Id);
        if (window != IntPtr.Zero)
        {
            XStoreName(display, window, title);

            Atom _NET_WM_NAME = XInternAtom(display, "_NET_WM_NAME", false);
            Atom UTF8_STRING = XInternAtom(display, "UTF8_STRING", false);

            var temp = Encoding.UTF8.GetBytes(title);

            GCHandle handle = GCHandle.Alloc(temp, GCHandleType.Pinned);
            try
            {
                IntPtr pointer = handle.AddrOfPinnedObject();
                XChangeProperty(display, window, _NET_WM_NAME, UTF8_STRING, 8,
                           (int)PropertyMode.Replace, pointer, temp.Length);
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free(); // 一定要释放句柄
            }
        }
        XCloseDisplay(display);
    }

    private static List<IntPtr>? GetWindows(IntPtr display, IntPtr window)
    {
        // 获取根窗口的所有子窗口
        IntPtr returnedRoot = IntPtr.Zero, returnedParent = IntPtr.Zero;
        if (XQueryTree(display, window, ref returnedRoot, ref returnedParent, out var childWindows) == 0)
        {
            return null;
        }

        return childWindows;
    }

    private unsafe static int XQueryTree(IntPtr display, IntPtr window, ref IntPtr WinRootReturn,
        ref IntPtr WinParentReturn, out List<IntPtr> ChildrenReturn)
    {
        ChildrenReturn = [];
        IntPtr pChildren = new();
        uint nChildren = 0;

        var r = XQueryTree(display, window, ref WinRootReturn, ref WinParentReturn,
            ref pChildren, ref nChildren);

        for (int i = 0; i < nChildren; i++)
        {
            var ptr = new IntPtr(pChildren.ToInt64() + i * sizeof(IntPtr));
            ChildrenReturn.Add((IntPtr)Marshal.ReadInt64(ptr));
        }

        return r;
    }

    private static IntPtr FindWindow(IntPtr display, Atom atom, int pid)
    {
        // 获取根窗口的 ID
        IntPtr rootWindow = XDefaultRootWindow(display);

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
                    XFree(propPid);

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
