using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;

namespace ColorMC.Gui.Manager;

public static class ThemeManager
{
    private static readonly Dictionary<string, List<WeakReference<IObserver<IBrush>>>> s_colorList = [];
    private static readonly Dictionary<string, List<WeakReference<IObserver<Thickness>>>> s_thinkList = [];
    private static readonly Dictionary<string, List<WeakReference<IObserver<object?>>>> s_styleList = [];

    private static readonly ThemeObj s_light;
    private static readonly ThemeObj s_dark;

    private static readonly Random s_random = new();

    private static readonly double s_fontTitleSize = 17;
    private static readonly Thickness s_borderPadding = new(6, 6, 15, 6);
    private static readonly Thickness s_borderPadding1 = new(6);

    private static ThemeObj s_theme;

    private static CornerRadius s_buttonCornerRadius = new(3);
    private static CornerRadius s_picRadius = new(0);
    private static int Radius;

    private static BoxShadows s_buttonShadow;

    public static readonly BoxShadows BorderShadows = new(BoxShadow.Parse("0 0 3 1 #1A000000"), [BoxShadow.Parse("0 0 5 -1 #1A000000")]);
    public static BoxShadows BorderSelecrShadows;

    private static readonly IBrush[] s_colors = [Brush.Parse("#3b82f6"), Brush.Parse("#22c55e"), 
        Brush.Parse("#eab308"), Brush.Parse("#ef4444"), Brush.Parse("#a855f7"), 
        Brush.Parse("#ec4899"), Brush.Parse("#14b8a6"), Brush.Parse("#6366f1"), 
        Brush.Parse("#f97316"), Brush.Parse("#06b6d4"), Brush.Parse("#84cc16")];

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

        var config = GuiConfigUtils.Config.Style;

        s_buttonCornerRadius = new(config.ButtonCornerRadius);

        if (config.EnablePicRadius)
        {
            s_picRadius = new(config.ButtonCornerRadius);
        }
        else
        {
            s_picRadius = new(0);
        }

        Radius = config.EnableBorderRadius ? config.ButtonCornerRadius : 0;

        var color = ColorSel.MainColor.ToColor();
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

        Reload();
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
        else if (key == "RandomColor")
        {
            return s_colors[s_random.Next(s_colors.Length)];
        }

        return Brushes.Transparent;
    }

    private static object? GetStyle(string key)
    {
        if (key == "ButtonCornerRadius")
        {
            return s_buttonCornerRadius;
        }
        else if (key == "PicRadius")
        {
            return s_picRadius;
        }
        else if (key == "FontTitle")
        {
            return s_fontTitleSize;
        }
        else if (key == "Radius")
        {
            return Radius;
        }
        else if (key == "BorderPadding")
        {
            return s_borderPadding;
        }
        else if (key == "BorderPadding1")
        {
            return s_borderPadding1;
        }
        else if (key == "ButtonTopBoxShadow")
        {
            return s_buttonShadow;
        }
        return null;
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
    }

    private class UnsubscribeColor(List<WeakReference<IObserver<IBrush>>> observers, IObserver<IBrush> observer) : IDisposable
    {
        public void Dispose()
        {
            foreach (var item in observers.ToArray())
            {
                if (!item.TryGetTarget(out var target)
                    || target == observer)
                {
                    observers.Remove(item);
                }
            }
        }
    }

    private class UnsubscribeThick(List<WeakReference<IObserver<Thickness>>> observers, IObserver<Thickness> observer) : IDisposable
    {
        public void Dispose()
        {
            foreach (var item in observers.ToArray())
            {
                if (!item.TryGetTarget(out var target)
                    || target == observer)
                {
                    observers.Remove(item);
                }
            }
        }
    }

    private class UnsubscribeStyle(List<WeakReference<IObserver<object?>>> observers, IObserver<object?> observer) : IDisposable
    {
        public void Dispose()
        {
            foreach (var item in observers.ToArray())
            {
                if (!item.TryGetTarget(out var target)
                    || target == observer)
                {
                    observers.Remove(item);
                }
            }
        }
    }

    static ThemeManager()
    {
        s_light = new()
        {
            WindowBG = Brush.Parse("#FFf3f3f3"),
            ProgressBarBG = Brush.Parse("#FFe4e4e7"),
            MainGroupBG = Brush.Parse("#FFd4d4d8"),
            MainGroupBorder = Brush.Parse("#FFE0E0E0"),
            ItemBG = Brush.Parse("#FFFFFFFF"),
            GameItemBG = Brush.Parse("#FFF2F2F2"),
            TopViewBG = Brush.Parse("#886D6D6D"),
            AllBorder = Brush.Parse("#FFe5e7eb"),
            ButtonBorder = Brush.Parse("#FFD4D4D8")
        };

        s_dark = new()
        {
            WindowBG = Brush.Parse("#FF18181b"),
            ProgressBarBG = Brush.Parse("#FF3f3f46"),
            MainGroupBG = Brush.Parse("#FF27272a"),
            MainGroupBorder = Brush.Parse("#FFE0E0E0"),
            ItemBG = Brush.Parse("#FF27272a"),
            GameItemBG = Brush.Parse("#FFc7c7cb"),
            TopViewBG = Brush.Parse("#886D6D6D"),
            AllBorder = Brush.Parse("#FFe5e7eb"),
            ButtonBorder = Brush.Parse("#FFD4D4D8")
        };
    }
}
