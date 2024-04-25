using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using ColorMC.Core.Nbt;

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
    public static Dictionary<byte, string> InputKeyIcon { get; } = new()
    {
        { 0, "/Resource/Icon/Input/button_a.svg" },
        { 1, "/Resource/Icon/Input/button_b.svg" },
        { 2, "/Resource/Icon/Input/button_x.svg" },
        { 3, "/Resource/Icon/Input/button_y.svg" },
        { 4, "/Resource/Icon/Input/button_menu1.svg" },
        { 6, "/Resource/Icon/Input/button_menu.svg" },
        { 7, "/Resource/Icon/Input/button_l.svg" },
        { 8, "/Resource/Icon/Input/button_r.svg" },
        { 9, "/Resource/Icon/Input/button_lb.svg" },
        { 10, "/Resource/Icon/Input/button_rb.svg" },
        { 11, "/Resource/Icon/Input/button_up.svg" },
        { 12, "/Resource/Icon/Input/button_down.svg" },
        { 13, "/Resource/Icon/Input/button_left.svg" },
        { 14, "/Resource/Icon/Input/button_right.svg" },
    };

    public static Dictionary<byte, string> InputAxisIcon { get; } = new()
    {
        { 1, "/Resource/Icon/Input/button_xz.svg" },
        { 0, "/Resource/Icon/Input/button_xy.svg" },
        { 3, "/Resource/Icon/Input/button_xz.svg" },
        { 2, "/Resource/Icon/Input/button_xy.svg" },
        { 4, "/Resource/Icon/Input/button_lt.svg" },
        { 5, "/Resource/Icon/Input/button_rt.svg" },
    };

    public static string GetInputKeyIcon(byte key)
    {
        if (InputKeyIcon.TryGetValue(key, out var icon))
        {
            return icon;
        }
        else
        {
            return "";
        }
    }

    public static string GetInputAxisIcon(byte key)
    {
        if (InputAxisIcon.TryGetValue(key, out var icon))
        {
            return icon;
        }
        else
        {
            return "";
        }
    }
}
