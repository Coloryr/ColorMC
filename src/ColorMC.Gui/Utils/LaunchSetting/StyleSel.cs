using Avalonia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tmds.DBus.SourceGenerator;

namespace ColorMC.Gui.Utils.LaunchSetting;

/// <summary>
/// 样式获取
/// </summary>
public class StyleSel : INotifyPropertyChanged

{
    public readonly static StyleSel Instance = new();

    private static readonly double s_fontTitleSize = 17;
    private static readonly Thickness s_borderPadding = new(6);

    private static CornerRadius s_buttonCornerRadius = new(3);
    private static CornerRadius s_picRadius = new(0);
    private static int Radius;

    public object? this[string key]
    {
        get
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
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerArrayName));
    }

    /// <summary>
    /// 加载
    /// </summary>
    public void Load()
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
