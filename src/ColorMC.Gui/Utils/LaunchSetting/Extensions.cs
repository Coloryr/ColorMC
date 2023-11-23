using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using System;
using Tmds.DBus.Protocol;

namespace ColorMC.Gui.Utils.LaunchSetting;

public static class Indexer
{
    public const string IndexerName = "Item";
    public const string IndexerArrayName = "Item[]";
}

public class ColorsExtension(string key) : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new ReflectionBindingExtension($"[{key}]")
        {
            Mode = BindingMode.OneWay,
            Source = ColorSel.Instance,
        };

        return binding.ProvideValue(serviceProvider);
    }
}

public class LocalizeExtension(string key) : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new ReflectionBindingExtension($"[{key}]")
        {
            Mode = BindingMode.OneWay,
            Source = Localizer.Instance,
        };

        return binding.ProvideValue(serviceProvider);
    }
}

public class FontExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new ReflectionBindingExtension("[Font]")
        {
            Mode = BindingMode.OneWay,
            Source = FontSel.Instance,
        };
        return binding.ProvideValue(serviceProvider);
    }
}

public class StyleExtension(string key) : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new ReflectionBindingExtension($"[{key}]")
        {
            Mode = BindingMode.OneWay,
            Source = StyleSel.Instance,
        };
        return binding.ProvideValue(serviceProvider);
    }
}