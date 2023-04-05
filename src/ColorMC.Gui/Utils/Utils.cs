using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using ColorMC.Core;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils.LaunchSetting;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui;

public static class OtherUtils
{
    public static string GetName(this SkinType type)
    {
        return type switch
        {
            SkinType.Old => App.GetLanguage("SkinType.Old"),
            SkinType.New => App.GetLanguage("SkinType.New"),
            SkinType.NewSlim => App.GetLanguage("SkinType.New_Slim"),
            _ => App.GetLanguage("SkinType.Other")
        };
    }

    public static string GetName(this FTBType type)
    {
        return type switch
        {
            FTBType.All => App.GetLanguage("FTBType.All"),
            FTBType.Featured => App.GetLanguage("FTBType.Featured"),
            FTBType.Popular => App.GetLanguage("FTBType.Popular"),
            FTBType.Installs => App.GetLanguage("FTBType.Installs"),
            FTBType.Search => App.GetLanguage("FTBType.Search"),
            _ => App.GetLanguage("FTBType.Other")
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
                return SkinType.NewSlim;
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
            return SkinType.Unkonw;
        }
    }

    private static bool IsSlimSkin(Image<Rgba32> image)
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

    private static bool Check(Image<Rgba32> image, int x, int y, int w, int h, Rgba32 color)
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
    public static T? FindToEnd<T>(this Visual visual)
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
            item1?.Bind(Border.BackgroundProperty, new Binding
            {
                Source = ColorSel.Instance,
                Path = "[TranBack]"
            });
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
                item1.Background = Brush.Parse("#CCffffff");
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

    public static string MakeString(this List<string>? strings)
    {
        if (strings == null)
            return "";
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
            return $"{size}";
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

    public static (double X, double Y) GetXY(this Visual? visual)
    {
        if (visual == null)
            return (0, 0);
        var temp = (visual.Bounds.X, visual.Bounds.Y);
        if (visual.GetVisualParent() != null)
        {
            var (X, Y) = GetXY(visual.GetVisualParent());
            temp.X += X;
            temp.Y += Y;
        }

        return temp;
    }

    public static T? FindTop<T>(this Visual visual) where T : Visual
    {
        var pan = visual.GetVisualParent();
        while (pan != null)
        {
            if (pan is T t)
            {
                return t;
            }
            pan = pan.GetVisualParent();
        }

        return default;
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

    private static Rgba32 Mix(Rgba32 rgba, Rgba32 mix)
    {
        double ap = (double)(mix.A / 255);
        double dp = 1 - ap;

        rgba.R = (byte)(mix.R * ap + rgba.R * dp);
        rgba.G = (byte)(mix.G * ap + rgba.G * dp);
        rgba.B = (byte)(mix.B * ap + rgba.B * dp);

        return rgba;
    }


    public static Task<Bitmap?> MakeBackImage(string file, int value, int lim)
    {
        return Task.Run(async () =>
        {
            try
            {
                Stream stream1;
                if (file.StartsWith("https://") || file.StartsWith("http://"))
                {
                    var res = await BaseClient.DownloadClient.GetAsync(file);
                    stream1 = res.Content.ReadAsStream();
                }
                else if (!File.Exists(file))
                {
                    return null;
                }
                else
                {
                    stream1 = File.OpenRead(file);
                }
                if (value > 0 || lim != 100)
                {
                    using var image = SixLabors.ImageSharp.Image.Load(stream1);

                    if (lim != 100)
                    {
                        int x = (int)(image.Width * (float)lim / 100);
                        int y = (int)(image.Height * (float)lim / 100);
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
                else
                {
                    return new Bitmap(stream1);
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("Error1"), e);
                return null;
            }
        });
    }
}

public static class ImageTemp
{
    public static string Local { get; private set; }

    public static void Init(string dir)
    {
        Local = dir + "image/";

        Directory.CreateDirectory(Local);
    }

    public static async Task<Bitmap?> Load(string url)
    {
        if (!Directory.Exists(Local))
        {
            Directory.CreateDirectory(Local);
        }
        var sha1 = Funtcions.GenSha256(url);
        if (File.Exists(Local + sha1))
        {
            return new Bitmap(Local + sha1);
        }
        else
        {
            try
            {
                var data1 = await BaseClient.GetBytes(url);
                using var stream = new MemoryStream(data1);
                var image = Bitmap.DecodeToWidth(stream, 80);
                image.Save(Local + sha1);

                return image;
            }
            catch (Exception e)
            {
                Logs.Error("http error", e);
            }

            return null;
        }
    }
}

public static class GuiConfigUtils
{
    public static GuiConfigObj Config { get; set; }

    private static string Name;

    public static void Init(string dir)
    {
        Name = dir + "gui.json";

        Load(Name);
    }

    public static bool Load(string name, bool quit = false)
    {
        if (File.Exists(name))
        {
            try
            {
                Config = JsonConvert.DeserializeObject<GuiConfigObj>(File.ReadAllText(name))!;
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("Gui.Error17"), e);
            }

            if (Config == null)
            {
                if (quit)
                {
                    return false;
                }

                Config = MakeDefaultConfig();

                SaveNow();
            }

            if (Config.ServerCustom == null)
            {
                if (quit)
                {
                    return false;
                }

                Config.ServerCustom = MakeServerCustomConfig();

                Save();
            }

            if (Config.Render == null
                || Config.Render.Windows == null
                || Config.Render.X11 == null)
            {
                if (quit)
                {
                    return false;
                }

                Config.Render = MakeRenderConfig();

                Save();
            }
        }
        else
        {
            Config = MakeDefaultConfig();

            SaveNow();
        }

        return true;
    }

    public static void SaveNow()
    {
        File.WriteAllText(Name,
                    JsonConvert.SerializeObject(Config, Formatting.Indented));
    }

    public static void Save()
    {
        ConfigSave.AddItem(new()
        {
            Name = "gui.json",
            Local = Name,
            Obj = Config
        });
    }

    public static Render MakeRenderConfig()
    {
        return new()
        {
            Windows = new()
            {
                UseWindowsUIComposition = null,
                UseWgl = null,
                AllowEglInitialization = null
            },
            X11 = new()
            {
                UseEGL = null,
                UseGpu = null,
                OverlayPopups = null
            }
        };
    }

    public static GuiConfigObj MakeDefaultConfig()
    {
        return new()
        {
            Version = ColorMCCore.Version,
            ColorMain = "#FF5ABED6",
            ColorBack = "#FFF4F4F5",
            ColorTranBack = "#88FFFFFF",
            RGBS = 100,
            RGBV = 100,
            ColorFont1 = "#FFFFFFFF",
            ColorFont2 = "#FF000000",
            ServerCustom = MakeServerCustomConfig(),
            FontDefault = true,
            Render = MakeRenderConfig(),
            BackLimitValue = 50
        };
    }

    public static ServerCustom MakeServerCustomConfig()
    {
        return new()
        {
            MotdColor = "#FFFFFFFF",
            MotdBackColor = "#FF000000",
            Volume = 30
        };
    }
}