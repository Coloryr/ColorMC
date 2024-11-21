using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace ColorMC.Gui.UI.Views;

public class WrapPanelWithStretch : Panel
{
    public static readonly StyledProperty<bool> RightMaxProperty =
        AvaloniaProperty.Register<WrapPanelWithStretch, bool>(nameof(RightMax));

    public static readonly StyledProperty<bool> LeftMaxProperty =
        AvaloniaProperty.Register<WrapPanelWithStretch, bool>(nameof(LeftMax));

    public bool RightMax
    {
        get => GetValue(RightMaxProperty);
        set => SetValue(RightMaxProperty, value);
    }
    public bool LeftMax
    {
        get => GetValue(LeftMaxProperty);
        set => SetValue(LeftMaxProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RightMaxProperty
            || change.Property == LeftMaxProperty)
        {
            InvalidateMeasure();
            InvalidateArrange();
        }
    }

    private static bool IsErrorSize(double value)
    {
        return value <= 0 || double.IsInfinity(value) || double.IsNaN(value);
    }

    private static double GetWidth(Control control)
    {
        double width = control.MinWidth;
        var childSize = control.DesiredSize;
        if (IsErrorSize(width))
        {
            width = control.Width;
        }
        if (IsErrorSize(width))
        {
            width = childSize.Width;
        }

        return Math.Max(width, childSize.Width);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double totalWidth = 0;
        double totalHeight = 0;
        double lineWidth = 0;
        double lineHeight = 0;

        bool left = LeftMax;
        bool right = RightMax;

        foreach (var child in Children)
        {
            child.Measure(Size.Infinity);
            var childSize = child.DesiredSize;
            double minWidth = GetWidth(child);

            if (lineWidth + minWidth > availableSize.Width && lineWidth > 0)
            {
                totalHeight += lineHeight;
                if (totalWidth < lineWidth)
                {
                    totalWidth = lineWidth;
                }
                lineWidth = 0;
            }

            lineWidth += minWidth;
            lineHeight = Math.Max(lineHeight, childSize.Height);
            if (totalWidth < lineWidth)
            {
                totalWidth = lineWidth;
            }
        }

        totalHeight += lineHeight;

        if (left || right)
        {
            return new Size(availableSize.Width, totalHeight);
        }

        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double totalWidth = 0;
        double totalHeight = 0;
        double lineWidth = 0;
        double lineHeight = 0;

        //上一个控件的高度
        double widthToUse = 0;
        Control? last = null;
        bool left = LeftMax;
        bool right = RightMax;
        var controls = new Stack<(Control con, Rect size)>();

        foreach (var child in Children)
        {
            var childSize = child.DesiredSize;
            //控件占用的宽度
            double minWidth = GetWidth(child);

            lineHeight = Math.Max(lineHeight, childSize.Height);

            //是否需要换行
            if (lineWidth + minWidth > finalSize.Width)
            {
                double lastheight = 0;

                //最左边的最大
                if (left)
                {
                    lineWidth = finalSize.Width;

                    foreach (var (con, size) in controls)
                    {
                        lastheight = Math.Max(size.Height, lastheight);
                    }

                    //从右往左排序
                    while (controls.TryPop(out var item))
                    {
                        //是否是最左边的
                        if (controls.Count == 0)
                        {
                            item.con.Arrange(new(0, totalHeight, lineWidth, lastheight));
                        }
                        else
                        {
                            lineWidth -= item.size.Width;
                            item.con.Arrange(new(lineWidth, totalHeight, item.size.Width, lastheight));
                        }
                    }
                }
                else
                {
                    //设置为最大高度
                    foreach (var (con, size) in controls)
                    {
                        lastheight = Math.Max(size.Height, lastheight);
                    }
                    //一行只有一个，给所有宽度
                    if (controls.Count == 1)
                    {
                        var (con, size) = controls.Pop();
                        con.Arrange(new(0, size.Y, finalSize.Width, lastheight));
                    }
                    else
                    {
                        while (controls.TryPop(out var item))
                        {
                            item.con.Arrange(new(item.size.X, item.size.Y, item.size.Width, lastheight));
                        }
                        //是否是最右边的
                        if (last != null && right)
                        {
                            last.Arrange(new(lineWidth - widthToUse, totalHeight,
                                finalSize.Width - lineWidth + widthToUse, lastheight));
                            last = null;
                        }
                    }
                }

                totalHeight += lastheight;
                if (lineWidth > totalWidth)
                {
                    totalWidth = lineWidth;
                }
                lineWidth = 0;
            }

            //是否是最后一个控件
            if (child == Children[^1])
            {
                //左边控件最大
                if (left)
                {
                    controls.Push((child, new(0, 0, minWidth, lineHeight)));
                    lineWidth = finalSize.Width;
                    //从右往左排序
                    while (controls.TryPop(out var item))
                    {
                        if (controls.Count == 0)
                        {
                            item.con.Arrange(new(0, totalHeight, lineWidth, lineHeight));
                        }
                        else
                        {
                            lineWidth -= item.size.Width;
                            item.con.Arrange(new(lineWidth, totalHeight, item.size.Width, lineHeight));
                        }
                    }
                }
                else
                {
                    //设置这行所有控件最大高度
                    while (controls.TryPop(out var item))
                    {
                        item.con.Arrange(new(item.size.X, item.size.Y, item.size.Width, lineHeight));
                    }

                    //右边控件最大
                    if (right)
                    {
                        child.Arrange(new(lineWidth, totalHeight, finalSize.Width - lineWidth, lineHeight));
                    }
                    //默认排序
                    else
                    {
                        child.Arrange(new(lineWidth, totalHeight, minWidth, lineHeight));
                    }
                }

                totalHeight += lineHeight;
                lineWidth += minWidth;
                if (lineWidth > totalWidth)
                {
                    totalWidth = lineWidth;
                }
            }
            else
            {
                widthToUse = minWidth;
                var size = new Rect(lineWidth, totalHeight, widthToUse, lineHeight);
                child.Arrange(size);
                lineWidth += widthToUse;

                controls.Push((child, size));

                if (!left)
                {
                    last = child;
                }
            }
        }

        if (left || right)
        {
            return finalSize;
        }

        return new(totalWidth, totalHeight);
    }
}
