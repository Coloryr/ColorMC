using Avalonia;
using Avalonia.Media;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using System;
using System.Text;

namespace ColorMC.Gui;

public static class ColorMCGui
{
    public static string RunDir { get; private set; }

    public static string[] BaseSha1 { get; private set; }

    public static RunType RunType { get; private set; } = RunType.AppBuilder;

    public const string Font = "resm:ColorMC.Launcher.Resources.MiSans-Normal.ttf?assembly=ColorMC.Launcher#MiSans";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        try
        {
            SystemInfo.Init();

            RunType = RunType.Program;

            RunDir = SystemInfo.Os switch
            {
                OsType.Linux => $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/ColorMC/",
                OsType.MacOS => "/Users/shared/ColorMC/",
                _ => AppContext.BaseDirectory
            };

            Console.WriteLine($"RunDir:{RunDir}");

            ColorMCCore.Init(RunDir);

            BuildAvaloniaApp()
                 .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Logs.Error("run fail", e);
            App.Close();
        }
    }

    public static void SetBaseSha1(string[] data)
    {
        BaseSha1 = data;
    }

    public static AppBuilder BuildAvaloniaApp()
    {
#if DEBUG
        RunDir = AppContext.BaseDirectory;
#endif

        GuiConfigUtils.Init(RunDir);
        ImageTemp.Init(RunDir);

        var config = GuiConfigUtils.Config.Render.Windows;
        var opt = new Win32PlatformOptions();
        if (config.UseWindowsUIComposition != null)
        {
            opt.UseWindowsUIComposition = config.UseWindowsUIComposition == true;
        }
        if (config.UseWgl != null)
        {
            opt.UseWgl = config.UseWgl == true;
        }
        if (config.AllowEglInitialization != null)
        {
            opt.AllowEglInitialization = config.AllowEglInitialization == true;
        }

        var config1 = GuiConfigUtils.Config.Render.X11;
        var opt1 = new X11PlatformOptions();
        if (config1.UseEGL != null)
        {
            opt1.UseEGL = config1.UseEGL == true;
        }
        if (config1.UseGpu != null)
        {
            opt1.UseGpu = config1.UseGpu == true;
        }
        if (config1.OverlayPopups != null)
        {
            opt1.OverlayPopups = config1.OverlayPopups == true;
        }

        return AppBuilder.Configure<App>()
            .With(new FontManagerOptions
            {
                DefaultFamilyName = Font,
            })
            .With(opt)
            .With(opt1)
            .LogToTrace()
            .UsePlatformDetect();
    }
}