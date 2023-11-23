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
    private static FontFamily s_font = new(ColorMCGui.Font);
    private static readonly List<IObserver<FontFamily>> s_fontList = [];

    public static IDisposable Add(IObserver<FontFamily> observer)
    {
        s_fontList.Add(observer);
        observer.OnNext(s_font);
        return new Unsubscribe(observer);
    }

    public static void Remove(IObserver<FontFamily> observer)
    {
        s_fontList.Remove(observer);
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
            item.OnNext(s_font);
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
}
