using Avalonia;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ColorMC.Gui.Utils.LaunchSetting;

/// <summary>
/// 样式获取
/// </summary>
public static class StyleSel
{
    private static readonly Dictionary<string, List<IObserver<object?>>> s_styleList = [];

    private static readonly double s_fontTitleSize = 17;
    private static readonly Thickness s_borderPadding = new(6);

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
        return null;
    }

    public static IDisposable Add(string key, IObserver<object?> observer)
    {
        if (s_styleList.TryGetValue(key, out var list))
        {
            list.Add(observer);
        }
        else
        {
            list = [observer];
            s_styleList.Add(key, list);
        }
        var value = Get(key);
        observer.OnNext(value);
        return new Unsubscribe(list, observer);
    }

    private class Unsubscribe(List<IObserver<object?>> observers, IObserver<object?> observer) : IDisposable
    {
        public void Dispose()
        {
            observers.Remove(observer);
        }
    }

    private static void Reload()
    {
        foreach (var item in s_styleList)
        {
            var value = Get(item.Key);
            foreach (var item1 in item.Value)
            {
                item1.OnNext(value);
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
}
