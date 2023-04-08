using Avalonia;
using Avalonia.Media;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui;

public class ColorMCGui
{
    public const string Version = "A16.230408";

    public static string RunDir { get; private set; }
    public static Action InitDone { get; private set; }
    public static Func<Task<(bool?, string?)>> Check { get; private set; }
    public static Action Update { get; private set; }

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

            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            RunDir = SystemInfo.Os switch
            {
                OsType.Linux => $"{path}/ColorMC/",
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

    public static void SetInit(Action ac)
    {
        InitDone = ac;
    }

    public static Task<bool> HaveUpdate(string data)
    {
        return App.HaveUpdate(data);
    }

    public static void CheckUpdateFail()
    {
        if (App.MainWindow == null)
            return;

        App.MainWindow.Window.Info1.Show(App.GetLanguage("Error13"));
    }

    public static void Quit()
    {
        App.Close();
    }

    public static void SetCheck(Func<Task<(bool?, string?)>> action)
    {
        Check = action;
    }

    public static void SetUpdate(Action action)
    {
        Update = action;
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