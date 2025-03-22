using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ColorMC.Gui.UI.Converters;

/// <summary>
/// 数字转换器
/// </summary>
public class NumberConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (targetType == typeof(int?))
        {
            if (int.TryParse(str, out var value1))
            {
                return value1;
            }

            return 0;
        }
        else if (targetType == typeof(ushort?))
        {
            if (ushort.TryParse(str, out var value1))
            {
                return value1;
            }

            return ushort.MinValue;
        }

        return AvaloniaProperty.UnsetValue;
    }
}
