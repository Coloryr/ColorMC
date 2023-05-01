using Avalonia;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace ColorMC.Launcher;

public static class Program
{
    public const string TopVersion = "A18";

    public const string Font = "resm:ColorMC.Launcher.Resources.MiSans-Normal.ttf?assembly=ColorMC.Launcher#MiSans";
    public static readonly string[] BaseSha1 = new[]
    {
        "4eee85b75ec69f6be4a9bb78a2a956eee62e0c45",
        "670eb3e32e5bf589423f9ce9d6cab40e5dd621b7",
        "ca0e1a854b98c46e54dfb201b79291d1e69a79ee",
        "f2ff5f04336e9707d267a4b2a2652ad1c4f61a08"
    };
    /// <summary>
    /// 加载路径
    /// </summary>
    public static string LoadDir { get; private set; } = AppContext.BaseDirectory;

    public delegate void IN(string[] args);
    public delegate AppBuilder IN1();

    public static IN MainCall;
    public static IN1 BuildApp;
    public static IN SetBaseSha1;

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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            LoadDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/ColorMC/dll/";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            LoadDir = "/Users/shared/ColorMC/dll/";
        }
        else
        {
            LoadDir = AppContext.BaseDirectory + "dll/";
        }

        Console.WriteLine($"CheckDir:{LoadDir}");

        Load();

        try
        {
            SetBaseSha1(BaseSha1);
            MainCall(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        Load();
        return BuildApp();
    }

    private static bool NotHaveDll()
    {
        return !File.Exists($"{LoadDir}ColorMC.Core.dll")
            || !File.Exists($"{LoadDir}ColorMC.Core.pdb")
            || !File.Exists($"{LoadDir}ColorMC.Gui.dll")
            || !File.Exists($"{LoadDir}ColorMC.Gui.pdb");
    }

    private static Assembly Gui;
    private static Assembly Core;

    private class ColorMCAssembly : AssemblyLoadContext
    {
        public ColorMCAssembly(string name, bool unload) : base(name, unload) 
        {
            Resolving += ColorMCAssembly_Resolving;
        }

        private Assembly? ColorMCAssembly_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            if (arg2.FullName == "ColorMC.Gui")
            {
                return Gui;
            }
            else if (arg2.FullName == "ColorMC.Core")
            {
                return Core;
            }

            return null;
        }
    }

    private static void Load()
    {
        //加载DLL
        AssemblyLoadContext context;
        if (NotHaveDll())
        {
            context = AssemblyLoadContext.Default;
            TestLoad.Load1();
        }
        else
        {
            context = new("ColorMC", true);
            {
                using var file = File.OpenRead($"{LoadDir}ColorMC.Gui.dll");
                using var file1 = File.OpenRead($"{LoadDir}ColorMC.Gui.pdb");
                Gui = context.LoadFromStream(file, file1);
            }
            {
                using var file = File.OpenRead($"{LoadDir}ColorMC.Core.dll");
                using var file1 = File.OpenRead($"{LoadDir}ColorMC.Core.pdb");
                Core = context.LoadFromStream(file, file1);
            }

            var item = context.Assemblies
                                .Where(x => x.GetName().Name == "ColorMC.Core")
                                .First();

            var mis = item.GetTypes().Where(x => x.FullName == "ColorMC.Core.ColorMCCore").First();

            var temp = mis.GetField("Version");
            var version = temp?.GetValue(null) as string;
            if (version?.StartsWith(TopVersion) != true)
            {
                context.Unload();
                context = AssemblyLoadContext.Default;
            }

            var item1 = context.Assemblies
                           .Where(x => x.GetName().Name == "ColorMC.Gui")
                           .First();

            var mis1 = item1.GetTypes().Where(x => x.FullName == "ColorMC.Gui.ColorMCGui").First();

            MainCall = (Delegate.CreateDelegate(typeof(IN),
                    mis1.GetMethod("Main")!) as IN)!;

            BuildApp = (Delegate.CreateDelegate(typeof(IN1),
                    mis1.GetMethod("BuildAvaloniaApp")!) as IN1)!;

            SetBaseSha1 = (Delegate.CreateDelegate(typeof(IN),
                    mis1.GetMethod("SetBaseSha1")!) as IN)!;
        }
    }
}
