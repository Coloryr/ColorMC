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

namespace ColorMC.Gui.Manager;

public static class ThemeManager
{
    private static readonly Dictionary<string, List<WeakReference<IObserver<IBrush>>>> s_colorList = [];
    private static readonly Dictionary<string, List<WeakReference<IObserver<Thickness>>>> s_thinkList = [];

    private static readonly ThemeObj s_light;
    private static readonly ThemeObj s_dark;

    private static ThemeObj s_theme;
    private static readonly Random s_random = new();

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
        else if (key == "RandomColor")
        {
            return s_colors[s_random.Next(s_colors.Length)];
        }

        return Brushes.Transparent;
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
        return new Unsubscribe(list, observer);
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

    private class Unsubscribe(List<WeakReference<IObserver<IBrush>>> observers, IObserver<IBrush> observer) : IDisposable
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

    static ThemeManager()
    {
        s_light = new()
        {
            WindowBG = Brush.Parse("#FFF4F4F5"),
            ProgressBarBG = Brush.Parse("#FFe4e4e7"),
            MainGroupBG = Brush.Parse("#FFd4d4d8"),
            MainGroupBorder = Brush.Parse("#FFE0E0E0"),
            ItemBG = Brush.Parse("#FFFFFFFF"),
            GameItemBG = Brush.Parse("#FFF2F2F2")
        };

        s_dark = new()
        {
            WindowBG = Brush.Parse("#FF18181b"),
            ProgressBarBG = Brush.Parse("#FF3f3f46"),
            MainGroupBG = Brush.Parse("#FF27272a"),
            MainGroupBorder = Brush.Parse("#FFE0E0E0"),
            ItemBG = Brush.Parse("#FF27272a"),
            GameItemBG = Brush.Parse("#FFc7c7cb")
        };
    }
}
