using System;
using Avalonia;
using Avalonia.Controls;

namespace ColorMC.Gui.UI.Views;

public class WrapPanelWithStretch : Panel
{
    public static readonly DirectProperty<WrapPanelWithStretch, bool> AutoMaxProperty =
        AvaloniaProperty.RegisterDirect<WrapPanelWithStretch, bool>(
            nameof(AutoMax),
            panel => panel.AutoMax,
            (panel, value) => panel.AutoMax = value);

    private bool _autoMax;

    public bool AutoMax
    {
        get => _autoMax;
        set
        {
            SetAndRaise(AutoMaxProperty, ref _autoMax, value);
            InvalidateMeasure();
            InvalidateArrange();
        }
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
            double minWidth = child.MinWidth;
            if (minWidth == 0)
            {
                minWidth = child.Width;
            }
            else if (minWidth == 0)
            {
                minWidth = childSize.Width;
            }

            if (totalWidth + minWidth > availableSize.Width && totalWidth > 0)
            {
                totalHeight += lineHeight;
                totalWidth = 0;
            }

            child.Measure(new Size(Math.Max(childSize.Width, minWidth), double.PositiveInfinity));
            totalWidth += Math.Max(childSize.Width, minWidth);
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

        foreach (var child in Children)
        {
            var childSize = child.DesiredSize;
            double minWidth = child.MinWidth;
            if (minWidth == 0)
            {
                minWidth = child.Width;
            }
            else if (minWidth == 0)
            {
                minWidth = childSize.Width;
            }

            if (totalWidth + minWidth > finalSize.Width)
            {
                if (last != null && AutoMax)
                {
                    last.Arrange(new Rect(totalWidth - widthToUse, totalHeight, finalSize.Width - totalWidth + widthToUse, childSize.Height));
                    last = null;
                }
                totalHeight += lineHeight;
                totalWidth = 0;
            }

            if (child == Children[^1] && AutoMax)
            {
                child.Arrange(new Rect(totalWidth, totalHeight, finalSize.Width - totalWidth, childSize.Height));
            }
            else
            {
                widthToUse = Math.Max(minWidth, childSize.Width);
                child.Arrange(new Rect(totalWidth, totalHeight, widthToUse, childSize.Height));
                totalWidth += widthToUse;
            }

            lineHeight = Math.Max(lineHeight, childSize.Height);
            last = child;
        }

        return finalSize;
    }
}
