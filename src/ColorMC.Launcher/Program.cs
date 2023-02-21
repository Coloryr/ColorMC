using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ColorMC.Core.Utils;
using ColorMC.Gui;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Launcher;

internal class Program
{
    public const string Font = "resm:ColorMC.Launcher.Resource.MiSans-Normal.ttf?assembly=ColorMC.Launcher#MiSans";

    private static Mutex mutex1;

    public static Updater updater = new();

    public delegate void IN(string[] args);
    public delegate void IN1();
    public delegate Task<bool> IN2();
    public delegate void IN3(Action action);

    public static IN1 CheckFailCall;
    public static IN1 Quit;
    public static IN3 SetInit;
    public static IN2 HaveUpdate;

    public static Semaphore semaphore = new(0, 2);

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        //Console.WriteLine("wait");
        //while (!Debugger.IsAttached)
        //{
        //    Thread.Sleep(100);
        //}

        try
        {
            bool temp;
            do
            {
                mutex1 = new Mutex(true, "ColorMC-Launcher", out temp);
                if (temp)
                    break;

                mutex1.WaitOne();
            }
            while (!temp);
        }
        catch
        { 
        
        }

        if (!File.Exists("ColorMC.Gui.dll"))
        {
            var app = BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            return;
        }

        try
        {
            AssemblyLoadContext context = new("ColorMC");
            {
                using var file = File.OpenRead("ColorMC.Gui.dll");
                var temp = context.LoadFromStream(file);
            }
            {
                using var file = File.OpenRead("ColorMC.Core.dll");
                var temp = context.LoadFromStream(file);
            }
            var item = context.Assemblies
                           .Where(x => x.GetName().Name == "ColorMC.Gui")
                           .First();

            var mis = item.GetTypes().Where(x => x.FullName == "ColorMC.Gui.ProgramGui").First();

            var temp1 = Delegate.CreateDelegate(typeof(IN), 
                mis.GetMethod("Main")!) as IN;

            CheckFailCall = (Delegate.CreateDelegate(typeof(IN1),
                mis.GetMethod("CheckUpdateFail")!) as IN1)!;

            Quit = (Delegate.CreateDelegate(typeof(IN1),
                mis.GetMethod("Quit")!) as IN1)!;

            SetInit = (Delegate.CreateDelegate(typeof(IN3),
                mis.GetMethod("SetInit")!) as IN3)!;

            HaveUpdate = (Delegate.CreateDelegate(typeof(IN2),
                mis.GetMethod("HaveUpdate")!) as IN2)!;

            SetInit(Init);

            updater.Check();

            temp1!(args);

            mutex1.ReleaseMutex();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void Init()
    {
        semaphore.Release();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
             .With(new FontManagerOptions
             {
                 DefaultFamilyName = Font,
             })
            .UsePlatformDetect()
            .LogToTrace();
}
