using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using ColorMC.Core;
using ColorMC.Gui.UI;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui;

public partial class App : Application
{
    private static IClassicDesktopStyleApplicationLifetime Life;
    public static DownloadWindow? DownloadWindow;
    public static UserWindow? UserWindow;
    public static MainWindow? MainWindow;
    public static HelloWindow? HelloWindow;
    public static AddGameWindow? AddGameWindow;
    public static AddCurseForgeWindow? AddCurseForgeWindow;

    public static Bitmap? BackBitmap;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
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

        BaseBinding.Init();
    }

    public static void DownloaderUpdate(CoreRunState state)
    {
        if (state == CoreRunState.Init)
        {
            ShowDownload();
        }
        else if (state == CoreRunState.Start)
        {
            DownloadWindow?.Start();
        }
        else if (state == CoreRunState.End)
        {
            DownloadWindow?.Close();
        }
    }

    private void Life_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        BaseBinding.Exit();
    }

    public static void ShowAddGame()
    {
        if (AddGameWindow != null)
        {
            AddGameWindow.Activate();
        }
        else
        {
            AddGameWindow = new();
            AddGameWindow.Show();
        }
    }

    public static void ShowDownload()
    {
        if (DownloadWindow != null)
        {
            DownloadWindow.Activate();
        }
        else
        {
            DownloadWindow = new();
            DownloadWindow.Show();
        }
    }

    public static void ShowUser(bool add)
    {
        if (UserWindow != null)
        {
            UserWindow.Activate();
        }
        else
        {
            UserWindow = new();
            UserWindow.Show();
        }

        if (add)
            UserWindow.SetAdd();
    }

    public static void ShowMain()
    {
        if (MainWindow != null)
        {
            MainWindow.Activate();
        }
        else
        {
            MainWindow = new();
            MainWindow.Show();
        }
    }

    public static void ShowNew()
    {
        if (HelloWindow != null)
        {
            HelloWindow.Activate();
        }
        else
        {
            HelloWindow = new();
            HelloWindow.Show();
        }
    }

    public static void ShowCurseForge()
    {
        if (AddCurseForgeWindow != null)
        {
            AddCurseForgeWindow.Activate();
        }
        else
        {
            AddCurseForgeWindow = new();
            AddCurseForgeWindow.Show();
        }
    }

    public static void ShowError(string data, Exception e, bool close)
    {
        new ErrorWindow().Show(data, e, close);
    }

    public static void Close()
    {
        Life?.Shutdown();
    }
}
