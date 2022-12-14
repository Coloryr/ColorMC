using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
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

    public static T? FindToEnd<T>(this IVisual visual)
    {
        foreach (var item in visual.VisualChildren)
        {
            if (item is T t)
            {
                return t;
            }
        }

        foreach (var item in visual.VisualChildren)
        {
            return FindToEnd<T>(item);
        }

        return default;
    }

    public static void MakeTran(this Expander expander)
    {
        try
        {
            var item1 = expander.FindToEnd<Border>();
            if (item1 != null)
            {
                item1.Background = Avalonia.Media.Brush.Parse("#88f2f2f2");
            }
        }
        catch
        {

        }
    }

    public static void MakeTran(this DataGrid grid)
    {
        try
        {
            var item1 = grid.FindToEnd<DataGridColumnHeadersPresenter>();
            if (item1 != null)
            {
                item1.Background = Avalonia.Media.Brush.Parse("#ccffffff");
                foreach (var item in item1.GetVisualChildren())
                {
                    var item2 = item.FindToEnd<TextBlock>();
                    if (item2 != null)
                    {
                        item2.Foreground = Avalonia.Media.Brushes.Black;
                    }
                }
            }
        }
        catch
        {

        }
    }

    public static void MakePadingNull(this Expander expander)
    {
        try
        {
            var item = expander.FindToEnd<Border>();
            if (item != null)
            {
                item.Background = Avalonia.Media.Brushes.Transparent;
                item.Padding = new Thickness(0);
                item.BorderBrush = Avalonia.Media.Brushes.Transparent;
            }
        }
        catch
        {

        }
    }

    public static void MakeThumb(this Slider slider)
    {
        try
        {
            var item = slider.FindToEnd<Thumb>();
            if (item != null)
            {
                var item1 = item.FindToEnd<Border>();
                if (item1 == null)
                    return;
                item1.Background = Avalonia.Media.Brush.Parse("#FF5ABED6");
                item1.BorderThickness = new Thickness(2);
                item1.BorderBrush = Avalonia.Media.Brush.Parse("#88FFFFFF");
            }
        }
        catch
        {

        }
    }
}

public static class ImageUtils
{
    public static async Task<MemoryStream> MakeHeadImage(string file)
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

        MemoryStream stream = new();
        await image2.SaveAsPngAsync(stream);
        return stream;
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

    public static Task<Bitmap?> MakeImageSharp(string file, int value)
    {
        return Task.Run(async () =>
        {
            try
            {
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(file);
                if (value > 0)
                {
                    image.Mutate(p =>
                    {
                        p.GaussianBlur(value);
                    });
                }

                using var stream = new MemoryStream();
                await image.SaveAsPngAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                return new Bitmap(stream);
            }
            catch (Exception e)
            {
                Logs.Error("背景图片加载失败", e);
                return null;
            }
        });
    }
}

public static class GuiConfigUtils
{
    public static GuiConfigObj Config { get; set; }

    public static string Dir;

    private static string Name;

    public static void Init(string dir)
    {
        Dir = dir;
        Name = dir + "gui.json";

        Load(Name);
    }

    public static bool Load(string name, bool quit = false)
    {
        Logs.Info($"正在读取配置文件");
        if (File.Exists(name))
        {
            try
            {
                Config = JsonConvert.DeserializeObject<GuiConfigObj>(File.ReadAllText(name))!;
            }
            catch (Exception e)
            {
                CoreMain.OnError?.Invoke("配置文件读取错误", e, true);
                Logs.Error("配置文件读取错误", e);
            }
        }

        if (Config == null)
        {
            if (quit)
            {
                return false;
            }
            Logs.Warn("配置为空，旧版配置文件会被覆盖");

            Config = MakeDefaultConfig();
            Save();
        }
        else
        {

        }

        Save();

        return true;
    }

    public static void Save()
    {
        Logs.Info($"正在保存配置文件");
        File.WriteAllText(Name, JsonConvert.SerializeObject(Config, Formatting.Indented));
    }

    private static GuiConfigObj MakeDefaultConfig()
    {
        return new()
        {
            Version = CoreMain.Version,
        };
    }
}