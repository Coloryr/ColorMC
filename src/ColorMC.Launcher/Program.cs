//#define AOT

using Avalonia;
using ColorMC.Gui;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace ColorMC.Launcher;

internal static class GuiLoad
{
    public static void Load()
    {
        Program.MainCall = ColorMCGui.Main;
        Program.BuildApp = ColorMCGui.BuildAvaloniaApp;
        Program.SetBaseSha1 = ColorMCGui.SetBaseSha1;
    }

    public static void Run(string[] args)
    {
        ColorMCGui.SetBaseSha1(Program.BaseSha1);
        ColorMCGui.Main(args);
    }
}

public static class Program
{
    public const string TopVersion = "A20";

    public static readonly string[] BaseSha1 = new[]
    {
        "6a96a1fe0e5568b79bc5a92f51023686f025615b",
        "7b875a40bd534b52f44dd99d971c0bf15ff88d7d",
        "b3274e2b5c6bb50ffb2c7b5787c2274870cfff6f",
        "0158dca9e18a116ce484f91d9424dd3918d3d1a0"
    };
    /// <summary>
    /// 加载路径
    /// </summary>
    public static string LoadDir { get; private set; } = AppContext.BaseDirectory;

    public delegate void IN(string[] args);
    public delegate AppBuilder IN1();

    public static IN MainCall { get; set; }
    public static IN1 BuildApp { get; set; }
    public static IN SetBaseSha1 { get; set; }

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

#if AOT || DEBUG
        GuiLoad.Run(args);
#else
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
#endif
    }

    public static AppBuilder BuildAvaloniaApp()
    {
#if AOT
        return ColorMCGui.BuildAvaloniaApp();
#else
        Load();
        return BuildApp();
#endif
    }

#if !AOT
    private static bool NotHaveDll()
    {
        return !File.Exists($"{LoadDir}ColorMC.Core.dll")
            || !File.Exists($"{LoadDir}ColorMC.Core.pdb")
            || !File.Exists($"{LoadDir}ColorMC.Gui.dll")
            || !File.Exists($"{LoadDir}ColorMC.Gui.pdb");
    }

    private static void Load()
    {
        if (NotHaveDll())
        {
            GuiLoad.Load();
            return;
        }
        else
        {
            var context = new AssemblyLoadContext("ColorMC", true);
            {
                using var file = File.OpenRead($"{LoadDir}ColorMC.Core.dll");
                using var file1 = File.OpenRead($"{LoadDir}ColorMC.Core.pdb");
                context.LoadFromStream(file, file1);
            }
            {
                using var file = File.OpenRead($"{LoadDir}ColorMC.Gui.dll");
                using var file1 = File.OpenRead($"{LoadDir}ColorMC.Gui.pdb");
                context.LoadFromStream(file, file1);
            }

            var item = context.Assemblies
                                .Where(x => x.GetName().Name == "ColorMC.Core")
                                .ToList()[0];

            var mis = item.GetTypes().Where(x => x.FullName == "ColorMC.Core.ColorMCCore").ToList()[0];

            var temp = mis.GetField("Version");
            var version = temp?.GetValue(null) as string;
            if (version?.StartsWith(TopVersion) != true)
            {
                context.Unload();
                GuiLoad.Load();

                var name = $"{LoadDir}ColorMC.Gui.dll";
                if (File.Exists(name))
                    File.Delete(name);
                name = $"{LoadDir}ColorMC.Gui.pdb";
                if (File.Exists(name))
                    File.Delete(name);
                name = $"{LoadDir}ColorMC.Core.dll";
                if (File.Exists(name))
                    File.Delete(name);
                name = $"{LoadDir}ColorMC.Core.pdb";
                if (File.Exists(name))
                    File.Delete(name);

                return;
            }

            var item1 = context.Assemblies
                           .Where(x => x.GetName().Name == "ColorMC.Gui")
                           .ToList()[0];

            var mis1 = item1.GetTypes().Where(x => x.FullName == "ColorMC.Gui.ColorMCGui").ToList()[0];

            MainCall = (Delegate.CreateDelegate(typeof(IN),
                    mis1.GetMethod("Main")!) as IN)!;

            BuildApp = (Delegate.CreateDelegate(typeof(IN1),
                    mis1.GetMethod("BuildAvaloniaApp")!) as IN1)!;

            SetBaseSha1 = (Delegate.CreateDelegate(typeof(IN),
                    mis1.GetMethod("SetBaseSha1")!) as IN)!;
        }
    }
#endif
}
