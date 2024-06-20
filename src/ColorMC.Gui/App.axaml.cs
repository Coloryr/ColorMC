using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Player;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UI.Controls.Count;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Controls.Download;
using ColorMC.Gui.UI.Controls.Error;
using ColorMC.Gui.UI.Controls.GameCloud;
using ColorMC.Gui.UI.Controls.GameConfigEdit;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UI.Controls.GameExport;
using ColorMC.Gui.UI.Controls.GameLog;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Controls.NetFrp;
using ColorMC.Gui.UI.Controls.ServerPack;
using ColorMC.Gui.UI.Controls.Setting;
using ColorMC.Gui.UI.Controls.Skin;
using ColorMC.Gui.UI.Controls.User;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using SkiaSharp;

namespace ColorMC.Gui;

public partial class App : Application
{
    public App()
    {
        Name = "ColorMC";
        ThisApp = this;

        AppDomain.CurrentDomain.UnhandledException += (a, e) =>
        {
            string temp = Lang("Gui.Error25");
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

    public static event Action? PicUpdate;
    public static event Action? UserEdit;
    public static event Action? SkinLoad;
    public static event Action? OnClose;

    public static Application ThisApp { get; private set; }
    public static IApplicationLifetime? Life { get; private set; }

    public static bool IsClose { get; set; }
    public static bool IsHide { get; private set; }

    public static PlatformThemeVariant NowTheme { get; private set; }

    private static readonly Language s_language = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        if (ColorMCGui.RunType == RunType.Program)
        {
            StartLock();
        }
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

    public static void ColorChange()
    {
        switch (GuiConfigUtils.Config.ColorType)
        {
            case ColorType.Auto:
                NowTheme = ThisApp.PlatformSettings!.GetColorValues().ThemeVariant;
                break;
            case ColorType.Light:
                NowTheme = PlatformThemeVariant.Light;
                break;
            case ColorType.Dark:
                NowTheme = PlatformThemeVariant.Dark;
                break;
        }
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

        ImageManager.Load();

        PlatformSettings!.ColorValuesChanged += PlatformSettings_ColorValuesChanged;

        ColorChange();

        BaseBinding.Init();

        WindowManager.StartWindow();

        if (ColorMCGui.RunType != RunType.AppBuilder)
        {
            Task.Run(() =>
            {
                ColorMCCore.Init1();
                BaseBinding.LoadDone();
            });
        }
        Dispatcher.UIThread.Post(async () => await ImageManager.LoadImage());

        if (ConfigBinding.WindowMode())
        {
            Dispatcher.UIThread.Post(() =>
            {
                TopLevel ??= TopLevel.GetTopLevel(WindowManager.AllWindow);
            });
        }
    }

    /// <summary>
    /// 加载样式
    /// </summary>
    public static void LoadPageSlide()
    {
        PageSlide500.Duration = TimeSpan.FromMilliseconds(GuiConfigUtils.Config.Style.AmTime);
        PageSlide500.Fade = GuiConfigUtils.Config.Style.AmFade;
    }

    /// <summary>
    /// 清理Gui缓存
    /// </summary>
    public static void Clear()
    {
        ColorSel.Remove();
        FontSel.Remove();
        LangSel.Remove();
        StyleSel.Remove();
    }

    public static void StartLock()
    {
        new Thread(() =>
        {
            while (!IsClose)
            {
                ColorMCGui.TestLock();
                if (IsClose)
                {
                    return;
                }
                IsHide = false;
                Dispatcher.UIThread.Post(Show);
            }
        })
        {
            Name = "ColorMC_Lock"
        }.Start();
    }

    private void PlatformSettings_ColorValuesChanged(object? sender, PlatformColorValues e)
    {
        if (GuiConfigUtils.Config.ColorType == ColorType.Auto)
        {
            NowTheme = PlatformSettings!.GetColorValues().ThemeVariant;

            ColorSel.Load();
            StyleSel.Load();
        }
    }

    public static void OnUserEdit()
    {
        UserEdit?.Invoke();
    }

    public static void OnPicUpdate()
    {
        PicUpdate?.Invoke();
    }

    public static void OnSkinLoad()
    {
        SkinLoad?.Invoke();
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
        IsClose = true;
        OnClose?.Invoke();
        WindowManager.CloseAllWindow();
        ColorMCCore.Close();
        (Life as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        Environment.Exit(Environment.ExitCode);
    }

    public static void Show()
    {
        IsHide = false;
        WindowManager.Show();
    }

    public static void Hide()
    {
        IsHide = true;
        Media.Stop();
        WindowManager.Hide();
    }

    public static void Reboot()
    {
        if (SystemInfo.Os != OsType.Android)
        {
            IsClose = true;
            Thread.Sleep(500);
            Process.Start($"{(SystemInfo.Os == OsType.Windows ?
                    "ColorMC.Launcher.exe" : "ColorMC.Launcher")}");
            //Thread.Sleep(200);
            Close();
        }
    }
}
