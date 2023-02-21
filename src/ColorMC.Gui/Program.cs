using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui;

public class ProgramGui
{
    public static Action InitDone;
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
            CoreMain.Init(AppContext.BaseDirectory);

            BuildAvaloniaApp()
                 .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Logs.Error("run fail", e);
        }
    }

    public static void SetInit(Action ac)
    {
        InitDone = ac;
    }

    public static Task<bool> HaveUpdate()
    {
        return App.MainWindow.Info.ShowWait(App.GetLanguage("Info5"));
    }
    
    public static void CheckUpdateFail()
    {
        App.MainWindow.Info1.Show(App.GetLanguage("Error13"));
    }

    public static void Quit()
    {
        App.MainWindow.Close();
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        GuiConfigUtils.Init(AppContext.BaseDirectory);

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