using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using ColorMC.Core.Net.Motd;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Dialog;
using ColorMC.Gui.UI.Model.Dialog;

namespace ColorMC.Gui.Utils;

public class TemplateSelector : IDataTemplate
{
    private static readonly Dictionary<Type, Type> _templates = [];

    static TemplateSelector()
    {
        _templates.Add(typeof(ProgressModel), typeof(ProgressControl));
        _templates.Add(typeof(InputModel), typeof(InputControl));
        _templates.Add(typeof(ChoiceModel), typeof(ChoiceControl));
        _templates.Add(typeof(SelectModel), typeof(SelectControl));
        _templates.Add(typeof(LongTextModel), typeof(LongTextControl));
        _templates.Add(typeof(GroupEditModel), typeof(GroupEditControl));
        _templates.Add(typeof(JoystickSettingModel), typeof(JoystickSettingControl));
        _templates.Add(typeof(ModDependModel), typeof(ModDependControl));
        _templates.Add(typeof(CollectDownloadModel), typeof(CollectDownloadControl));
        _templates.Add(typeof(SelectGameModel), typeof(SelectGameControl));
        _templates.Add(typeof(NbtDialogAddModel), typeof(NbtDialogAddControl));
        _templates.Add(typeof(NbtDialogEditModel), typeof(NbtDialogEditControl));
        _templates.Add(typeof(NbtDialogFindModel), typeof(NbtDialogFindControl));
        _templates.Add(typeof(NetFrpAddModel), typeof(NetFrpAddControl));
        _templates.Add(typeof(FrpShareModel), typeof(FrpShareControl));
        _templates.Add(typeof(AddLockLoginModel), typeof(AddLockLoginControl));
        _templates.Add(typeof(AddUserModel), typeof(AddUserControl));
        _templates.Add(typeof(BlockListModel), typeof(BlockListControl));
    }

    public Control? Build(object? param)
    {
        var key = (param?.GetType())
            ?? throw new ArgumentNullException(nameof(param));

        if (_templates.TryGetValue(key, out var view))
        {
            return Activator.CreateInstance(view) as Control;
        }

        return null;
    }

    public bool Match(object? data)
    {
        var key = data?.GetType();
        return key != null
            && _templates.ContainsKey(key);
    }
}

/// <summary>
/// UI处理
/// </summary>
public static class UIUtils
{
    /// <summary>
    /// 测量左边距
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public static double SumLeft(this Control control)
    {
        if (!control.IsMeasureValid)
        {
            return 0;
        }

        double left = control.Bounds.Left;
        var par = control.Parent;
        for (; ; )
        {
            if (par == null)
            {
                return left;
            }
            if (par is Visual visual)
            {
                left += visual.Bounds.Left;
            }
            par = par.Parent;
        }
    }

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

    public static string MakeDownload(long times)
    {
        if (GuiConfigUtils.Config.Language == LanguageType.zh_cn)
        {
            if (times > 100000000)
            {
                return $"{times / 100000000}亿+";
            }
            else if (times > 10000)
            {
                return $"{times / 10000}万+";
            }
            else if (times > 1000)
            {
                return $"{times / 10000}千+";
            }
            else
            {
                return $"{times}次";
            }
        }

        return times.ToString();
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

    public static IBrush GetColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return Brushes.White;
        }
        if (color.StartsWith('#'))
        {
            return Brush.Parse(color);
        }
        if (ServerMotd.ColorMap.TryGetValue(color, out var color1))
        {
            return Brush.Parse(color1);
        }

        return Brush.Parse(color);
    }

    public static void StringCut(ref string text)
    {
        if (text.Length > 40)
        {
            text = "..." + text[^40..];
        }
    }
}
