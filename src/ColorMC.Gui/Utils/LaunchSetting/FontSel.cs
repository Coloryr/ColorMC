using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ColorMC.Gui.Utils.LaunchSetting;

/// <summary>
/// 字体
/// </summary>
public static class FontSel
{
    private static FontFamily s_font = new(FontFamily.DefaultFontFamilyName);
    private static readonly List<WeakReference<IObserver<FontFamily>>> s_fontList = [];

    public static IDisposable Add(IObserver<FontFamily> observer)
    {
        s_fontList.Add(new WeakReference<IObserver<FontFamily>>(observer));
        observer.OnNext(s_font);
        return new Unsubscribe(observer);
    }

    public static void Remove(IObserver<FontFamily> observer)
    {
        foreach (var item in s_fontList.ToArray())
        {
            if (!item.TryGetTarget(out var target)
                || target == observer)
            {
                s_fontList.Remove(item);
            }
        }
    }

    private class Unsubscribe(IObserver<FontFamily> observer) : IDisposable
    {
        public void Dispose()
        {
            Remove(observer);
        }
    }

    /// <summary>
    /// 刷新UI
    /// </summary>
    private static void Reload()
    {
        foreach (var item in s_fontList)
        {
            if (item.TryGetTarget(out var target))
            {
                target.OnNext(s_font);
            }
        }
    }

    /// <summary>
    /// 加载字体
    /// </summary>
    public static void Load()
    {
        if (!GuiConfigUtils.Config.FontDefault
            && !string.IsNullOrWhiteSpace(GuiConfigUtils.Config.FontName)
            && FontManager.Current.SystemFonts.Any(a => a.Name == GuiConfigUtils.Config.FontName)
            && SkiaSharp.SKFontManager.Default.MatchFamily(GuiConfigUtils.Config.FontName) is { } font)
        {
            s_font = new(font.FamilyName);
            Reload();
        }
        else
        {
            s_font = new(ColorMCGui.Font);
            Reload();
        }
    }

    public static void Remove()
    {
        foreach (var item in s_fontList.ToArray())
        {
            if (!item.TryGetTarget(out _))
            {
                s_fontList.Remove(item);
            }
        }
    }
}