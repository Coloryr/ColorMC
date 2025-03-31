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
#if !Phone
        ColorMCGui.StartLock();
#endif
    }

    /// <summary>
    /// 退出时
    /// </summary>
    public static event Action? OnClose;

    /// <summary>
    /// App
    /// </summary>
    public static Application ThisApp { get; private set; }
    /// <summary>
    /// 生命周期
    /// </summary>
    public static IApplicationLifetime? Life { get; private set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    public static bool IsHide { get; private set; }

    /// <summary>
    /// 本地化
    /// </summary>
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

    /// <summary>
    /// 获取本地化语言
    /// </summary>
    /// <param name="input">语言键</param>
    /// <returns>语言</returns>
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
            //初始化样式
            setting.ColorValuesChanged += (object? sender, PlatformColorValues e) =>
            {
                if (GuiConfigUtils.Config.ColorType == ColorType.Auto)
                {
                    ThemeManager.Init();
                }
            };
        }

        //初始化
        CoreManager.Init();
        ThemeManager.Init();
        ImageManager.Init();
        WindowManager.Init();

        CollectUtils.Init();
        GameCountUtils.Init();
#if !Phone
        FrpConfigUtils.Init();
        FrpLaunchUtils.Init();
        JoystickConfig.Init();
        ToolUtils.Init();
        SdlUtils.Init();
        Media.Init();
        LauncherUpgrade.Init();
#endif
        GameCloudUtils.Init();

        if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = WindowManager.AllWindow;
        }

        //核心第二阶段初始化
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

    /// <summary>
    /// 清理UI绑定
    /// </summary>
    public static void Clear()
    {
        ThemeManager.Remove();
        LangMananger.Remove();
        FuntionUtils.RunGC();
    }

    /// <summary>
    /// 加载语言文件
    /// </summary>
    /// <param name="type">语言类型</param>
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

    /// <summary>
    /// 关闭程序并退出
    /// </summary>
    public static void Exit()
    {
        Close();
        Environment.Exit(0);
    }

    /// <summary>
    /// 关闭程序不退出
    /// </summary>
    public static void Close()
    {
        OnClose?.Invoke();
        WindowManager.CloseAllWindow();
        ColorMCCore.Close();
        (Life as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    public static void Show()
    {
        IsHide = false;
        Dispatcher.UIThread.Post(WindowManager.Show);
    }

    /// <summary>
    /// 隐藏窗口
    /// </summary>
    public static void Hide()
    {
        IsHide = true;
        Media.PlayState = PlayState.Stop;
        WindowManager.Hide();
    }

    /// <summary>
    /// 测试是否需要关闭程序
    /// </summary>
    public static void TestClose()
    {
        if (IsHide && !GameManager.IsGameRuning())
        {
            ColorMCGui.Exit();
        }
    }
}
