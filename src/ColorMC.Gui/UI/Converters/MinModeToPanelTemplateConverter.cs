﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;

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