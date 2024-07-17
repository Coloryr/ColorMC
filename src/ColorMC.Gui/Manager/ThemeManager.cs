using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;


namespace ColorMC.Gui.Manager;

public static class ThemeManager
{
    public const string MainColorStr = "#FF5ABED6";

    private static readonly Dictionary<string, List<WeakReference<IObserver<IBrush>>>> s_colorList = [];
    private static readonly Dictionary<string, List<WeakReference<IObserver<Thickness>>>> s_thinkList = [];
    private static readonly Dictionary<string, List<WeakReference<IObserver<object?>>>> s_styleList = [];
    private static readonly List<WeakReference<IObserver<FontFamily>>> s_fontList = [];

    private static readonly ThemeObj s_light;
    private static readonly ThemeObj s_dark;

    private static readonly Random s_random = new();

    private static readonly double s_fontTitleSize = 17;
    private static readonly Thickness s_borderPadding = new(6, 6, 15, 6);
    private static readonly Thickness s_borderPadding1 = new(6);

    private static ThemeObj s_theme;

    private static BoxShadows s_buttonShadow;

    public static readonly BoxShadows BorderShadows = new(BoxShadow.Parse("0 0 3 1 #1A000000"), [BoxShadow.Parse("0 0 5 -1 #1A000000")]);
    public static BoxShadows BorderSelecrShadows;

    private static readonly IBrush[] s_colors = [Brush.Parse("#3b82f6"), Brush.Parse("#22c55e"),
        Brush.Parse("#eab308"), Brush.Parse("#ef4444"), Brush.Parse("#a855f7"),
        Brush.Parse("#ec4899"), Brush.Parse("#14b8a6"), Brush.Parse("#6366f1"),
        Brush.Parse("#f97316"), Brush.Parse("#06b6d4"), Brush.Parse("#84cc16")];

    private static FontFamily s_font = new(FontFamily.DefaultFontFamilyName);

    private static void RgbColor_ColorChanged()
    {
        Dispatcher.UIThread.Invoke(Reload);
    }

    public static void Load()
    {
        if (App.NowTheme == PlatformThemeVariant.Light)
        {
            s_theme = s_light;
        }
        else
        {
            s_theme = s_dark;
        }

        LoadColor();
        LoadFont();

        Reload();
    }

    private static void LoadColor()
    {
        s_theme.MainColor = Brush.Parse(GuiConfigUtils.Config.ColorMain);
        var color = s_theme.MainColor.ToColor();
        var color1 = new Color(255, color.R, color.G, color.B);

        s_buttonShadow = new(new BoxShadow
        {
            Blur = 3,
            Spread = 1,
            Color = color1
        });

        BorderSelecrShadows = new(new BoxShadow
        {
            Blur = 3,
            Spread = 1,
            Color = color1
        });

        RgbColor.Load();
        ColorSel.Load();
    }

    private static void LoadFont()
    {
        if (!GuiConfigUtils.Config.FontDefault
            && !string.IsNullOrWhiteSpace(GuiConfigUtils.Config.FontName)
            && FontManager.Current.SystemFonts.Any(a => a.Name == GuiConfigUtils.Config.FontName)
            && SkiaSharp.SKFontManager.Default.MatchFamily(GuiConfigUtils.Config.FontName) is { } font)
        {
            s_font = new(font.FamilyName);
        }
        else
        {
            s_font = new(ColorMCGui.Font);
        }
    }

    public static IBrush GetColor(string key)
    {
        if (key == "WindowBG")
        {
            if (ImageManager.BackBitmap != null)
            {
                return new SolidColorBrush(s_theme.WindowBG.ToColor(), 0.75);
            }
            else if (GuiConfigUtils.Config.WindowTran)
            {
                return Brushes.Transparent;
            }
            return s_theme.WindowBG;
        }
        else if (key == "WindowTranColor")
        {
            return s_theme.WindowTranColor;
        }
        else if (key == "WindowBase")
        {
            if (ImageManager.BackBitmap != null)
            {
                return new SolidColorBrush(s_theme.WindowBG.ToColor(), 0.75);
            }
            return Brushes.Transparent;
        }
        else if (key == "ItemBG")
        {
            return s_theme.ItemBG;
        }
        else if (key == "MainGroupBG")
        {
            if (ImageManager.BackBitmap != null)
            {
                return new SolidColorBrush(s_theme.MainGroupBG.ToColor(), 0.75);
            }
            return s_theme.MainGroupBG;
        }
        else if (key == "MainGroupBorder")
        {
            if (GuiConfigUtils.Config.WindowTran && ImageManager.BackBitmap == null)
            {
                return s_theme.MainGroupBorder;
            }

            return Brushes.Transparent;
        }
        else if (key == "MainGroupItemBG")
        {
            if (ImageManager.BackBitmap != null)
            {
                return new SolidColorBrush(s_theme.MainGroupBG.ToColor(), 0.75);
            }
            return Brushes.Transparent;
        }
        else if (key == "ProgressBarBG")
        {
            return s_theme.ProgressBarBG;
        }
        else if (key == "GameItemBG")
        {
            return s_theme.GameItemBG;
        }
        else if (key == "TopViewBG")
        {
            return s_theme.TopViewBG;
        }
        else if (key == "AllBorder")
        {
            return s_theme.AllBorder;
        }
        else if (key == "ButtonBorder")
        {
            return s_theme.ButtonBorder;
        }
        else if (key == "MainColor")
        {
            return RgbColor.IsEnable() ? RgbColor.GetColor() : s_theme.MainColor;
        }
        else if (key == "FontColor")
        {
            return s_theme.FontColor;
        }
        else if (key == "TopBGColor")
        {
            return s_theme.TopBGColor;
        }
        else if (key == "TopGridColor")
        {
            return s_theme.TopGridColor;
        }
        else if (key == "OverBGColor")
        {
            return s_theme.OverBGColor;
        }
        else if (key == "OverBrushColor")
        {
            return s_theme.OverBrushColor;
        }
        else if (key == "RandomColor")
        {
            return s_colors[s_random.Next(s_colors.Length)];
        }

        return Brushes.Transparent;
    }

    private static object? GetStyle(string key)
    {
        if (key == "FontTitle")
        {
            return s_fontTitleSize;
        }
        else if (key == "ButtonTopBoxShadow")
        {
            return s_buttonShadow;
        }
        return null;
    }

    public static IDisposable AddFont(IObserver<FontFamily> observer)
    {
        s_fontList.Add(new WeakReference<IObserver<FontFamily>>(observer));
        observer.OnNext(s_font);
        return new UnsubscribeFont(s_fontList, observer);
    }

    public static IDisposable AddStyle(string key, IObserver<object?> observer)
    {
        if (s_styleList.TryGetValue(key, out var list))
        {
            list.Add(new(observer));
        }
        else
        {
            list = [new(observer)];
            s_styleList.Add(key, list);
        }
        var value = GetStyle(key);
        observer.OnNext(value);
        return new UnsubscribeStyle(list, observer);
    }

    private static Thickness GetThick(string key)
    {
        if (key == "Border")
        {
            return GuiConfigUtils.Config.WindowTran && ImageManager.BackBitmap == null ? new(1) : new(0);
        }

        return new(0);
    }

    public static IDisposable Add(string key, IObserver<IBrush> observer)
    {
        if (s_colorList.TryGetValue(key, out var list))
        {
            list.Add(new(observer));
        }
        else
        {
            list = [new(observer)];
            s_colorList.Add(key, list);
        }
        var value = GetColor(key);
        observer.OnNext(value);
        return new UnsubscribeColor(list, observer);
    }

    public static IDisposable Add(string key, IObserver<Thickness> observer)
    {
        if (s_thinkList.TryGetValue(key, out var list))
        {
            list.Add(new(observer));
        }
        else
        {
            list = [new(observer)];
            s_thinkList.Add(key, list);
        }
        var value = GetThick(key);
        observer.OnNext(value);
        return new UnsubscribeThick(list, observer);
    }

    public static void Remove()
    {
        ColorSel.Remove();

        foreach (var item in s_colorList.Values)
        {
            foreach (var item1 in item.ToArray())
            {
                if (!item1.TryGetTarget(out _))
                {
                    item.Remove(item1);
                }
            }
        }
        foreach (var item in s_thinkList.Values)
        {
            foreach (var item1 in item.ToArray())
            {
                if (!item1.TryGetTarget(out _))
                {
                    item.Remove(item1);
                }
            }
        }

        foreach (var item in s_styleList.Values)
        {
            foreach (var item1 in item.ToArray())
            {
                if (!item1.TryGetTarget(out _))
                {
                    item.Remove(item1);
                }
            }
        }

        foreach (var item in s_fontList.ToArray())
        {
            if (!item.TryGetTarget(out _))
            {
                s_fontList.Remove(item);
            }
        }
    }

    private static void Reload()
    {
        foreach (var item in s_colorList)
        {
            if (item.Key.StartsWith("Random"))
            {
                foreach (var item1 in item.Value)
                {
                    if (item1.TryGetTarget(out var target))
                    {
                        target.OnNext(GetColor(item.Key));
                    }
                }
            }
            else
            {
                var value = GetColor(item.Key);
                foreach (var item1 in item.Value)
                {
                    if (item1.TryGetTarget(out var target))
                    {
                        target.OnNext(value);
                    }
                }
            }
        }
        foreach (var item in s_thinkList)
        {
            var value = GetThick(item.Key);
            foreach (var item1 in item.Value)
            {
                if (item1.TryGetTarget(out var target))
                {
                    target.OnNext(value);
                }
            }
        }

        foreach (var item in s_styleList)
        {
            var value = GetStyle(item.Key);
            foreach (var item1 in item.Value)
            {
                if (item1.TryGetTarget(out var target))
                {
                    target.OnNext(value);
                }
            }
        }

        foreach (var item in s_fontList)
        {
            if (item.TryGetTarget(out var target))
            {
                target.OnNext(s_font);
            }
        }
    }

    static ThemeManager()
    {
        RgbColor.ColorChanged += RgbColor_ColorChanged;

        s_light = new()
        {
            MainColor = Brush.Parse(MainColorStr),
            FontColor = Brush.Parse("#FF000000"),
            WindowBG = Brush.Parse("#FFf3f3f3"),
            WindowTranColor = Brush.Parse("#80FFFFFF"),
            ProgressBarBG = Brush.Parse("#FFe4e4e7"),
            MainGroupBG = Brush.Parse("#FFd4d4d8"),
            MainGroupBorder = Brush.Parse("#FFE0E0E0"),
            ItemBG = Brush.Parse("#CFFFFFFF"),
            GameItemBG = Brush.Parse("#FFF2F2F2"),
            TopViewBG = Brush.Parse("#886D6D6D"),
            AllBorder = Brush.Parse("#FFe5e7eb"),
            ButtonBorder = Brush.Parse("#FFD4D4D8"),
            TopBGColor = Brush.Parse("#CFF4F4F5"),
            TopGridColor = Brush.Parse("#FFFFFFFF"),
            OverBGColor = Brush.Parse("#FFFFFFFF"),
            OverBrushColor = Brush.Parse("#FFe5e5e5")
        };

        s_dark = new()
        {
            MainColor = Brush.Parse(MainColorStr),
            FontColor = Brush.Parse("#FFFFFFFF"),
            WindowBG = Brush.Parse("#FF18181b"),
            WindowTranColor = Brush.Parse("#80202020"),
            ProgressBarBG = Brush.Parse("#FF3f3f46"),
            MainGroupBG = Brush.Parse("#FF27272a"),
            MainGroupBorder = Brush.Parse("#FFE0E0E0"),
            ItemBG = Brush.Parse("#CF27272a"),
            GameItemBG = Brush.Parse("#FFc7c7cb"),
            TopViewBG = Brush.Parse("#886D6D6D"),
            AllBorder = Brush.Parse("#FFe5e7eb"),
            ButtonBorder = Brush.Parse("#FFD4D4D8"),
            TopBGColor = Brush.Parse("#CFF4F4F5"),
            TopGridColor = Brush.Parse("#FF202020"),
            OverBGColor = Brush.Parse("#FF000000"),
            OverBrushColor = Brush.Parse("#FF1d1d1d"),
        };
    }
}
