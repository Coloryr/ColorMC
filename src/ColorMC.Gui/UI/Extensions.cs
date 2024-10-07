using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ColorMC.Gui.Manager;

namespace ColorMC.Gui.UI;

public class ColorsExtension(string key) : MarkupExtension, IObservable<IBrush>
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this.ToBinding();
    }

    public IDisposable Subscribe(IObserver<IBrush> observer)
    {
        return ColorManager.Add(key, observer);
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
        return LangMananger.Add(key, observer);
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
        return ThemeManager.AddFont(observer);
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
        return ThemeManager.AddStyle(key, observer);
    }
}

public class ThemeExtension(string key) : MarkupExtension, IObservable<IBrush>
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this.ToBinding();
    }

    public IDisposable Subscribe(IObserver<IBrush> observer)
    {
        return ThemeManager.Add(key, observer);
    }
}

public class ThemeThickExtension(string key) : MarkupExtension, IObservable<Thickness>
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this.ToBinding();
    }

    public IDisposable Subscribe(IObserver<Thickness> observer)
    {
        return ThemeManager.Add(key, observer);
    }
}

public class UnsubscribeColor(List<WeakReference<IObserver<IBrush>>> observers, IObserver<IBrush> observer) : IDisposable
{
    public void Dispose()
    {
        foreach (var item in observers.ToArray())
        {
            if (!item.TryGetTarget(out var target)
                || target == observer)
            {
                observers.Remove(item);
            }
        }
    }
}
public class UnsubscribeThick(List<WeakReference<IObserver<Thickness>>> observers, IObserver<Thickness> observer) : IDisposable
{
    public void Dispose()
    {
        foreach (var item in observers.ToArray())
        {
            if (!item.TryGetTarget(out var target)
                || target == observer)
            {
                observers.Remove(item);
            }
        }
    }
}

public class UnsubscribeStyle(List<WeakReference<IObserver<object?>>> observers, IObserver<object?> observer) : IDisposable
{
    public void Dispose()
    {
        foreach (var item in observers.ToArray())
        {
            if (!item.TryGetTarget(out var target)
                || target == observer)
            {
                observers.Remove(item);
            }
        }
    }
}

public class UnsubscribeFont(List<WeakReference<IObserver<FontFamily>>> observers, IObserver<FontFamily> observer) : IDisposable
{
    public void Dispose()
    {
        foreach (var item in observers.ToArray())
        {
            if (!item.TryGetTarget(out var target)
                || target == observer)
            {
                observers.Remove(item);
            }
        }
    }
}