using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Skin;
using ColorMC.Gui.Utils.LaunchSetting;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ColorMC.Gui;

//public static class XLib
//{
//    public enum PropertyMode
//    {
//        Replace = 0,
//        Prepend = 1,
//        Append = 2
//    }

//    [Flags]
//    public enum MotifFlags
//    {
//        Functions = 1,
//        Decorations = 2,
//        InputMode = 4,
//        Status = 8
//    }

//    [Flags]
//    public enum MotifFunctions
//    {
//        All = 0x01,
//        Resize = 0x02,
//        Move = 0x04,
//        Minimize = 0x08,
//        Maximize = 0x10,
//        Close = 0x20
//    }

//    [Flags]
//    public enum MotifDecorations
//    {
//        All = 0x01,
//        Border = 0x02,
//        ResizeH = 0x04,
//        Title = 0x08,
//        Menu = 0x10,
//        Minimize = 0x20,
//        Maximize = 0x40,
//    }

//    [Flags]
//    public enum MotifInputMode
//    {
//        Modeless = 0,
//        ApplicationModal = 1,
//        SystemModal = 2,
//        FullApplicationModal = 3
//    }

//    [StructLayout(LayoutKind.Sequential)]
//    public struct MotifWmHints
//    {
//        internal IntPtr flags;
//        internal IntPtr functions;
//        internal IntPtr decorations;
//        internal IntPtr input_mode;
//        internal IntPtr status;
//    }

//    const string libX11 = "libX11.so.6";

//    [DllImport(libX11)]
//    public static extern IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

//    [DllImport(libX11)]
//    public static extern IntPtr XOpenDisplay(IntPtr display);

//    [DllImport(libX11)]
//    public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
//            int format, PropertyMode mode, ref MotifWmHints data, int nelements);
//}

public static class OtherUtils
{
    public static DateTime TimestampToDataTime(long unixTimeStamp)
    {
        DateTime start = new DateTime(1970, 1, 1) +
            TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        DateTime dt = start.AddMilliseconds(unixTimeStamp);
        return dt;
    }

    public static string GetName(this SkinType type)
    {
        return type switch
        {
            SkinType.Old => Localizer.Instance["SkinType.Old"],
            SkinType.New => Localizer.Instance["SkinType.New"],
            SkinType.New_Slim => Localizer.Instance["SkinType.New_Slim"],
            _ => Localizer.Instance["SkinType.Other"]
        };
    }
}

public static class SkinUtil
{
    public static SkinType GetTextType(Image<Rgba32> image)
    {
        if (image.Width >= 64 && image.Height >= 64 && image.Width == image.Height)
        {
            if (IsSlimSkin(image))
            {
                return SkinType.New_Slim;
            }
            else
            {
                return SkinType.New;
            }
        }
        else if (image.Width == image.Height * 2)
        {
            return SkinType.Old;
        }
        else
        {
            return SkinType.UNKNOWN;
        }
    }

    public static bool IsSlimSkin(Image<Rgba32> image)
    {
        var scale = image.Width / 64;
        return (Check(image, 50 * scale, 16 * scale, 2 * scale, 4 * scale,
            SixLabors.ImageSharp.Color.Transparent) ||
                Check(image, 54 * scale, 20 * scale, 2 * scale, 12 * scale,
                SixLabors.ImageSharp.Color.Transparent) ||
                Check(image, 42 * scale, 48 * scale, 2 * scale, 4 * scale,
                SixLabors.ImageSharp.Color.Transparent) ||
                Check(image, 46 * scale, 52 * scale, 2 * scale, 12 * scale,
                SixLabors.ImageSharp.Color.Transparent)) ||
                (Check(image, 50 * scale, 16 * scale, 2 * scale, 4 * scale,
                SixLabors.ImageSharp.Color.White) &&
                        Check(image, 54 * scale, 20 * scale, 2 * scale, 12 * scale, 
                        SixLabors.ImageSharp.Color.White) &&
                        Check(image, 42 * scale, 48 * scale, 2 * scale, 4 * scale, 
                        SixLabors.ImageSharp.Color.White) &&
                        Check(image, 46 * scale, 52 * scale, 2 * scale, 12 * scale,
                        SixLabors.ImageSharp.Color.White)) ||
                (Check(image, 50 * scale, 16 * scale, 2 * scale, 4 * scale,
                SixLabors.ImageSharp.Color.Black) &&
                        Check(image, 54 * scale, 20 * scale, 2 * scale, 12 * scale,
                        SixLabors.ImageSharp.Color.Black) &&
                        Check(image, 42 * scale, 48 * scale, 2 * scale, 4 * scale,
                        SixLabors.ImageSharp.Color.Black) &&
                        Check(image, 46 * scale, 52 * scale, 2 * scale, 12 * scale,
                        SixLabors.ImageSharp.Color.Black));
    }

    public static bool Check(Image<Rgba32> image, int x, int y, int w, int h, Rgba32 color)
    {
        for (int wi = 0; wi < w; wi++)
        {
            for (int hi = 0; hi < h; hi++)
            {
                if (image[x + wi, y + hi] != color)
                {
                    return false;
                }
            }
        }

        return true;
    }
}

public static partial class UIUtils
{
    //public static void MakeItNoChrome(this Window window)
    //{
    //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    //    {
    //        var type = window.PlatformImpl.GetType();
    //        if (type == null)
    //            return;
    //        if (type.FullName != "Avalonia.X11.X11Window")
    //            return;
    //        var field1 = type.GetField("_platform", BindingFlags.Instance | BindingFlags.NonPublic);
    //        if (field1 == null)
    //            return;
    //        var data1 = field1.GetValue(window.PlatformImpl);
    //        if (data1 == null)
    //            return;
    //        var type1 = data1.GetType();
    //        if (type1 == null)
    //            return;
    //        var property1 = type1.GetProperty("Display", BindingFlags.Instance | BindingFlags.Public);
    //        if (property1 == null)
    //            return;
    //        var temp = (IntPtr)property1.GetValue(data1);

    //        var temp1 = XLib.XInternAtom(temp, "_MOTIF_WM_HINTS", false);

    //        var hints = new XLib.MotifWmHints
    //        {
    //            flags = 2,
    //            decorations = 0,
    //            functions = 0,
    //            input_mode = 0,
    //            status = 0
    //        };

    //        _ = XLib.XChangeProperty(temp, window.PlatformImpl.Handle.Handle, 
    //            temp1, temp1, 32, XLib.PropertyMode.Replace, ref hints, 5);

    //        window.CanResize = true;
    //    }
    //}

    public static void MakeResizeDrag(this Avalonia.Controls.Shapes.Rectangle rectangle,
        Window window)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            rectangle.PointerPressed += (a, e) =>
            {
                if (e.GetCurrentPoint(rectangle).Properties.IsLeftButtonPressed)
                {
                    var point = e.GetPosition(rectangle);
                    var arg1 = point.X / rectangle.Bounds.Width;
                    var arg2 = point.Y / rectangle.Bounds.Height;
                    if (arg1 > 0.95)
                    {
                        if (arg2 > 0.95)
                        {
                            window.BeginResizeDrag(WindowEdge.SouthEast, e);
                        }
                        else if (arg2 <= 0.95)
                        {
                            window.BeginResizeDrag(WindowEdge.East, e);
                        }
                    }
                    else if (arg1 < 0.05)
                    {
                        if (arg2 <= 0.95)
                        {
                            window.BeginResizeDrag(WindowEdge.West, e);
                        }
                        else if (arg2 > 0.95)
                        {
                            window.BeginResizeDrag(WindowEdge.SouthWest, e);
                        }
                    }
                    else if (arg2 > 0.95)
                    {
                        window.BeginResizeDrag(WindowEdge.South, e);
                    }
                }
            };
        }
    }

    public static T? FindToEnd<T>(this IVisual visual)
    {
        foreach (var item in visual.GetVisualChildren())
        {
            if (item is T t)
            {
                return t;
            }
        }

        foreach (var item in visual.GetVisualChildren())
        {
            var res = FindToEnd<T>(item);
            if (res != null)
            {
                return res;
            }
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
                item1.Bind(Border.BackgroundProperty, new Binding
                {
                    Source = Utils.LaunchSetting.ColorSel.Instance,
                    Path = "[TranBack]"
                });
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
                item1.Background = Brush.Parse("#ccffffff");
                foreach (var item in item1.GetVisualChildren())
                {
                    var item2 = item.FindToEnd<TextBlock>();
                    if (item2 != null)
                    {
                        item2.Foreground = Brushes.Black;
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
                item.Background = Brushes.Transparent;
                item.Padding = new Thickness(0);
                item.BorderBrush = Brushes.Transparent;
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

                item1.Bind(Border.BackgroundProperty, new Binding
                {
                    Source = Utils.LaunchSetting.ColorSel.Instance,
                    Path = "[Main]"
                });

                item1.Bind(Border.BorderBrushProperty, new Binding
                {
                    Source = Utils.LaunchSetting.ColorSel.Instance,
                    Path = "[TranBack]"
                });

                item1.BorderThickness = new Thickness(2);
            }
        }
        catch
        {

        }
    }

    public static bool CheckNotNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;
        return Regex1().IsMatch(input);
    }

    [GeneratedRegex("[^0-9]+")]
    private static partial Regex Regex1();

    public static string Make(this List<string> strings)
    {
        string temp = "";
        foreach (var item in strings)
        {
            temp += item + ",";
        }

        if (temp.Length > 0)
        {
            return temp[..^1];
        }

        return temp;
    }

    public static string MakeFileSize(long size)
    {
        if (size > 1000000)
        {
            return $"{(double)size / 1000000:#.000}Mb/s";
        }
        else if (size > 1000)
        {
            return $"{(double)size / 1000:#.000}Kb/s";
        }
        else
        {
            return $"{size}b/s";
        }
    }

    public static string MakeFileSize1(long size)
    {
        if (size > 1000000)
        {
            return $"{(double)size / 1000000:#.000}MB";
        }
        else if (size > 1000)
        {
            return $"{(double)size / 1000:#.000}KB";
        }
        else
        {
            return $"{size}N";
        }
    }

    public static Avalonia.Media.Color ToColor(this IBrush brush)
    {
        if (brush is ImmutableSolidColorBrush brush1)
        {
            return brush1.Color;
        }

        return new(255, 255, 255, 255);
    }

    public static (double X, double Y) GetXY(this IVisual visual)
    {
        var temp = (visual.Bounds.X, visual.Bounds.Y);
        if (visual.GetVisualParent() != null)
        {
            var (X, Y) = GetXY(visual.GetVisualParent());
            temp.X += X;
            temp.Y += Y;
        }

        return temp;
    }

    public static void BindFont(this Window window)
    {
        window.Bind(TemplatedControl.FontFamilyProperty, new Binding
        {
            Source = FontSel.Instance,
            Path = "[Font]"
        });
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
        await image2.SaveAsBmpAsync(stream);
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

    public static (int X, int Y) GetMaxSize()
    {
        int x = 0, y = 0;
        foreach (var item in App.MainWindow!.Screens.All)
        {
            x = x < item.Bounds.Width ? item.Bounds.Width : x;
            y = y < item.Bounds.Height ? item.Bounds.Height : y;
        }

        return (x, y);
    }

    public static Task<Bitmap?> MakeImageSharp(string file, int value)
    {
        return Task.Run(() =>
        {
            try
            {
                var (X, Y) = GetMaxSize();
                using var image = SixLabors.ImageSharp.Image.Load(file);
                if (X != 0 && Y != 0
                && image.Width > X && image.Height > Y)
                {
                    int x = X;
                    int y = (int)(image.Height / ((float)image.Width / X));
                    image.Mutate(p =>
                    {
                        p.Resize(x, y);
                    });
                }

                if (value > 0)
                {
                    image.Mutate(p =>
                    {
                        p.GaussianBlur(value);
                    });
                }

                using var stream = new MemoryStream();
                image.SaveAsPng(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return new Bitmap(stream);
            }
            catch (Exception e)
            {
                Logs.Error(Localizer.Instance["Error1"], e);
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
        Logs.Info(Localizer.Instance["Info1"]);
        if (File.Exists(name))
        {
            try
            {
                Config = JsonConvert.DeserializeObject<GuiConfigObj>(File.ReadAllText(name))!;
            }
            catch (Exception e)
            {
                CoreMain.OnError?.Invoke(Localizer.Instance["Error2"], e, true);
                Logs.Error(Localizer.Instance["Error2"], e);
            }
        }

        if (Config == null)
        {
            if (quit)
            {
                return false;
            }
            Logs.Warn(Localizer.Instance["Warn1"]);

            Config = MakeDefaultConfig();

            Save();
        }
        else if (Config.ServerCustom == null)
        {
            if (quit)
            {
                return false;
            }

            Logs.Warn(Localizer.Instance["Warn1"]);

            Config.ServerCustom = MakeServerCustomConfig();

            Save();
        }

        ColorSel.Instance.Load();

        return true;
    }

    public static void Save()
    {
        Logs.Info(Localizer.Instance["Info2"]);
        try
        {
            File.WriteAllText(Name, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }
        catch (Exception e)
        {
            CoreMain.OnError?.Invoke(Localizer.Instance["Error3"], e, true);
            Logs.Error(Localizer.Instance["Error3"], e);
        }
    }

    private static GuiConfigObj MakeDefaultConfig()
    {
        return new()
        {
            Version = CoreMain.Version,
            ColorMain = "#FF5ABED6",
            ColorBack = "#FFF4F4F5",
            ColorTranBack = "#88FFFFFF",
            RGBS = 100,
            RGBV = 100,
            ColorFont1 = "#FFFFFFFF",
            ColorFont2 = "#FF000000",
            ServerCustom = MakeServerCustomConfig(),
            FontDefault = true
        };
    }

    public static ServerCustom MakeServerCustomConfig()
    {
        return new()
        {
            MotdColor = "#FFFFFFFF",
            MotdBackColor = "#FF000000"
        };
    }
}