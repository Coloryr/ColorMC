using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ColorMC.Core;
using ColorMC.UI;
using System;

namespace ColorMC;

public partial class App : Application
{
    private static IClassicDesktopStyleApplicationLifetime Life;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Life = desktop;
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();

        CoreMain.OnError = ShowError;
        CoreMain.NewStart = ShowNew;
    }

    public static void ShowNew()
    {
        new HelloWindow().Set();
    }

    private static void ShowError(string data, Exception e, bool close)
    {
        new ErrorWindow().Show(data, e, close);
    }

    public static void Close()
    {
        Life?.Shutdown();
    }
}
