using Avalonia;
using Avalonia.Media;
using ColorMC.Gui;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Launcher;

public class Program
{
    public const string Font = "resm:ColorMC.Launcher.Resources.MiSans-Normal.ttf?assembly=ColorMC.Launcher#MiSans";

    /// <summary>
    /// 加载路径
    /// </summary>
    public static string LoadDir { get; private set; }

    public delegate void IN(string[] args);

    public static IN MainCall;

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

        //解压openal
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var dll = AppContext.BaseDirectory + "OpenAL32.dll";
            if (!File.Exists(dll))
            {
                File.WriteAllBytes(dll, Resource1.OpenAL32);
            }
        }

        Console.WriteLine($"CheckDir:{LoadDir}");

        //先检查加载路径
        if (NotHaveDll(LoadDir))
        {
            //不存在
            //启动内部的
            MainCall = ColorMCGui.Main;
        }
        else
        {
            //有Dll
            Load();
        }

        try
        {
            MainCall(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static bool NotHaveDll(string dir)
    {
        return !File.Exists($"{dir}ColorMC.Core.dll")
            || !File.Exists($"{dir}ColorMC.Core.pdb")
            || !File.Exists($"{dir}ColorMC.Gui.dll")
            || !File.Exists($"{dir}ColorMC.Gui.pdb");
    }
    
    private static void Load()
    {
#if DEBUG
        LoadDir = AppContext.BaseDirectory;
#endif

        //加载DLL
        AssemblyLoadContext context = new("ColorMC");
        {
            using var file = File.OpenRead($"{LoadDir}ColorMC.Gui.dll");
            using var file1 = File.OpenRead($"{LoadDir}ColorMC.Gui.pdb");
            context.LoadFromStream(file, file1);
        }
        {
            using var file = File.OpenRead($"{LoadDir}ColorMC.Core.dll");
            using var file1 = File.OpenRead($"{LoadDir}ColorMC.Core.pdb");
            context.LoadFromStream(file, file1);
        }
        var item = context.Assemblies
                       .Where(x => x.GetName().Name == "ColorMC.Gui")
                       .First();

        var mis = item.GetTypes().Where(x => x.FullName == "ColorMC.Gui.ColorMCGui").First();

        MainCall = (Delegate.CreateDelegate(typeof(IN),
                mis.GetMethod("Main")!) as IN)!;
    }
}
