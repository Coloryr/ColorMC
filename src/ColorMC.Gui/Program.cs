using Avalonia;
using Avalonia.Media;
using System;
using System.Text;

namespace ColorMC.Gui;

internal class Program
{
    public const string Font = "avares://ColorMC.Gui/Resource/Font/SourceHanSansHWSC-Regular.otf";
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

        AppBuilder.Configure<App>()
            .With(new FontManagerOptions
            {
                DefaultFamilyName = Font,
            })
            .LogToTrace()
            .UsePlatformDetect()
            .StartWithClassicDesktopLifetime(args);
    }
}