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
    /// �˳�ʱ
    /// </summary>
    public static event Action? OnClose;

    /// <summary>
    /// App
    /// </summary>
    public static Application ThisApp { get; private set; }
    /// <summary>
    /// ��������
    /// </summary>
    public static IApplicationLifetime? Life { get; private set; }

    /// <summary>
    /// �Ƿ�����
    /// </summary>
    public static bool IsHide { get; private set; }

    /// <summary>
    /// ���ػ�
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
    /// ��ȡ���ػ�����
    /// </summary>
    /// <param name="input">���Լ�</param>
    /// <returns>����</returns>
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
            //��ʼ����ʽ
            setting.ColorValuesChanged += (object? sender, PlatformColorValues e) =>
            {
                if (GuiConfigUtils.Config.ColorType == ColorType.Auto)
                {
                    ThemeManager.Init();
                }
            };
        }

        //��ʼ��
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
        UpdateUtils.Init();
#endif
        GameCloudUtils.Init();

        if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = WindowManager.AllWindow;
        }

        //���ĵڶ��׶γ�ʼ��
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
    /// ����UI��
    /// </summary>
    public static void Clear()
    {
        ThemeManager.Remove();
        LangMananger.Remove();
        FuntionUtils.RunGC();
    }

    /// <summary>
    /// ���������ļ�
    /// </summary>
    /// <param name="type">��������</param>
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
    /// �رճ����˳�
    /// </summary>
    public static void Exit()
    {
        Close();
        Environment.Exit(0);
    }

    /// <summary>
    /// �رճ����˳�
    /// </summary>
    public static void Close()
    {
        OnClose?.Invoke();
        WindowManager.CloseAllWindow();
        ColorMCCore.Close();
        (Life as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }

    /// <summary>
    /// ��ʾ����
    /// </summary>
    public static void Show()
    {
        IsHide = false;
        Dispatcher.UIThread.Post(WindowManager.Show);
    }

    /// <summary>
    /// ���ش���
    /// </summary>
    public static void Hide()
    {
        IsHide = true;
        Media.PlayState = PlayState.Stop;
        WindowManager.Hide();
    }

    /// <summary>
    /// �����Ƿ���Ҫ�رճ���
    /// </summary>
    public static void TestClose()
    {
        if (IsHide && !GameManager.IsGameRuning())
        {
            ColorMCGui.Exit();
        }
    }
}
