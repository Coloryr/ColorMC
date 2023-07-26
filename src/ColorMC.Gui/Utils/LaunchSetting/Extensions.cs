using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;

namespace ColorMC.Gui.Utils.LaunchSetting;

public static class Indexer
{
    public const string IndexerName = "Item";
    public const string IndexerArrayName = "Item[]";
}

public class ColorsExtension : MarkupExtension
{
    public ColorsExtension(string key)
    {
        Key = key;
    }

    public string Key { get; set; }


    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var keyToUse = Key;

        var binding = new ReflectionBindingExtension($"[{keyToUse}]")
        {
            Mode = BindingMode.OneWay,
            Source = ColorSel.Instance,
        };

        return binding.ProvideValue(serviceProvider);
    }
}

public class LocalizeExtension : MarkupExtension
{
    public LocalizeExtension(string key)
    {
        Key = key;
    }

    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var keyToUse = Key;

        var binding = new ReflectionBindingExtension($"[{keyToUse}]")
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

public class StyleExtension : MarkupExtension
{
    public StyleExtension(string key)
    {
        Key = key;
    }

    public string Key { get; set; }
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var keyToUse = Key;
        var binding = new ReflectionBindingExtension($"[{keyToUse}]")
        {
            Mode = BindingMode.OneWay,
            Source = StyleSel.Instance,
        };

        return binding.ProvideValue(serviceProvider);
    }
}