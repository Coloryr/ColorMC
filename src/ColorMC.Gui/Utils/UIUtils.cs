using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using ColorMC.Gui.Utils.LaunchSetting;
using System;

namespace ColorMC.Gui.Utils;

/// <summary>
/// UI处理
/// </summary>
public static class UIUtils
{
    /// <summary>
    /// 找到控件
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="visual">查找</param>
    /// <returns>控件</returns>
    public static T? FindToEnd<T>(this Visual visual)
    {
        foreach (var item in visual.GetVisualChildren())
        {
            if (item is T t)
            {
                return t;
            }
        }

        foreach (var item in visual.GetVisualChildren())
        {
            var res = FindToEnd<T>(item);
            if (res != null)
            {
                return res;
            }
        }

        return default;
    }

    private class ColorObservable(string key) : IObservable<IBrush>
    {
        public IDisposable Subscribe(IObserver<IBrush> observer)
        {
            return ColorSel.Add(key, observer);
        }
    }

    /// <summary>
    /// 转速度
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string MakeSpeedSize(long size)
    {
        if (size > 1000000)
        {
            return $"{(double)size / 1000000:#.000}Mb/s";
        }
        else if (size > 1000)
        {
            return $"{(double)size / 1000:#.000}Kb/s";
        }
        else
        {
            return $"{size}b/s";
        }
    }

    /// <summary>
    /// 转大小
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string MakeFileSize1(long size)
    {
        if (size > 1000000)
        {
            return $"{(double)size / 1000000:#.000}MB";
        }
        else if (size > 1000)
        {
            return $"{(double)size / 1000:#.000}KB";
        }
        else
        {
            return $"{size}";
        }
    }

    /// <summary>
    /// 颜色转换
    /// </summary>
    /// <param name="brush"></param>
    /// <returns></returns>
    public static Color ToColor(this IBrush brush)
    {
        if (brush is ImmutableSolidColorBrush brush1)
        {
            return brush1.Color;
        }

        return new(255, 255, 255, 255);
    }

    /// <summary>
    /// 获得控件大小
    /// </summary>
    /// <param name="visual"></param>
    /// <returns></returns>
    public static (double X, double Y) GetXY(this Visual? visual)
    {
        if (visual == null)
            return (0, 0);
        var temp = (visual.Bounds.X, visual.Bounds.Y);
        if (visual.GetVisualParent() != null)
        {
            var (X, Y) = GetXY(visual.GetVisualParent());
            temp.X += X;
            temp.Y += Y;
        }

        return temp;
    }
}
