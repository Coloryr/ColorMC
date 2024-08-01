using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace ColorMC.Gui.UI.Views;

public class WrapPanelWithStretch : Panel
{
    public static readonly StyledProperty<bool> AutoMaxProperty =
        AvaloniaProperty.Register<WrapPanelWithStretch, bool>(nameof(AutoMax));

    public static readonly StyledProperty<bool> LeftMaxProperty =
        AvaloniaProperty.Register<WrapPanelWithStretch, bool>(nameof(LeftMax));

    public bool AutoMax
    {
        get => GetValue(AutoMaxProperty);
        set => SetValue(AutoMaxProperty, value);
    }
    public bool LeftMax
    {
        get => GetValue(LeftMaxProperty);
        set => SetValue(LeftMaxProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == AutoMaxProperty
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
        double lineHeight = 0;

        foreach (var child in Children)
        {
            child.Measure(Size.Infinity);
            var childSize = child.DesiredSize;
            double minWidth = GetWidth(child);

            if (totalWidth + minWidth > availableSize.Width && totalWidth > 0)
            {
                totalHeight += lineHeight;
                totalWidth = 0;
            }

            totalWidth += minWidth;
            lineHeight = Math.Max(lineHeight, childSize.Height);
        }

        totalHeight += lineHeight;

        return new Size(availableSize.Width, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double totalWidth = 0;
        double totalHeight = 0;
        double lineHeight = 0;

        //上一个控件的高度
        double widthToUse = 0;
        Control? last = null;
        bool left = LeftMax;
        bool right = AutoMax;
        var controls = new Stack<(Control con, Rect size)>();

        foreach (var child in Children)
        {
            var childSize = child.DesiredSize;
            //控件占用的宽度
            double minWidth = GetWidth(child);

            lineHeight = Math.Max(lineHeight, childSize.Height);

            //是否需要换行
            if (totalWidth + minWidth > finalSize.Width)
            {
                //最左边的最大
                if (left)
                {
                    totalWidth = finalSize.Width;
                    //从右往左排序
                    while (controls.TryPop(out var item))
                    {
                        //是否是最左边的
                        if (controls.Count == 0)
                        {
                            item.con.Arrange(new(0, totalHeight, totalWidth, lineHeight));
                        }
                        else
                        {
                            totalWidth -= item.size.Width;
                            item.con.Arrange(new(totalWidth, totalHeight, item.size.Width, lineHeight));
                        }
                    }
                }
                else
                {
                    //设置为最大高度
                    while (controls.TryPop(out var item))
                    {
                        item.con.Arrange(new(item.size.X, item.size.Y, item.size.Width, lineHeight));
                    }

                    //是否是最右边的
                    if (last != null && right)
                    {
                        last.Arrange(new(totalWidth - widthToUse, totalHeight, 
                            finalSize.Width - totalWidth + widthToUse, lineHeight));
                        last = null;
                    }
                }

                totalHeight += lineHeight;
                totalWidth = 0;
            }

            //是否是最后一个控件
            if (child == Children[^1])
            {
                //左边控件最大
                if (left)
                {
                    controls.Push((child, new()));
                    totalWidth = finalSize.Width;
                    //从右往左排序
                    while (controls.TryPop(out var item))
                    {
                        if (controls.Count == 0)
                        {
                            item.con.Arrange(new(0, totalHeight, totalWidth, lineHeight));
                        }
                        else
                        {
                            totalWidth -= item.size.Width;
                            item.con.Arrange(new(totalWidth, totalHeight, item.size.Width, lineHeight));
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
                        child.Arrange(new(totalWidth, totalHeight, finalSize.Width - totalWidth, lineHeight));
                    }
                    //默认排序
                    else
                    {
                        child.Arrange(new(totalWidth, totalHeight, minWidth, lineHeight));
                    }
                }
            }
            else
            {
                widthToUse = minWidth;
                var size = new Rect(totalWidth, totalHeight, widthToUse, lineHeight);
                child.Arrange(size);
                totalWidth += widthToUse;

                controls.Push((child, size));

                if (!left)
                {
                    last = child;
                }
            }
        }

        return finalSize;
    }
}
