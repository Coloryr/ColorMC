using Avalonia.Data.Converters;
using ColorMC.Core.Nbt;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class FileIconConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 2 &&
            values[0] is bool isDirectory &&
            values[1] is bool isExpanded)
        {
            if (!isDirectory)
                return "[F]";
            else
                return isExpanded ? "{O}" : "{ }";
        }

        return null;
    }
}

public class NbtIconConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 1 &&
            values[0] is NbtType type)
        {
            return type switch
            {
                NbtType.NbtEnd => "E",
                NbtType.NbtByte => "B",
                NbtType.NbtShort => "S",
                NbtType.NbtInt => "I",
                NbtType.NbtLong => "L",
                NbtType.NbtFloat => "F",
                NbtType.NbtDouble => "D",
                NbtType.NbtByteArray => "[B]",
                NbtType.NbtString => "T",
                NbtType.NbtList => "[ ]",
                NbtType.NbtCompound => "{ }",
                NbtType.NbtIntArray => "[I]",
                NbtType.NbtLongArray => "[L]",
                _ => ""
            };
        }

        return "";
    }
}

public static class IconConverter
{
    static IconConverter()
    {
        FileIconConverter = new FileIconConverter();
        NbtIconConverter = new NbtIconConverter();
    }

    public static IMultiValueConverter FileIconConverter { get; }
    public static IMultiValueConverter NbtIconConverter { get; }
}
