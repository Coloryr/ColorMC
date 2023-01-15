using Avalonia;
using Avalonia.Media;
using System;

namespace ColorMC.Gui;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        //Console.WriteLine("wait");
        //while (!Debugger.IsAttached)
        //{
        //    Thread.Sleep(100);
        //}

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .With(new FontManagerOptions
            {
                DefaultFamilyName = "avares://ColorMC.Gui/Resource/Font/SourceHanSansHWSC-Regular.otf"
            })
            .UsePlatformDetect()
            .LogToTrace();
}
