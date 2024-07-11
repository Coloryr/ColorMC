using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ColorMC.Gui.Utils;

public class WidthToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double width && parameter is string paramString && double.TryParse(paramString, out double threshold))
        {
            return width < threshold ? true : false;
        }
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
