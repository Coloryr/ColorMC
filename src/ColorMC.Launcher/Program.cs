using Avalonia;
using System;

#if !DEBUG
using Avalonia.Media;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
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
        Program.SetRuntimeState = Gui.ColorMCGui.SetRuntimeState;
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
    public const string Font = "resm:ColorMC.Launcher.Resources.MiSans-Regular.ttf?assembly=ColorMC.Launcher#MiSans";

#if !DEBUG

#if MIN
    public const bool IsMin = true;
#else
    public const bool IsMin = false;
#endif

#if AOT
    public const bool Aot = true;
#else
    public const bool Aot = false;
#endif

    /// <summary>
    /// 加载路径
    /// </summary>
    public const string TopVersion = "A35";

    public static readonly string[] BaseSha1 =
    [
        "f37874fcee09ae133c6dfd09c5b7b81683a26f8b",
        "8302c4ac5d71c7064dc1606924e40c3be1d60f4a",
        "698960f388d171f4a9683bae54f746af96adfd39",
        "3a081933a003f6576332b75600593125c59a6710"
    ];

    public delegate void IN(string[] args);
    public delegate void IN2(bool aot, bool min);
    public delegate AppBuilder IN1();
    public delegate void IN3(string dir);

    public static IN MainCall { get; set; }
    public static IN1 BuildApp { get; set; }
    public static IN SetBaseSha1 { get; set; }
    public static IN2 SetRuntimeState { get; set; }
    public static IN3 SetInputDir { get; set; }

    private static string _loadDir;
    private static string _inputDir;

    private static bool _isDll;
#endif

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
#if !DEBUG
        var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/ColorMC/run";
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
                _inputDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.ColorMC/";

                path = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/ColorMC/";
                if (Directory.Exists(path))
                {
                    Directory.Move(path, _inputDir);
                }
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
            if (!Directory.Exists(_inputDir))
            {
                Directory.CreateDirectory(_inputDir);
            }
            File.Create(_inputDir + "test").Close();
            File.Delete(_inputDir + "test");
        }
        catch
        {
            //没有权限写文件
            _inputDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/ColorMC/";
            _inputDir = Path.GetFullPath(_inputDir);
        }

        _loadDir = _inputDir + "dll";

        Console.WriteLine($"LoadDir: {_loadDir}");
#endif
        try
        {
#if DEBUG
            Gui.ColorMCGui.SetInputDir(AppContext.BaseDirectory);
            Gui.ColorMCGui.Main(args);
#else

#if !AOT
            if (!Load())
            {
                LoadError(args);
                return;
            }
#else
            GuiLoad.Load();
#endif
            SetInputDir(_inputDir);
            SetRuntimeState(Aot, IsMin);
            SetBaseSha1(BaseSha1);
            MainCall(args);
#endif
        }
        catch
        {
#if !DEBUG
            if (_isDll)
            {
                LoadError(args);
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
    private static void LoadError(string[] args)
    {
        File.Delete($"{_loadDir}/ColorMC.Gui.dll");
        File.Delete($"{_loadDir}/ColorMC.Gui.pdb");
        File.Delete($"{_loadDir}/ColorMC.Core.dll");
        File.Delete($"{_loadDir}/ColorMC.Core.pdb");

        GuiLoad.Load();
        GuiLoad.Run(args, true);

    }
#if !AOT
    private static bool NotHaveDll()
    {
        return File.Exists($"{_loadDir}/ColorMC.Core.dll")
            && File.Exists($"{_loadDir}/ColorMC.Core.pdb")
            && File.Exists($"{_loadDir}/ColorMC.Gui.dll")
            && File.Exists($"{_loadDir}/ColorMC.Gui.pdb");
    }

    private static bool Load()
    {
        if (!NotHaveDll())
        {
            GuiLoad.Load();
            return true;
        }
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
                Console.WriteLine("Main version error");
                return false;
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

            SetRuntimeState = (Delegate.CreateDelegate(typeof(IN2),
                   mis1.GetMethod("SetRuntimeState")!) as IN2)!;

            SetInputDir = (Delegate.CreateDelegate(typeof(IN3),
                   mis1.GetMethod("SetInputDir")!) as IN3)!;

            _isDll = true;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }
#endif

#endif
}
