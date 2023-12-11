using Avalonia;
using ColorMC.Gui;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
#if !DEBUG
using System.Linq;
using System.Runtime.Loader;
#endif

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

    public static void Run(string[] args, bool crash)
    {
        ColorMCGui.SetCrash(crash);
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
    public const string TopVersion = "A23";

    public static readonly string[] BaseSha1 =
    [
        "09483d8a01e9fa48907f9afad2d8c9d3731edbd0",
        "9112fdef44fa14ea9a5b40a07868e642cca42cc1",
        "5d5da93be14f65e31c7ef98903aca91d13ba42f2",
        "37bd03c457a44d036f1cadc5ef4fe0e68e400cfe"
    ];
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

    private static bool _isDll;

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
        //Test AOT
        try
        {
            var assemblyName = new AssemblyName("AotTestClass");
            var assBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        }
        catch
        {
            Aot = true;
        }

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
            if (_isDll)
            {
                File.Delete($"{LoadDir}ColorMC.Gui.dll");
                File.Delete($"{LoadDir}ColorMC.Gui.pdb");
                File.Delete($"{LoadDir}ColorMC.Core.dll");
                File.Delete($"{LoadDir}ColorMC.Core.pdb");

                GuiLoad.Run(args, true);
            }
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        Load();
        return BuildApp();
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
#else
        if (Aot)
        {
            GuiLoad.Load();
            return;
        }
        else if (NotHaveDll())
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

                    File.Delete($"{LoadDir}ColorMC.Gui.dll");
                    File.Delete($"{LoadDir}ColorMC.Gui.pdb");
                    File.Delete($"{LoadDir}ColorMC.Core.dll");
                    File.Delete($"{LoadDir}ColorMC.Core.pdb");

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

                SetAot = (Delegate.CreateDelegate(typeof(IN2),
                       mis1.GetMethod("SetAot")!) as IN2)!;

                _isDll = true;
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
#endif
    }
}
