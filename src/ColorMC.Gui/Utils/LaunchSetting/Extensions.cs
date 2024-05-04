using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class ColorsExtension(string key) : MarkupExtension, IObservable<IBrush>
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this.ToBinding();
    }

    public IDisposable Subscribe(IObserver<IBrush> observer)
    {
        return ColorSel.Add(key, observer);
    }
}

public class LocalizeExtension(string key) : MarkupExtension, IObservable<string>
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this.ToBinding();
    }

    public IDisposable Subscribe(IObserver<string> observer)
    {
        return LangSel.Add(key, observer);
    }
}

public class FontExtension : MarkupExtension, IObservable<FontFamily>
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this.ToBinding();
    }

    public IDisposable Subscribe(IObserver<FontFamily> observer)
    {
        return FontSel.Add(observer);
    }
}

public class StyleExtension(string key) : MarkupExtension, IObservable<object?>
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this.ToBinding();
    }

    public IDisposable Subscribe(IObserver<object?> observer)
    {
        return StyleSel.Add(key, observer);
    }
}