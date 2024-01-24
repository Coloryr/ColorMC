using Avalonia;
using System;
using ColorMC.Gui.Objs;


#if !DEBUG
using Avalonia.Media;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
#endif

namespace ColorMC.Launcher;

#if !DEBUG
internal static class GuiLoad
{
    public static void Load()
    {
        Program.MainCall = Gui.ColorMCGui.Main;
        Program.BuildApp = Gui.ColorMCGui.BuildAvaloniaApp;
        Program.SetBaseSha1 = Gui.ColorMCGui.SetBaseSha1;
        Program.SetAot = Gui.ColorMCGui.SetAot;
        Program.SetInputDir = Gui.ColorMCGui.SetInputDir;
    }

    public static void Run(string[] args, bool crash)
    {
        Gui.ColorMCGui.SetCrash(crash);
        Gui.ColorMCGui.Main(args);
    }
}
#endif

public static class Program
{
    public const string Font = "resm:ColorMC.Launcher.Resources.MiSans-Normal.ttf?assembly=ColorMC.Launcher#MiSans";

#if !DEBUG
    /// <summary>
    /// 加载路径
    /// </summary>
    public const string TopVersion = "A24";

    public static readonly string[] BaseSha1 =
    [
        "bd58a8c8365098d253de231f9ebd251b5e258e74",
        "6032f845fdeceea9bec991b5956e8a362ce338ff",
        "b4c6c4292e45a6422fa999877ba7bd484b187a8a",
        "b72baf28731c3ea3e90b0a753e86ca39ebcd5bdd"
    ];

    public delegate void IN(string[] args);
    public delegate void IN2(bool aot);
    public delegate AppBuilder IN1();
    public delegate void IN3(string dir);

    public static IN MainCall { get; set; }
    public static IN1 BuildApp { get; set; }
    public static IN SetBaseSha1 { get; set; }
    public static IN2 SetAot { get; set; }
    public static IN3 SetInputDir { get; set; }
    
    public static bool Aot { get; set; }

    private static string _loadDir;
    private static string _inputDir;

    private static bool _isDll;
    private static bool _isError;
#endif

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
#if !DEBUG
        var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/ColorMC/run";
        if (File.Exists(path))
        {
            var dir = File.ReadAllText(path);
            if (Directory.Exists(dir))
            {
                _inputDir = dir;
            }
        }

        if (string.IsNullOrWhiteSpace(_inputDir))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _loadDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/ColorMC/dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _loadDir = "/Users/shared/ColorMC/dll";
            }
            else
            {
                _loadDir = AppContext.BaseDirectory + "dll";
            }
        }
        else
        {
            _loadDir = _inputDir + "/dll";
        }

        Console.WriteLine($"LoadDir: {_loadDir}");

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

        //有没有权限写文件
        try
        {
            File.Create(_inputDir + "/temp").Close();
        }
        catch
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            return;
        }
#endif
        try
        {
#if DEBUG
            Gui.ColorMCGui.Main(args);
#else
            Load();
            if (!string.IsNullOrWhiteSpace(_inputDir))
            {
                SetInputDir(_inputDir);
            }
            SetAot(Aot);
            SetBaseSha1(BaseSha1);
            MainCall(args);
#endif
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
#if !DEBUG
            if (_isDll)
            {
                _isError = true;
                File.Delete($"{_loadDir}/ColorMC.Gui.dll");
                File.Delete($"{_loadDir}/ColorMC.Gui.pdb");
                File.Delete($"{_loadDir}/ColorMC.Core.dll");
                File.Delete($"{_loadDir}/ColorMC.Core.pdb");

                GuiLoad.Run(args, _isError);
            }
#endif
        }
    }
#if DEBUG
    public static AppBuilder BuildAvaloniaApp()
    {
        return Gui.ColorMCGui.BuildAvaloniaApp();
    }
#else

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
                .With(new FontManagerOptions
                {
                    DefaultFamilyName = Font,
                })
                .LogToTrace()
                .UsePlatformDetect();
    }

    private static bool NotHaveDll()
    {
        return File.Exists($"{_loadDir}/ColorMC.Core.dll")
            && File.Exists($"{_loadDir}/ColorMC.Core.pdb")
            && File.Exists($"{_loadDir}/ColorMC.Gui.dll")
            && File.Exists($"{_loadDir}/ColorMC.Gui.pdb");
    }

    private static void Load()
    {
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
                    using var file = File.OpenRead($"{_loadDir}/ColorMC.Core.dll");
                    using var file1 = File.OpenRead($"{_loadDir}/ColorMC.Core.pdb");
                    context.LoadFromStream(file, file1);
                }
                {
                    using var file = File.OpenRead($"{_loadDir}/ColorMC.Gui.dll");
                    using var file1 = File.OpenRead($"{_loadDir}/ColorMC.Gui.pdb");
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

                    File.Delete($"{_loadDir}/ColorMC.Gui.dll");
                    File.Delete($"{_loadDir}/ColorMC.Gui.pdb");
                    File.Delete($"{_loadDir}/ColorMC.Core.dll");
                    File.Delete($"{_loadDir}/ColorMC.Core.pdb");

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

                SetInputDir = (Delegate.CreateDelegate(typeof(IN3),
                       mis1.GetMethod("SetInputDir")!) as IN3)!;

                _isDll = true;
            }
            catch
            {
                _isError = true;
                GuiLoad.Load();
            }
        }
        else
        {
            GuiLoad.Load();
        }
    }
#endif
}
