using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Utils.LaunchSetting;

/// <summary>
/// 样式获取
/// </summary>
public static class StyleSel
{
    private static readonly Dictionary<string, List<WeakReference<IObserver<object?>>>> s_styleList = [];

    private static readonly double s_fontTitleSize = 17;
    private static readonly Thickness s_borderPadding = new(6, 6, 15, 6);
    private static readonly Thickness s_borderPadding1 = new(6);

    private static readonly BoxShadows s_shadow = BoxShadows.Parse("0 1 3 0 #999999");
    private static readonly BoxShadows s_shadow1 = BoxShadows.Parse("0 1 3 0 #EEEEEE");

    private static CornerRadius s_buttonCornerRadius = new(3);
    private static CornerRadius s_picRadius = new(0);
    private static int Radius;

    private static object? Get(string key)
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
        else if (key == "ButtonLight")
        {
            return App.NowTheme == PlatformThemeVariant.Light ? s_shadow : s_shadow1;
        }
        return null;
    }

    public static IDisposable Add(string key, IObserver<object?> observer)
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
        var value = Get(key);
        observer.OnNext(value);
        return new Unsubscribe(list, observer);
    }

    private class Unsubscribe(List<WeakReference<IObserver<object?>>> observers, IObserver<object?> observer) : IDisposable
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

    private static void Reload()
    {
        foreach (var item in s_styleList)
        {
            var value = Get(item.Key);
            foreach (var item1 in item.Value)
            {
                if (item1.TryGetTarget(out var target))
                {
                    target.OnNext(value);
                }
            }
        }
    }

    /// <summary>
    /// 加载
    /// </summary>
    public static void Load()
    {
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

        Reload();
    }

    public static void Remove()
    {
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
    }
}