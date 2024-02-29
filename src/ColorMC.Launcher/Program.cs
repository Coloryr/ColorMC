using Avalonia;
using System;


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
    public const string TopVersion = "A24.1";

    public static readonly string[] BaseSha1 =
    [
        "f6b4215ce6aba05f3ce0047b36a5584140575cdf",
        "3579213f38d88c8b81f86b48f235f3a4fdbaad79",
        "e6db51604fd00a2c30e953912dfbf378244724d6",
        "621bac500c40a5e949746ab406ab5f62f69ed7a4"
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
                _inputDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/ColorMC/";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _inputDir = "/Users/shared/ColorMC/";
            }
            else
            {
                _inputDir = AppContext.BaseDirectory;
            }
        }

        if (!_inputDir.EndsWith('/'))
        {
            _inputDir += "/";
            _inputDir = Path.GetFullPath(_inputDir);
        }

        try
        {
            File.Create(_inputDir + "temp").Close();
        }
        catch
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            return;
        }


        _loadDir = _inputDir + "dll";

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
        
#endif
        try
        {
#if DEBUG
            Gui.ColorMCGui.Main(args);
#else
            Load();
            SetInputDir(_inputDir);
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
