using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using Tmds.DBus.Protocol;

namespace ColorMC.Gui;

public static class ColorMCGui
{
    public static string RunDir { get; private set; }
    public static string[] BaseSha1 { get; private set; }
    public static string InputDir { get; private set; }

    public static RunType RunType { get; private set; } = RunType.AppBuilder;

    public static Func<Control> PhoneGetSetting { get; set; }
    public static Func<FrpType, string> PhoneGetFrp { get; set; }
    public static bool IsAot { get; set; }
    public static bool IsCrash { get; set; }

    public const string Font = "resm:ColorMC.Launcher.Resources.MiSans-Normal.ttf?assembly=ColorMC.Launcher#MiSans";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 |
            SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        try
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            SystemInfo.Init();

            RunType = RunType.Program;

            if (string.IsNullOrWhiteSpace(InputDir))
            {
                RunDir = SystemInfo.Os switch
                {
                    OsType.Linux => $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/ColorMC/",
                    OsType.MacOS => "/Users/shared/ColorMC/",
                    _ => AppContext.BaseDirectory
                };
            }
            else
            {
                RunDir = InputDir;
            }

            Console.WriteLine($"RunDir:{RunDir}");

            if (args.Length > 0)
            {
                if (args[0] == "-game" && args.Length != 2)
                {
                    return;
                }
                else
                {
                    BaseBinding.SetLaunch(args[1]);
                }
            }

            var name = RunDir + "lock";
            if (File.Exists(name))
            {
                try
                {
                    using var temp = File.Open(name, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch
                {
                    using var temp = File.Open(name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    using var writer = new StreamWriter(temp);
                    writer.Write(true);
                    writer.Flush();
                    Environment.Exit(0);
                    return;
                }
            }

            ColorMCCore.Init(RunDir);

            BuildAvaloniaApp()
                 .StartWithClassicDesktopLifetime(args);

            Console.WriteLine();
        }
        catch (Exception e)
        {
            PathBinding.OpFile(Logs.Crash("Gui Crash", e));
            App.Close();
        }
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        if (e.Exception.InnerException is DBusException)
        {
            Logs.Error(App.Lang("Gui.Error25"), e.Exception);
            return;
        }
        Logs.Crash(App.Lang("Gui.Error25"), e.Exception);
    }

    public static void StartPhone(string local)
    {
        SystemInfo.Init();

        RunType = RunType.Phone;

        RunDir = local;

        Console.WriteLine($"RunDir:{RunDir}");

        ColorMCCore.Init(RunDir);
        GuiConfigUtils.Init(RunDir);
        FrpConfigUtils.Init(RunDir);
    }

    public static void TestLock()
    {
        string name = RunDir + "lock";
        using var temp = File.Open(name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var file = MemoryMappedFile.CreateFromFile(temp, null, 100,
            MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
        using var reader = file.CreateViewAccessor();
        reader.Write(0, false);
        while (!App.IsClose)
        {
            Thread.Sleep(100);
            var data = reader.ReadBoolean(0);
            if (data)
                break;
        }
    }

    public static void SetAot(bool aot)
    {
        IsAot = aot;
    }

    public static void SetBaseSha1(string[] data)
    {
        BaseSha1 = data;
    }

    public static void SetInputDir(string dir)
    {
        InputDir = dir;
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        if (RunType == RunType.AppBuilder)
        {
            RunDir = AppContext.BaseDirectory;
        }

        GuiConfigUtils.Init(RunDir);

        var config = GuiConfigUtils.Config.Render.Windows;
        var opt = new Win32PlatformOptions();
        if (SystemInfo.IsArm)
        {
            opt.RenderingMode = [Win32RenderingMode.Wgl];
        }
        if (config.ShouldRenderOnUIThread != null)
        {
            opt.ShouldRenderOnUIThread = config.ShouldRenderOnUIThread == true;
        }

        var config1 = GuiConfigUtils.Config.Render.X11;
        var opt1 = new X11PlatformOptions();
        if (config1.UseDBusMenu != null)
        {
            opt1.UseDBusMenu = config1.UseDBusMenu == true;
        }
        if (config1.UseDBusFilePicker != null)
        {
            opt1.UseDBusFilePicker = config1.UseDBusFilePicker == true;
        }
        if (config1.OverlayPopups != null)
        {
            opt1.OverlayPopups = config1.OverlayPopups == true;
        }

        var opt2 = new MacOSPlatformOptions()
        {
            DisableDefaultApplicationMenuItems = true,
        };

        return AppBuilder.Configure<App>()
            .With(new FontManagerOptions
            {
                DefaultFamilyName = Font,
            })
            .With(opt)
            .With(opt1)
            .With(opt2)
            .LogToTrace()
            .UsePlatformDetect();
    }

    public static void SetCrash(bool crash)
    {
        IsCrash = crash;
    }
}