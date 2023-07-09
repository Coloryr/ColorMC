using Avalonia;
using Avalonia.Media;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;

namespace ColorMC.Gui;

public static class ColorMCGui
{
    public static string RunDir { get; private set; }

    public static string[] BaseSha1 { get; private set; }

    public static RunType RunType { get; private set; } = RunType.AppBuilder;

    public const string Font = "resm:ColorMC.Launcher.Resources.MiSans-Normal.ttf?assembly=ColorMC.Launcher#MiSans";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        try
        {
            SystemInfo.Init();

            RunType = RunType.Program;

            RunDir = SystemInfo.Os switch
            {
                OsType.Linux => $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/ColorMC/",
                OsType.MacOS => "/Users/shared/ColorMC/",
                _ => AppContext.BaseDirectory
            };

            Console.WriteLine($"RunDir:{RunDir}");

            string name = RunDir + "lock";
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
        }
        catch (Exception e)
        {
            BaseBinding.OpFile(Logs.SaveCrash("Gui Crash", e));
            App.Close();
        }
    }

    public static void StartPhone(string local)
    {
        SystemInfo.Init();

        RunType = RunType.Program;

        RunDir = local;

        Console.WriteLine($"RunDir:{RunDir}");

        ColorMCCore.Init(RunDir);
        GuiConfigUtils.Init(RunDir);
    }

    public static void TestLock()
    {
        string name = RunDir + "lock";
        using var temp = File.Open(name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var file = MemoryMappedFile.CreateFromFile(temp, null, 100, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
        using var reader = file.CreateViewAccessor();
        reader.Write(0, false);
        while (!App.NeedClose)
        {
            var data = reader.ReadBoolean(0);
            if (data)
                break;
            Thread.Sleep(100);
        }
    }

    public static void SetBaseSha1(string[] data)
    {
        BaseSha1 = data;
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

        return AppBuilder.Configure<App>()
            .With(new FontManagerOptions
            {
                DefaultFamilyName = Font,
            })
            .With(opt)
            .With(opt1)
            .LogToTrace()
            .UsePlatformDetect();
    }
}