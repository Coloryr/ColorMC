using Silk.NET.Core;
using System;
using System.Runtime.InteropServices;
using X11;

namespace ColorMC.Gui.Utils.Hook;

internal partial class X11
{
    // X11 相关的常量和函数声明
    public const int PropModeReplace = 0;

    public const int ClientMessage = 33;
    public const long NoEventMask = 0L;
    public const long SubstructureRedirectMask = 0x00000001L;
    public const long SubstructureNotifyMask = 0x00000002L;
    public const long StructureNotifyMask = 0x00000004L;

    public const ulong MWM_HINTS_DECORATIONS = (1L << 1);

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
    public static partial Atom XInternAtom(IntPtr display, string name, [MarshalAs(UnmanagedType.Bool)] bool only_if_exists);

    [LibraryImport("libX11", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int XGetWindowProperty(IntPtr display, IntPtr w,  Atom property, long long_offset, 
        long long_length, [MarshalAs(UnmanagedType.Bool)] bool delete, Atom req_type, out Atom actual_type_return,
        out int actual_format_return, out long nitems_return, out long bytes_after_return, out IntPtr prop_return);
}
