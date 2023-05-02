using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Media.Immutable;
using Avalonia;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.VisualTree;
using Avalonia.Data;
using Avalonia.Media;

namespace ColorMC.Gui.Utils;

public static partial class UIUtils
{
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

    public static void MakeTran(this Expander expander)
    {
        try
        {
            var item1 = expander.FindToEnd<Border>();
            item1?.Bind(Border.BackgroundProperty, new Binding
            {
                Source = ColorSel.Instance,
                Path = "[TranBack]"
            });
        }
        catch
        {

        }
    }

    public static void MakeTran(this DataGrid grid)
    {
        try
        {
            var item1 = grid.FindToEnd<DataGridColumnHeadersPresenter>();
            if (item1 != null)
            {
                //item1.Background = Brush.Parse("#CCffffff");
                foreach (var item in item1.GetVisualChildren())
                {
                    var item2 = item.FindToEnd<TextBlock>();
                    item2?.Bind(TextBlock.ForegroundProperty, new Binding
                    {
                        Source = ColorSel.Instance,
                        Path = "[Font]"
                    });
                }
            }
        }
        catch
        {

        }
    }

    public static string MakeString(this List<string>? strings)
    {
        if (strings == null)
            return "";
        string temp = "";
        foreach (var item in strings)
        {
            temp += item + ",";
        }

        if (temp.Length > 0)
        {
            return temp[..^1];
        }

        return temp;
    }

    public static string MakeFileSize(long size)
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

    public static Avalonia.Media.Color ToColor(this IBrush brush)
    {
        if (brush is ImmutableSolidColorBrush brush1)
        {
            return brush1.Color;
        }

        return new(255, 255, 255, 255);
    }

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

    public static T? FindTop<T>(this Visual visual) where T : Visual
    {
        var pan = visual.GetVisualParent();
        while (pan != null)
        {
            if (pan is T t)
            {
                return t;
            }
            pan = pan.GetVisualParent();
        }

        return default;
    }
}
