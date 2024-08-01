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

            child.Measure(new Size(minWidth, double.PositiveInfinity));
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

        double widthToUse = 0;
        Control? last = null;
        bool left = LeftMax;
        var size = new Rect();
        var controls = new Stack<(Control con, Rect size)>();

        foreach (var child in Children)
        {
            var childSize = child.DesiredSize;
            double minWidth = GetWidth(child);

            lineHeight = Math.Max(lineHeight, childSize.Height);

            if (totalWidth + minWidth > finalSize.Width)
            {
                if (left)
                {
                    totalWidth = finalSize.Width;
                    while (controls.TryPop(out var item))
                    {
                        minWidth = GetWidth(child);

                        if (controls.Count == 0)
                        {
                            size = new Rect(0, totalHeight, totalWidth, lineHeight);
                            item.con.Arrange(size);
                        }
                        else
                        {
                            totalWidth -= minWidth;
                            size = new Rect(totalWidth, totalHeight, minWidth, lineHeight);
                            item.con.Arrange(size);
                        }
                    }
                }
                else
                {
                    while (controls.TryPop(out var item))
                    {
                        item.con.Arrange(new(item.size.X, item.size.Y, item.size.Width, lineHeight));
                    }

                    if (last != null && AutoMax)
                    {
                        last.Arrange(new Rect(totalWidth - widthToUse, totalHeight, 
                            finalSize.Width - totalWidth + widthToUse, lineHeight));
                        last = null;
                    }
                }
                totalHeight += lineHeight;
                totalWidth = 0;
            }

            if (child == Children[^1])
            {
                if (left)
                {
                    controls.Push((child, new()));
                    totalWidth = finalSize.Width;
                    while (controls.TryPop(out var item))
                    {
                        minWidth = GetWidth(child);
                        if (controls.Count == 0)
                        {
                            size = new Rect(0, totalHeight,
                            totalWidth, child.DesiredSize.Height);
                            item.con.Arrange(size);
                        }
                        else
                        {
                            totalWidth -= minWidth;
                            size = new Rect(totalWidth, totalHeight, minWidth, lineHeight);
                            item.con.Arrange(size);
                        }
                    }
                    
                }
                else if (AutoMax)
                {
                    size = new Rect(totalWidth, totalHeight, finalSize.Width - totalWidth, lineHeight);
                    child.Arrange(size);
                }
                else
                {
                    widthToUse = minWidth;
                    size = new Rect(totalWidth, totalHeight, widthToUse, lineHeight);
                    child.Arrange(size);
                }
            }
            else
            {
                widthToUse = minWidth;
                size = new Rect(totalWidth, totalHeight, widthToUse, lineHeight);
                child.Arrange(size);
                totalWidth += widthToUse;
            }

            controls.Push((child, size));

            if (!left)
            {
                last = child;
            }
        }

        return finalSize;
    }
}
