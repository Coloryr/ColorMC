using Avalonia;
using Avalonia.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Launcher;

internal class Program
{
    public const string Font = "resm:ColorMC.Launcher.Resource.MiSans-Normal.ttf?assembly=ColorMC.Launcher#MiSans";

    private static Mutex mutex1;

    public static Updater updater;
    public static string BaseDir { get; private set; }

    public delegate void IN(string[] args);
    public delegate void IN1();
    public delegate Task<bool> IN2(string data);
    public delegate void IN3(Action action);
    public delegate AppBuilder IN4();
    public delegate void IN5(Func<Task<(bool?, string?)>> action);

    public static IN1 CheckFailCall;
    public static IN1 Quit;
    public static IN3 SetInit;
    public static IN2 HaveUpdate;
    public static IN4 BuildApp;
    public static IN MainCall;
    public static IN5 SetCheck;
    public static IN3 SetUpdate;

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

        var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            BaseDir = AppContext.BaseDirectory;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            BaseDir = $"{AppContext.BaseDirectory}Contents/MacOS/";

            if (!Directory.Exists(BaseDir))
            {
                BaseDir = AppContext.BaseDirectory;
            }
        }
        else
        {
            BaseDir = AppContext.BaseDirectory;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var dll = BaseDir + "OpenAL32.dll";
            if (!File.Exists(dll))
            {
                File.WriteAllBytes(dll, Resource1.OpenAL32);
            }
        }

        Console.WriteLine($"BaseDir:{BaseDir}");

        updater = new();

        if (!File.Exists($"{BaseDir}ColorMC.Core.dll")
            || !File.Exists($"{BaseDir}ColorMC.Core.pdb")
            || !File.Exists($"{BaseDir}ColorMC.Gui.dll")
            || !File.Exists($"{BaseDir}ColorMC.Gui.pdb"))
        {
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

            var app = AppBuilder.Configure<App>()
                 .With(new FontManagerOptions
                 {
                     DefaultFamilyName = Font,
                 })
                .UsePlatformDetect()
                .LogToTrace()
                //.With(new X11PlatformOptions()
                //{
                //    UseCompositor = false
                //})
                .StartWithClassicDesktopLifetime(args);
            return;
        }
        try
        {
            Load();

            SetInit(Init);
            SetCheck(updater.CheckOne);
            SetUpdate(updater.StartUpdate);

            MainCall(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void Load()
    {
#if DEBUG
        BaseDir = AppContext.BaseDirectory;
#endif

        AssemblyLoadContext context = new("ColorMC");
        {
            using var file = File.OpenRead($"{BaseDir}ColorMC.Gui.dll");
            using var file1 = File.OpenRead($"{BaseDir}ColorMC.Gui.pdb");
            context.LoadFromStream(file, file1);
        }
        {
            using var file = File.OpenRead($"{BaseDir}ColorMC.Core.dll");
            using var file1 = File.OpenRead($"{BaseDir}ColorMC.Core.pdb");
            context.LoadFromStream(file, file1);
        }
        var item = context.Assemblies
                       .Where(x => x.GetName().Name == "ColorMC.Gui")
                       .First();

        var mis = item.GetTypes().Where(x => x.FullName == "ColorMC.Gui.ColorMCGui").First();

        MainCall = (Delegate.CreateDelegate(typeof(IN),
                mis.GetMethod("Main")!) as IN)!;

        CheckFailCall = (Delegate.CreateDelegate(typeof(IN1),
            mis.GetMethod("CheckUpdateFail")!) as IN1)!;

        Quit = (Delegate.CreateDelegate(typeof(IN1),
            mis.GetMethod("Quit")!) as IN1)!;

        SetInit = (Delegate.CreateDelegate(typeof(IN3),
            mis.GetMethod("SetInit")!) as IN3)!;

        HaveUpdate = (Delegate.CreateDelegate(typeof(IN2),
            mis.GetMethod("HaveUpdate")!) as IN2)!;

        BuildApp = (Delegate.CreateDelegate(typeof(IN4),
            mis.GetMethod("BuildAvaloniaApp")!) as IN4)!;

        SetCheck = (Delegate.CreateDelegate(typeof(IN5),
            mis.GetMethod("SetCheck")!) as IN5)!;

        SetUpdate = (Delegate.CreateDelegate(typeof(IN3),
            mis.GetMethod("SetUpdate")!) as IN3)!;
    }

    private static void Init()
    {
        updater.Check();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Load();
        return BuildApp();
    }

    public static void Launch()
    {
        try
        {
            Process.Start($"{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            "ColorMC.Launcher.exe" : "ColorMC.Launcher")}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
