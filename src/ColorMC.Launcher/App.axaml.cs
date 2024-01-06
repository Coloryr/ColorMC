using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace ColorMC.Launcher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        if (ApplicationLifetime is ClassicDesktopStyleApplicationLifetime life)
        {
            life.MainWindow = new Window1();
        }
    }

    //public static void Reboot()
    //{
    //    if (SystemInfo.Os != OsType.Android)
    //    {
    //        IsClose = true;
    //        Thread.Sleep(500);
    //        Process.Start($"{(SystemInfo.Os == OsType.Windows ?
    //                "ColorMC.Launcher.exe" : "ColorMC.Launcher")}");
    //        //Thread.Sleep(200);
    //        Close();
    //    }
    //}
}
