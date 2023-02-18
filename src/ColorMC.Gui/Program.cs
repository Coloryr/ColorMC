using Avalonia;
using Avalonia.Media;
using ColorMC.Core;
using System;
using System.Text;

namespace ColorMC.Gui;

public class Program
{
    public const string Font = "avares://ColorMC.Gui/Resource/Font/MiSans-Normal.ttf#MiSans";
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        //Console.WriteLine("wait");
        //while (!Debugger.IsAttached)
        //{
        //    Thread.Sleep(100);
        //}

        CoreMain.Init(AppContext.BaseDirectory);

        BuildAvaloniaApp()
             .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        GuiConfigUtils.Init(AppContext.BaseDirectory);

        var config = GuiConfigUtils.Config.Render.Windows;

        return AppBuilder.Configure<App>()
            .With(new FontManagerOptions
            {
                DefaultFamilyName = Font,
            })
            .With(new Win32PlatformOptions()
            {
                UseCompositor = config.UseCompositor,
                UseWgl = config.UseWgl,
                UseWindowsUIComposition = config.UseWindowsUIComposition,
                UseDeferredRendering = config.UseDeferredRendering
            })
            .With(new X11PlatformOptions()
            {
                UseGpu = false
            })
            .LogToTrace()
            .UsePlatformDetect();
    }
}