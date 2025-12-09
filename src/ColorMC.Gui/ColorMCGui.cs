using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Media;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Hook;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using Silk.NET.Core.Loader;
using Tmds.DBus.Protocol;

namespace ColorMC.Gui;

/// <summary>
/// ColorMC Gui
/// </summary>
public static class ColorMCGui
{
    /// <summary>
    /// Core 启动参数
    /// </summary>
    private static readonly CoreInitObj s_arg = new()
    {
        Local = "",
        CurseForgeKey = "$2a$10$6L8AkVsaGMcZR36i8XvCr.O4INa2zvDwMhooYdLZU0bb/E78AsT0m",
        OAuthKey = "aa0dd576-d717-4950-b257-a478d2c20968"
    };

    /// <summary>
    /// 文件锁
    /// </summary>
    private static FileStream s_lock;

    /// <summary>
    /// 运行目录
    /// </summary>
    public static string BaseDir { get; private set; }
    /// <summary>
    /// 基础版本信息
    /// </summary>
    public static string[] BaseSha1 { get; private set; }

    /// <summary>
    /// 运行类型
    /// </summary>
    public static RunType RunType { get; private set; } = RunType.AppBuilder;

    /// <summary>
    /// 是否为Aot模式
    /// </summary>
    public static bool IsAot { get; private set; }
    /// <summary>
    /// 是否为Min模式
    /// </summary>
    public static bool IsMin { get; private set; }
    /// <summary>
    /// 是否崩溃
    /// </summary>
    public static bool IsCrash { get; private set; }
    /// <summary>
    /// 是否关闭
    /// </summary>
    public static bool IsClose { get; private set; }
    /// <summary>
    /// 是否已经初始化
    /// </summary>
    public static bool IsInit { get; private set; }

    /// <summary>
    /// 默认字体
    /// </summary>
    public const string Font = "resm:ColorMC.Launcher.Resources.SourceHanSansSC-Regular.otf?assembly=ColorMC.Launcher#Source Han Sans SC";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        //SDL运行库查找
        ((DefaultPathResolver)PathResolver.Default).Resolvers.Add(file =>
            AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") is string nativeDllSearchDirectories
                ? nativeDllSearchDirectories.Split(";").Select(directory => Path.Combine(directory, file))
                : []
        );

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            if (e.Exception.InnerException is DBusException)
            {
                return;
            }
            Logs.Error(LanguageUtils.Get("App.Error.Log16"), e.Exception);
        };

        RunType = RunType.Program;

        Logs.Init();
        SystemInfo.Init();

        Console.WriteLine($"RunDir: {BaseDir}");
        if (!Directory.Exists(BaseDir))
        {
            Directory.CreateDirectory(BaseDir);
        }

        try
        {
            GuiConfigUtils.Init();

            //以管理员方式启动
            if (GuiConfigUtils.Config.ServerCustom.AdminLaunch && !ProcessUtils.IsRunAsAdmin())
            {
                try
                {
                    ProcessUtils.LaunchAdmin(args);
                }
                catch
                {

                }
                return;
            }

            if (StartArg(args))
            {
                return;
            }

            //是否已经启动过了
            if (IsLock(out var port))
            {
                LaunchSocketUtils.SendMessage(port, args).Wait();
                return;
            }

            var builder = BuildAvaloniaApp();

            if (GuiConfigUtils.Config.LauncherFunction.FastLaunch && ProcessUtils.IsRunAsAdmin())
            {
                HookUtils.RegisterFastLaunch(false);
                if (GuiConfigUtils.Config.LauncherFunction.FastModrinth)
                {
                    HookUtils.RegisterFastLaunch(true);
                }
            }

            s_arg.Local = BaseDir;

            LanguageUtils.Load(LanguageType.zh_cn);

            ColorMCCore.Init(s_arg);

            BaseBinding.ReadBuildConfig();

            LanguageUtils.Load(GuiConfigUtils.Config.Language);

            IsInit = true;

            builder.StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            if (IsClose)
            {
                return;
            }
            PathBinding.OpenFileWithExplorer(Logs.Crash("Gui Crash", e));
            App.Close();
            throw new GuiException();
        }
        finally
        {
            s_lock?.Close();
            s_lock?.Dispose();
        }
    }

    public static bool StartArg(string[] args)
    {
        //快捷启动
        if (args.Length <= 0)
        {
            return false;
        }

        if (args[0] is GuiNames.NameCommandGame && args.Length >= 2)
        {
            if (IsInit)
            {
                BaseBinding.Launch(args[1..]);
            }
            else
            {
                BaseBinding.SetLaunch(args[1..]);
            }
        }
        else if (args[0].StartsWith(GuiNames.NameUrlColorMC))
        {
            var url = args[0][GuiNames.NameUrlColorMC.Length..];
            if (url.StartsWith(GuiNames.NameUrlRun))
            {
                string uuid = url[GuiNames.NameUrlRun.Length..];
                if (IsInit)
                {
                    BaseBinding.Launch([uuid]);
                }
                else
                {
                    BaseBinding.SetLaunch([uuid]);
                }
            }
            else if (url.StartsWith(GuiNames.NameUrlInstall))
            {
                string appId = url[GuiNames.NameUrlInstall.Length..];
                if (IsInit)
                {
                    WindowManager.ShowAddGame(null, file: appId);
                }
            }
        }
        else if (args[0].StartsWith(GuiNames.NameUrlModrinth))
        {
            var url = args[0][GuiNames.NameUrlModrinth.Length..];
            if (url.StartsWith(GuiNames.NameUrlMod))
            {
                string appId = url[GuiNames.NameUrlMod.Length..];
                if (IsInit)
                {
                    WindowManager.ShowAdd(appId);
                }
            }
        }
        else if (args[0] == GuiNames.NameCommandRegister)
        {
            if (!ProcessUtils.IsRunAsAdmin())
            {
                try
                {
                    ProcessUtils.LaunchAdmin(args);
                    return false;
                }
                catch
                {

                }
            }
            else
            {
                HookUtils.RegisterFastLaunch(false);
                if (args.Contains(GuiNames.NameCommandRemapModrinth))
                {
                    HookUtils.RegisterFastLaunch(true);
                }
                Environment.Exit(0);
            }

            return true;
        }

        if (!IsInit)
        {
            return false;
        }

        switch (args[0])
        {
            case GuiNames.NameCommandInstall when args.Length == 2:
            //MMC for labymod
            case GuiNames.NameCommandImport when args.Length == 2:
                WindowManager.ShowAddGame(null, file: args[1]);
                break;
        }
        return false;
    }

    /// <summary>
    /// 退出程序
    /// </summary>
    public static void Exit()
    {
        IsClose = true;
        App.Exit();
    }

    /// <summary>
    /// 重启程序
    /// </summary>
    public static void Reboot()
    {
        IsClose = true;
        s_lock.Close();
        s_lock.Dispose();
        Thread.Sleep(500);
        Process.Start($"{(SystemInfo.Os == OsType.Windows ?
                "ColorMC.Launcher.exe" : "ColorMC.Launcher")}");
        App.Exit();
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
        BaseDir = dir;
    }

    public static void SetCrash(bool crash)
    {
        IsCrash = crash;
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        if (RunType == RunType.AppBuilder)
        {
            BaseDir = AppContext.BaseDirectory + "colormc\\";

            SystemInfo.Init();
            GuiConfigUtils.Init();
        }

        var builder = AppBuilder.Configure<App>()
            .With(new FontManagerOptions
            {
                DefaultFamilyName = Font,
            })
#if DEBUG
            .LogToTrace(LogEventLevel.Information)
#else
            .LogToTrace()
#endif
            .UsePlatformDetect();

        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                {
                    var config = GuiConfigUtils.Config.Render.Windows;
                    var opt = new Win32PlatformOptions();

                    if (config.ShouldRenderOnUIThread is { } value)
                    {
                        opt.ShouldRenderOnUIThread = value;
                    }

                    if (config.OverlayPopups is { } value1)
                    {
                        opt.OverlayPopups = value1;
                    }

                    if (config.Wgl is true)
                    {
                        opt.RenderingMode = [Win32RenderingMode.Wgl, Win32RenderingMode.Software];
                    }
                    else if (config.UseVulkan is true)
                    {
                        opt.RenderingMode =
                        [
                            Win32RenderingMode.Vulkan,
                        Win32RenderingMode.Software
                        ];
                    }
                    else if (config.UseSoftware is true)
                    {
                        opt.RenderingMode = [Win32RenderingMode.Software];
                    }
                    else if (SystemInfo.IsArm)
                    {
                        opt.RenderingMode =
                        [
                            Win32RenderingMode.Wgl,
                            Win32RenderingMode.Software
                        ];
                    }

#if DEBUG
                    //opt.RenderingMode =
                    //[
                    //    Win32RenderingMode.Vulkan,
                    //    Win32RenderingMode.Software
                    //];
#endif
                    builder.With(opt);
                    break;
                }
            case OsType.Linux:
                {
                    var config = GuiConfigUtils.Config.Render.X11;
                    var opt = new X11PlatformOptions();
                    if (config.UseDBusMenu is { } value2)
                    {
                        opt.UseDBusMenu = value2;
                    }

                    if (config.UseDBusFilePicker is { } value3)
                    {
                        opt.UseDBusFilePicker = value3;
                    }

                    if (config.OverlayPopups is { } value4)
                    {
                        opt.OverlayPopups = value4;
                    }

                    if (config.UseEgl is true)
                    {
                        opt.RenderingMode =
                        [
                            X11RenderingMode.Egl,
                        X11RenderingMode.Software
                        ];
                    }
                    else if (config.UseVulkan is true)
                    {
                        opt.RenderingMode =
                        [
                            X11RenderingMode.Vulkan,
                        X11RenderingMode.Software
                        ];
                    }
                    else if (config.UseSoftware == true)
                    {
                        opt.RenderingMode = [X11RenderingMode.Software];
                    }
                    else if (SystemInfo.IsArm)
                    {
                        opt.RenderingMode =
                        [
                            X11RenderingMode.Egl,
                        X11RenderingMode.Software
                        ];
                    }

                    builder.With(opt);
                    break;
                }
            case OsType.MacOS:
                {
                    var opt = new MacOSPlatformOptions()
                    {
                        DisableDefaultApplicationMenuItems = true,
                    };
                    builder.With(opt);
                    break;
                }
        }

        return builder;
    }

    private static bool IsLock(out int port)
    {
        var name = Path.Combine(BaseDir, GuiNames.NameLockFile);
        port = -1;
        if (!File.Exists(name))
        {
            return false;
        }

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

        return false;
    }

    internal static void StartLock()
    {
        LaunchSocketUtils.Init().Wait();
        string name = Path.Combine(BaseDir, GuiNames.NameLockFile);
        s_lock = File.Open(name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        var data = BitConverter.GetBytes(LaunchSocketUtils.Port);
        s_lock.Write(data);
        s_lock.Flush();
    }
}