using System;
using System.Diagnostics;
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
    private static readonly CoreInitArg s_arg = new()
    {
        CurseForgeKey = "$2a$10$6L8AkVsaGMcZR36i8XvCr.O4INa2zvDwMhooYdLZU0bb/E78AsT0m",
        OAuthKey = "aa0dd576-d717-4950-b257-a478d2c20968"
    };

    private static FileStream s_lock;

    public static string RunDir { get; private set; }
    public static string[] BaseSha1 { get; private set; }
    public static string InputDir { get; private set; }

    public static RunType RunType { get; private set; } = RunType.AppBuilder;

    public static Func<Control> PhoneGetSetting { get; set; }
    public static Func<FrpType, string> PhoneGetFrp { get; set; }
    /// <summary>
    /// 获取一个空闲端口
    /// </summary>
    public static Func<int> PhoneGetFreePort { get; set; }

    public static bool IsAot { get; private set; }
    public static bool IsMin { get; private set; }
    public static bool IsCrash { get; private set; }
    public static bool IsClose { get; private set; }

    public const string Font = "resm:ColorMC.Launcher.Resources.MiSans-Regular.ttf?assembly=ColorMC.Launcher#MiSans";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 |
            SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        TaskScheduler.UnobservedTaskException += (object? sender, UnobservedTaskExceptionEventArgs e) =>
        {
            if (e.Exception.InnerException is DBusException)
            {
                Logs.Error(App.Lang("App.Error1"), e.Exception);
                return;
            }
            Logs.Crash(App.Lang("App.Error1"), e.Exception);
        };

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

        Console.WriteLine($"RunDir: {RunDir}");

        if (args.Length > 0)
        {
            if (args[0] == "-game" && args.Length < 2)
            {
                return;
            }
            else
            {
                BaseBinding.SetLaunch(args[1..]);
            }
        }

        SystemInfo.Init();

        try
        {
            if (IsLock(out var port))
            {
                GameSocket.SendMessage(port).Wait();
                return;
            }

            s_arg.Local = RunDir;
            ColorMCCore.Init(s_arg);

            BuildAvaloniaApp()
                 .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            s_lock?.Dispose();
            PathBinding.OpFile(Logs.Crash("Gui Crash", e));
            App.Close();
        }
    }

    public static void Close()
    {
        IsClose = true;
        App.Close();
    }

    public static void Reboot()
    {
        if (SystemInfo.Os != OsType.Android)
        {
            IsClose = true;
            Thread.Sleep(500);
            Process.Start($"{(SystemInfo.Os == OsType.Windows ?
                    "ColorMC.Launcher.exe" : "ColorMC.Launcher")}");
            App.Close();
        }
    }

    public static void StartPhone(string local)
    {
        SystemInfo.Init();

        RunType = RunType.Phone;

        RunDir = local;

        Console.WriteLine($"RunDir:{RunDir}");

        s_arg.Local = RunDir;
        ColorMCCore.Init(s_arg);
        GuiConfigUtils.Init(RunDir);
        FrpConfigUtils.Init(RunDir);
    }

    public static void SetRuntimeState(bool aot, bool min)
    {
        IsAot = aot;
        IsMin = min;
    }

    public static void SetBaseSha1(string[] data)
    {
        BaseSha1 = data;
    }

    public static void SetInputDir(string dir)
    {
        InputDir = dir;
    }

    public static void SetCrash(bool crash)
    {
        IsCrash = crash;
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        if (RunType == RunType.AppBuilder)
        {
            RunDir = AppContext.BaseDirectory;

            SystemInfo.Init();
        }

        GuiConfigUtils.Init(RunDir);

        var config = GuiConfigUtils.Config.Render.Windows;
        var opt = new Win32PlatformOptions();
        if (SystemInfo.IsArm)
        {
            opt.RenderingMode = [Win32RenderingMode.Wgl];
        }
        if (config.ShouldRenderOnUIThread is { } value)
        {
            opt.ShouldRenderOnUIThread = value;
        }
        if (config.OverlayPopups is { } value1)
        {
            opt.OverlayPopups = value1;
        }

        var config1 = GuiConfigUtils.Config.Render.X11;
        var opt1 = new X11PlatformOptions();
        if (config1.UseDBusMenu is { } value2)
        {
            opt1.UseDBusMenu = value2;
        }
        if (config1.UseDBusFilePicker is { } value3)
        {
            opt1.UseDBusFilePicker = value3;
        }
        if (config1.OverlayPopups is { } value4)
        {
            opt1.OverlayPopups = value4;
        }
        if (SystemInfo.IsArm)
        {
            opt1.RenderingMode = [X11RenderingMode.Egl];
        }
        else if (config1.SoftwareRender == true)
        {
            opt1.RenderingMode = [X11RenderingMode.Software];
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

    private static bool IsLock(out int port)
    {
        var name = RunDir + "lock";
        port = -1;
        if (File.Exists(name))
        {
            try
            {
                using var temp = File.Open(name, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                using var temp = File.Open(name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                byte[] temp1 = new byte[4];
                temp.ReadExactly(temp1);
                port = BitConverter.ToInt32(temp1);
                return true;
            }
        }

        return false;
    }

    public static void StartLock()
    {
        GameSocket.Init().Wait();
        string name = RunDir + "lock";
        s_lock = File.Open(name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        var data = BitConverter.GetBytes(GameSocket.Port);
        s_lock.Write(data);
        s_lock.Flush();
    }
}