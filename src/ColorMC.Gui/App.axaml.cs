using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.LaunchPath;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Player;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui;

public partial class App : Application
{
    public App()
    {
        ThisApp = this;

        AppDomain.CurrentDomain.UnhandledException += (a, e) =>
        {
            string temp = Lang("App.Error1");
            Logs.Error(temp, e.ExceptionObject as Exception);
            WindowManager.ShowError(temp, e.ExceptionObject as Exception);
        };
    }

    public static TopLevel? TopLevel { get; set; }

    public static readonly SelfCrossFade CrossFade300 = new(TimeSpan.FromMilliseconds(300));
    public static readonly SelfCrossFade CrossFade200 = new(TimeSpan.FromMilliseconds(200));
    public static readonly SelfCrossFade CrossFade100 = new(TimeSpan.FromMilliseconds(100));
    public static readonly SelfPageSlide PageSlide500 = new(TimeSpan.FromMilliseconds(500));
    public static readonly SelfPageSlideSide SidePageSlide300 = new(TimeSpan.FromMilliseconds(300));

    public static event Action? OnClose;

    public static Application ThisApp { get; private set; }
    public static IApplicationLifetime? Life { get; private set; }

    public static bool IsHide { get; private set; }

    private static readonly Language s_language = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static string Lang(string input)
    {
        var data = s_language.GetLanguage(input, out bool have);
        if (have)
        {
            return data;
        }

        return LanguageHelper.Get(input);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        Life = ApplicationLifetime;

        if (ConfigUtils.Config == null)
        {
            LoadLanguage(LanguageType.zh_cn);
        }
        else
        {
            LoadLanguage(ConfigUtils.Config.Language);
        }

        if (PlatformSettings is { } setting)
        {
            setting.ColorValuesChanged += (object? sender, PlatformColorValues e) =>
            {
                if (GuiConfigUtils.Config.ColorType == ColorType.Auto)
                {
                    ThemeManager.Init();
                }
            };
        }

        GameSocket.Init();
        UpdateChecker.Init();
        GameCloudUtils.Init(ColorMCGui.RunDir);
        FrpConfigUtils.Init(ColorMCGui.RunDir);
        ImageUtils.Init(ColorMCGui.RunDir);
        InputConfigUtils.Init(ColorMCGui.RunDir);
        FrpPath.Init(ColorMCGui.RunDir);

        LoadPageSlide();

        BaseBinding.Init();

        ThemeManager.Init();
        ImageManager.Init();

        WindowManager.Init();

        if (ColorMCGui.RunType != RunType.AppBuilder)
        {
            Task.Run(() =>
            {
                ColorMCCore.Init1();
                BaseBinding.Init1();
            });
        }
        _ = ImageManager.LoadImage();
    }

    public static void LoadPageSlide()
    {
        PageSlide500.Duration = TimeSpan.FromMilliseconds(GuiConfigUtils.Config.Style.AmTime);
        PageSlide500.Fade = GuiConfigUtils.Config.Style.AmFade;
    }

    public static void Clear()
    {
        ThemeManager.Remove();
        LangMananger.Remove();
        FuntionUtils.RunGC();
    }

    public static void LoadLanguage(LanguageType type)
    {
        var assm = Assembly.GetExecutingAssembly();
        if (assm == null)
        {
            return;
        }
        string name = type switch
        {
            LanguageType.en_us => "ColorMC.Gui.Resource.Language.gui_en-us.json",
            _ => "ColorMC.Gui.Resource.Language.gui_zh-cn.json"
        };
        using var item = assm.GetManifestResourceStream(name)!;
        var reader = new StreamReader(item);
        s_language.Load(reader.ReadToEnd());
    }

    public static void Close()
    {
        OnClose?.Invoke();
        WindowManager.CloseAllWindow();
        ColorMCCore.Close();
        (Life as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        Environment.Exit(Environment.ExitCode);
    }

    public static void Show()
    {
        IsHide = false;
        Dispatcher.UIThread.Post(WindowManager.Show);
    }

    public static void Hide()
    {
        IsHide = true;
        Media.Stop();
        WindowManager.Hide();
    }

    public static void TestClose()
    {
        if (IsHide && !GameManager.IsGameRuning())
        {
            ColorMCGui.Close();
        }
    }
}
