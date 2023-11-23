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
public class FontSel : INotifyPropertyChanged
{
    private static FontFamily s_font = new(ColorMCGui.Font);
    public readonly static FontSel Instance = new FontSel();

    /// <summary>
    /// 刷新UI
    /// </summary>
    public FontFamily this[string key]
    {
        get
        {
            return s_font;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerArrayName));
    }

    /// <summary>
    /// 加载字体
    /// </summary>
    public void Load()
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
