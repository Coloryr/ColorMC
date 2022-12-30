using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Http.Downloader;
using ColorMC.Gui.UI;
using ColorMC.Gui.UI.Views;
using System;

namespace ColorMC.Gui;

public partial class App : Application
{
    private static IClassicDesktopStyleApplicationLifetime Life;
    public static DownloadWindow? DownloadWindow;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        CoreMain.OnError = ShowError;
        CoreMain.NewStart = ShowNew;
        CoreMain.DownloaderUpdate = DownloaderUpdate;

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

    public void DownloaderUpdate(CoreRunState state)
    {
        if (state == CoreRunState.Init)
        {
            if (DownloadWindow != null)
            {
                DownloadWindow.Activate();
            }
            else
            {
                DownloadWindow = new DownloadWindow();
                DownloadWindow.Show();
            }
        }
        else if (state == CoreRunState.End)
        {
            DownloadWindow?.Close();
        }
    }

    private void Life_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Logs.Stop();
        DownloadManager.Stop();
    }

    public static void ShowMain()
    {
        new MainWindow().Show();
    }

    public static void ShowNew()
    {
        new HelloWindow().Show();
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
