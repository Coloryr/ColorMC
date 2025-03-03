using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ColorMC.Gui.UI.Converters;

public class MinModeToPanelTemplateConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool minMode)
        {
            if (minMode)
            {
                return new FuncTemplate<Panel>(() => new StackPanel());
            }
            else
            {
                return new FuncTemplate<Panel>(() => new WrapPanel());
            }
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
