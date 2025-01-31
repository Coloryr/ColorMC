using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.MusicPlayer;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
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

        if (SystemInfo.Os != OsType.Android)
        {
            ColorMCGui.StartLock();
        }
    }

    public static event Action? OnClose;

    public static Application ThisApp { get; private set; }
    public static IApplicationLifetime? Life { get; private set; }

    public static bool IsHide { get; private set; }

    private static readonly Language s_language = new();

    public override void Initialize()
    {
        if (ConfigUtils.Config == null)
        {
            LoadLanguage(LanguageType.zh_cn);
        }
        else
        {
            LoadLanguage(ConfigUtils.Config.Language);
        }

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

        CoreManager.Init();
        ThemeManager.Init();
        JoystickConfig.Init(ColorMCGui.RunDir);
        FrpConfigUtils.Init(ColorMCGui.RunDir);
        FrpLaunchUtils.Init(ColorMCGui.RunDir);
        CollectUtils.Init(ColorMCGui.RunDir);
        ImageManager.Init(ColorMCGui.RunDir);
        WindowManager.Init(ColorMCGui.RunDir);
        GameCount.Init(ColorMCGui.RunDir);
        //if (SystemInfo.Os != OsType.MacOS)
        {
            SdlUtils.Init();
        }
        Media.Init();
        UpdateUtils.Init();
        GameCloudUtils.Init(ColorMCGui.RunDir);

        if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = WindowManager.AllWindow;
        }

        if (ColorMCGui.RunType != RunType.AppBuilder)
        {
            Task.Run(() =>
            {
                ColorMCCore.Init1();
                BaseBinding.Init1();
            });
        }
        _ = ImageManager.LoadBGImage();

        DataContext = new AppModel();
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

    public static void Exit()
    {
        Close();
        Environment.Exit(0);
    }

    public static void Close()
    {
        OnClose?.Invoke();
        WindowManager.CloseAllWindow();
        ColorMCCore.Close();
        (Life as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }

    public static void Show()
    {
        IsHide = false;
        Dispatcher.UIThread.Post(WindowManager.Show);
    }

    public static void Hide()
    {
        IsHide = true;
        Media.PlayState = PlayState.Stop;
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
