using Avalonia.Controls;
using ColorMC.Gui.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui;

public static class XLib
{
    public enum PropertyMode
    {
        Replace = 0,
        Prepend = 1,
        Append = 2
    }

    [Flags]
    public enum MotifFlags
    {
        Functions = 1,
        Decorations = 2,
        InputMode = 4,
        Status = 8
    }

    [Flags]
    public enum MotifFunctions
    {
        All = 0x01,
        Resize = 0x02,
        Move = 0x04,
        Minimize = 0x08,
        Maximize = 0x10,
        Close = 0x20
    }

    [Flags]
    public enum MotifDecorations
    {
        All = 0x01,
        Border = 0x02,
        ResizeH = 0x04,
        Title = 0x08,
        Menu = 0x10,
        Minimize = 0x20,
        Maximize = 0x40,
    }

    [Flags]
    public enum MotifInputMode
    {
        Modeless = 0,
        ApplicationModal = 1,
        SystemModal = 2,
        FullApplicationModal = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MotifWmHints
    {
        internal IntPtr flags;
        internal IntPtr functions;
        internal IntPtr decorations;
        internal IntPtr input_mode;
        internal IntPtr status;

        public override string ToString()
        {
            return string.Format("MotifWmHints <flags={0}, functions={1}, decorations={2}, input_mode={3}, status={4}", (MotifFlags)flags.ToInt32(), (MotifFunctions)functions.ToInt32(), (MotifDecorations)decorations.ToInt32(), (MotifInputMode)input_mode.ToInt32(), status.ToInt32());
        }
    }

    const string libX11 = "libX11.so.6";

    [DllImport(libX11)]
    public static extern IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

    [DllImport(libX11)]
    public static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport(libX11)]
    public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
            int format, PropertyMode mode, ref MotifWmHints data, int nelements);
}

public static class UIUtils
{
    public static void MakeItNoChrome(this Window window)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var type = window.PlatformImpl.GetType();
            if (type == null)
                return;
            if (type.FullName != "Avalonia.X11.X11Window")
                return;
            var field1 = type.GetField("_platform", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field1 == null)
                return;
            var data1 = field1.GetValue(window.PlatformImpl);
            if (data1 == null)
                return;
            var type1 = data1.GetType();
            if (type1 == null)
                return;
            var property1 = type1.GetProperty("Display", BindingFlags.Instance | BindingFlags.Public);
            if (property1 == null)
                return;
            var temp = (IntPtr)property1.GetValue(data1);

            var temp1 = XLib.XInternAtom(temp, "_MOTIF_WM_HINTS", false);

            var hints = new XLib.MotifWmHints
            {
                flags = 2,
                decorations = 0,
                functions = 0,
                input_mode = 0,
                status = 0
            };

            _ = XLib.XChangeProperty(temp, window.PlatformImpl.Handle.Handle, temp1, temp1, 32, XLib.PropertyMode.Replace, ref hints, 5);
        }
    }
}
