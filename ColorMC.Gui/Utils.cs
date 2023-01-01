using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
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

    public static void MakeExpanderTran(this Expander expander)
    {
        try
        {
            var border = expander.GetVisualChildren();
            foreach (var item in border)
            {
                if (item is DockPanel)
                {
                    foreach (var item1 in item.VisualChildren)
                    {
                        if (item1 is Border)
                        {
                            (item1 as Border).Background = Avalonia.Media.Brush.Parse("#88f2f2f2");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        { 
            
        }
    }
}

public static class HeadImageUtils
{
    public static async Task<string> MakeHeadImage(string file)
    {
        using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(file);
        using var image1 = new Image<Rgba32>(8, 8);
        using var image2 = new Image<Rgba32>(64, 64);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                image1[i, j] = image[i + 8, j + 8];
            }
        }

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                image1[i, j] = Mix(image1[i, j], image[i + 40, j + 8]);
            }
        }

        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                image2[i, j] = image1[i / 8, j / 8];
            }
        }

        var file1 = Path.GetFileName(AppContext.BaseDirectory + "test.png");
        await image2.SaveAsPngAsync(file1);
        return file1;
    }

    public static Rgba32 Mix(Rgba32 rgba, Rgba32 mix)
    {
        double ap = (double)(mix.A / 255);
        double dp = 1 - ap;

        rgba.R = (byte)(mix.R * ap + rgba.R * dp);
        rgba.G = (byte)(mix.G * ap + rgba.G * dp);
        rgba.B = (byte)(mix.B * ap + rgba.B * dp);

        return rgba;
    }
}