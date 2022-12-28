using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ColorMC.Core;
using ColorMC.Core.Http.Downloader;
using ColorMC.Gui.UI;
using ColorMC.Gui.UI.Views;
using System;

namespace ColorMC.Gui;

public partial class App : Application
{
    private static IClassicDesktopStyleApplicationLifetime Life;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        CoreMain.OnError = ShowError;
        CoreMain.NewStart = ShowNew;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Life = desktop;
            desktop.MainWindow = new InitWindow();
        }

        base.OnFrameworkInitializationCompleted();

        if (Life != null)
        {
            Life.Exit += Life_Exit;
        }
    }

    private void Life_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Logs.Stop();
        DownloadManager.Stop();
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
