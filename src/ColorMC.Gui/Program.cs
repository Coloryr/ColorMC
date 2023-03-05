using Avalonia;
using Avalonia.Media;
using ColorMC.Core;
using ColorMC.Core.Utils;
using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui;

public class ColorMCGui
{
    public const string Version = "A14:230303";

    public static string BaseDir { get; private set; }
    public static Action InitDone { get; private set; }
    public static Func<Task<(bool?, string?)>> Check { get; private set; }
    public static Action Update { get; private set; }

    public const string Font = "resm:ColorMC.Launcher.Resource.MiSans-Normal.ttf?assembly=ColorMC.Launcher#MiSans";

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

            BaseDir = SystemInfo.Os == OsType.Linux ? $"{path}/ColorMC/" : AppContext.BaseDirectory;

            ColorMCCore.Init(BaseDir);

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
        if (App.MainWindow == null)
            return Task.FromResult(false);

        return App.MainWindow.Info6.ShowWait(App.GetLanguage("Info5"), data);
    }

    public static void CheckUpdateFail()
    {
        if (App.MainWindow == null)
            return;

        App.MainWindow.Info1.Show(App.GetLanguage("Error13"));
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
        GuiConfigUtils.Init(BaseDir);

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
        if (config.UseCompositor != null)
        {
            opt.UseCompositor = config.UseCompositor == true;
        }
        if (config.UseDeferredRendering != null)
        {
            opt.UseDeferredRendering = config.UseDeferredRendering == true;
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
        if (config1.UseDeferredRendering != null)
        {
            opt1.UseDeferredRendering = config1.UseDeferredRendering == true;
        }
        if (config1.UseCompositor != null)
        {
            opt1.UseCompositor = config1.UseCompositor == true;
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