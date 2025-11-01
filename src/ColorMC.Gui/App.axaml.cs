using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ColorMC.Core;
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
            string temp = LanguageUtils.Get("App.Error1");
            Logs.Error(temp, e.ExceptionObject as Exception);
            WindowManager.ShowError(LanguageUtils.Get("App.Error9") ,temp, e.ExceptionObject as Exception);
        };
        ColorMCGui.StartLock();
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

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        Life = ApplicationLifetime;

        if (PlatformSettings is { } setting)
        {
            //初始化样式
            setting.ColorValuesChanged += (sender, e) =>
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
        FrpConfigUtils.Init();
        FrpLaunchUtils.Init();
        JoystickConfig.Init();
        ToolUtils.Init();
        BlockTexUtils.Init();
        Task.Run(() =>
        {
            SdlUtils.Init();
            Media.Init();
        });
        UpdateUtils.Init();
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

                Logs.Info(LanguageUtils.Get("Core.Info3"));

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
