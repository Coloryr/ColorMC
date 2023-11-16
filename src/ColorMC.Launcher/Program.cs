using Avalonia;
using ColorMC.Core.Helpers;
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
        Program.SetAot = ColorMCGui.SetAot;
    }

    public static void Run(string[] args)
    {
        ColorMCGui.SetBaseSha1(Program.BaseSha1);
        ColorMCGui.Main(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return ColorMCGui.BuildAvaloniaApp();
    }
}

public static class Program
{
    public const string TopVersion = "A22";

    public static readonly string[] BaseSha1 = new[]
    {
        "",
        "",
        "",
        ""
    };
    /// <summary>
    /// 加载路径
    /// </summary>
    public static string LoadDir { get; private set; } = AppContext.BaseDirectory;

    public delegate void IN(string[] args);
    public delegate void IN2(bool aot);
    public delegate AppBuilder IN1();

    public static IN MainCall { get; set; }
    public static IN1 BuildApp { get; set; }
    public static IN SetBaseSha1 { get; set; }
    public static IN2 SetAot { get; set; }

    public static bool Aot { get; set; }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
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
        try
        {
            Load();
            SetAot(Aot);
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
#if DEBUG
        return GuiLoad.BuildAvaloniaApp();
#else
        Load();
        return BuildApp();
#endif
    }

    private static bool NotHaveDll()
    {
        return File.Exists($"{LoadDir}ColorMC.Core.dll")
            && File.Exists($"{LoadDir}ColorMC.Core.pdb")
            && File.Exists($"{LoadDir}ColorMC.Gui.dll")
            && File.Exists($"{LoadDir}ColorMC.Gui.pdb");
    }

    private static void Load()
    {
#if DEBUG
        GuiLoad.Load();
        return;
#endif
        if (NotHaveDll())
        {
            try
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

                    PathHelper.Delete($"{LoadDir}ColorMC.Gui.dll");
                    PathHelper.Delete($"{LoadDir}ColorMC.Gui.pdb");
                    PathHelper.Delete($"{LoadDir}ColorMC.Core.dll");
                    PathHelper.Delete($"{LoadDir}ColorMC.Core.pdb");

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
            catch
            {
                Aot = true;
                GuiLoad.Load();
            }
        }
        else
        {
            GuiLoad.Load();
        }
    }
}
