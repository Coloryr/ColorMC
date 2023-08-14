using Avalonia.Data.Converters;
using ColorMC.Core.Nbt;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    private static FileIconConverter? s_fileIconConverter;
    public static IMultiValueConverter FileIconConverter
    {
        get
        {
            s_fileIconConverter ??= new FileIconConverter();
            return s_fileIconConverter;
        }
    }

    private static NbtIconConverter? s_nbtIconConverter;
    public static IMultiValueConverter NbtIconConverter
    {
        get
        {
            s_nbtIconConverter ??= new NbtIconConverter();

            return s_nbtIconConverter;
        }
    }
}
