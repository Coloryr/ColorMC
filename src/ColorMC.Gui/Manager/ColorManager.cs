using System;
using System.Collections.Generic;
using Avalonia.Media;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 颜色设定
/// </summary>
public static class ColorManager
{
    public static IBrush MotdColor { get; private set; } = Brush.Parse("#FFFFFFFF");
    public static IBrush MotdBackColor { get; private set; } = Brush.Parse("#FF000000");

    public static IBrush InfoColor { get; private set; } = Brushes.Black;
    public static IBrush WarnColor { get; private set; } = Brush.Parse("#8B8B00");
    public static IBrush ErrorColor { get; private set; } = Brushes.Red;
    public static IBrush DebugColor { get; private set; } = Brushes.Gray;
    public static IBrush NoneColor { get; private set; } = Brushes.Black;


    private static readonly Dictionary<string, List<WeakReference<IObserver<IBrush>>>> s_colorList = [];

    /// <summary>
    /// 加载颜色
    /// </summary>
    public static void Load()
    {
        try
        {
            MotdColor = Brush.Parse(GuiConfigUtils.Config.ServerCustom.MotdColor);
            MotdBackColor = Brush.Parse(GuiConfigUtils.Config.ServerCustom.MotdBackColor);

            InfoColor = Brush.Parse(GuiConfigUtils.Config.LogColor.Info);
            WarnColor = Brush.Parse(GuiConfigUtils.Config.LogColor.Warn);
            ErrorColor = Brush.Parse(GuiConfigUtils.Config.LogColor.Error);
            DebugColor = Brush.Parse(GuiConfigUtils.Config.LogColor.Debug);
            NoneColor = Brush.Parse(GuiConfigUtils.Config.LogColor.None);

            Reload();
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("Config.Error4"), e);
        }
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

    public static void Reload()
    {
        foreach (var item in s_colorList)
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
    }

    public static IBrush GetColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Info => InfoColor,
            LogLevel.Warn => WarnColor,
            LogLevel.Debug => DebugColor,
            LogLevel.Error => ErrorColor,
            LogLevel.Fatal => ErrorColor,
            _ => NoneColor
        };
    }

    private static IBrush GetColor(string key)
    {
        if (key == "Motd")
            return MotdColor;
        else if (key == "MotdBack")
            return MotdBackColor;

        return Brushes.White;
    }
}