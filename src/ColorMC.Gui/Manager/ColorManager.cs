using System;
using System.Collections.Generic;
using Avalonia.Media;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 颜色设定
/// </summary>
public static class ColorManager
{
    /// <summary>
    /// Motd文字颜色
    /// </summary>
    public static IBrush MotdColor { get; private set; } = Brush.Parse("#FFFFFFFF");
    /// <summary>
    /// Motd背景色
    /// </summary>
    public static IBrush MotdBackColor { get; private set; } = Brush.Parse("#FF000000");

    /// <summary>
    /// 日志警告颜色
    /// </summary>
    public static IBrush WarnColor { get; private set; } = Brush.Parse("#8B8B00");
    /// <summary>
    /// 日志错误颜色
    /// </summary>
    public static IBrush ErrorColor { get; private set; } = Brushes.Red;
    /// <summary>
    /// 日志调试颜色
    /// </summary>
    public static IBrush DebugColor { get; private set; } = Brushes.Gray;

    /// <summary>
    /// 绑定UI的颜色更新器列表
    /// </summary>
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

            WarnColor = Brush.Parse(GuiConfigUtils.Config.LogColor.Warn);
            ErrorColor = Brush.Parse(GuiConfigUtils.Config.LogColor.Error);
            DebugColor = Brush.Parse(GuiConfigUtils.Config.LogColor.Debug);

            Reload();
        }
        catch (Exception e)
        {
            Logs.Error(LanguageUtils.Get("App.Error.Log15"), e);
        }
    }

    /// <summary>
    /// 添加UI颜色绑定
    /// </summary>
    /// <param name="key">颜色键</param>
    /// <param name="observer">更新器</param>
    /// <returns></returns>
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

    /// <summary>
    /// 刷新UI颜色设置
    /// </summary>
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

    /// <summary>
    /// 清理UI颜色绑定器
    /// </summary>
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

    /// <summary>
    /// 获取颜色
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static IBrush GetColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Warn => WarnColor,
            LogLevel.Debug => DebugColor,
            LogLevel.Error => ErrorColor,
            LogLevel.Fatal => ErrorColor,
            _ => ThemeManager.NowThemeColor.FontColor
        };
    }

    /// <summary>
    /// 获取颜色
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static IBrush GetColor(string key)
    {
        if (key == "Motd")
            return MotdColor;
        else if (key == "MotdBack")
            return MotdBackColor;

        return Brushes.White;
    }
}