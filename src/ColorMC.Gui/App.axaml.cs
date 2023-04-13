using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Player;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Controls.Download;
using ColorMC.Gui.UI.Controls.Error;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UI.Controls.Hello;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Controls.Server;
using ColorMC.Gui.UI.Controls.Setting;
using ColorMC.Gui.UI.Controls.Skin;
using ColorMC.Gui.UI.Controls.User;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ColorMC.Gui;

public partial class App : Application
{
    public App()
    {
        Name = "ColorMC";

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logs.Error("Error", e.ExceptionObject as Exception);
    }

    public static IClassicDesktopStyleApplicationLifetime? Life { get; private set; }

    public static AllControl? AllWindow { get; set; }
    public static DownloadControl? DownloadWindow { get; set; }
    public static UsersControl? UserWindow { get; set; }
    public static MainControl? MainWindow { get; set; }
    public static AddGameControl? AddGameWindow { get; set; }
    public static CustomControl? CustomWindow { get; set; }
    public static AddModPackControl? AddModPackWindow { get; set; }
    public static SettingControl? SettingWindow { get; set; }
    public static SkinControl? SkinWindow { get; set; }
    public static AddJavaControl? AddJavaWindow { get; set; }

    public readonly static Dictionary<string, GameEditControl> GameEditWindows = new();
    public readonly static Dictionary<string, AddControl> AddWindows = new();
    public readonly static Dictionary<string, ServerPackControl> ServerPackWindows = new();

    public static readonly CrossFade CrossFade300 = new(TimeSpan.FromMilliseconds(300));
    public static readonly CrossFade CrossFade200 = new(TimeSpan.FromMilliseconds(200));
    public static readonly CrossFade CrossFade100 = new(TimeSpan.FromMilliseconds(100));
    public static readonly SelfPageSlide PageSlide500 = new(TimeSpan.FromMilliseconds(500));

    public static event Action? PicUpdate;
    public static event Action? UserEdit;
    public static event Action? SkinLoad;

    public static Window? LastWindow;

    public static Bitmap? BackBitmap { get; private set; }
    public static Bitmap GameIcon { get; private set; }
    public static WindowIcon? Icon { get; private set; }

    public static PlatformThemeVariant NowTheme { get; private set; }

    public static IPlatformSettings PlatformSettings { get; private set; }

    private static readonly Language Language = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static string GetLanguage(string input)
    {
        var data = Language.GetLanguage(input, out bool have);
        if (have)
            return data;

        return LanguageHelper.GetName(input);
    }

    public static void ColorChange()
    {
        switch (GuiConfigUtils.Config.ColorType)
        {
            case ColorType.Auto:
                NowTheme = PlatformSettings.GetColorValues().ThemeVariant;
                break;
            case ColorType.Light:
                NowTheme = PlatformThemeVariant.Light;
                break;
            case ColorType.Dark:
                NowTheme = PlatformThemeVariant.Dark;
                break;
        }
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Life = desktop;

            desktop.Exit += Life_Exit;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {

        }

        try
        {
            if (ConfigUtils.Config == null)
            {
                LoadLanguage(LanguageType.zh_cn);
            }
            else
            {
                LoadLanguage(ConfigUtils.Config.Language);
            }

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using var asset = assets!.Open(new Uri("resm:ColorMC.Gui.Resource.Pic.game.png"));
            GameIcon = new Bitmap(asset);

            using var asset1 = assets!.Open(new Uri("resm:ColorMC.Gui.icon.ico"));
            Icon = new(asset1!);

            PlatformSettings = AvaloniaLocator.Current.GetRequiredService<IPlatformSettings>();
            PlatformSettings.ColorValuesChanged += PlatformSettings_ColorValuesChanged;

            ColorChange();

            BaseBinding.Init();

            if (ConfigBinding.WindowMode())
            {
                AllWindow = new();

                if (SystemInfo.Os == OsType.Android)
                {
                    (Life as ISingleViewApplicationLifetime)!.MainView = AllWindow;
                    AllWindow.Head.Max = false;
                    AllWindow.Head.Min = false;
                    AllWindow.Head.Clo = false;
                    AllWindow.Opened();
                }

                else if (SystemInfo.Os != OsType.Android)
                {
                    new SingleWindow(AllWindow).Show();
                }
            }

            ShowCustom();

            await LoadImage();


            //new Thread(() =>
            //{
            //    while (true)
            //    {
            //        Thread.Sleep(1000);
            //        GC.Collect();
            //    }
            //}).Start();
        }
        catch (Exception e)
        {
            Logs.Error("fail", e);
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static IBaseWindow FindRoot(object? con)
    {
        if (con is IBaseWindow win)
            return win;
        else if (con is UserControl con1 && con1.GetVisualRoot() is IBaseWindow win1)
            return win1;

        return AllWindow!;
    }

    private void PlatformSettings_ColorValuesChanged(object? sender, PlatformColorValues e)
    {
        if (GuiConfigUtils.Config.ColorType == ColorType.Auto)
        {
            NowTheme = PlatformSettings.GetColorValues().ThemeVariant;

            ColorSel.Instance.Load();
            OnPicUpdate();
        }
    }

    private void Life_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {

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
            LanguageType.en_us => "ColorMC.Gui.Resource.Language.en-us.xml",
            _ => "ColorMC.Gui.Resource.Language.zh-cn.xml"
        };
        using var item = assm.GetManifestResourceStream(name)!;

        Language.Load(item);
    }

    public static byte[] GetFile(string name)
    {
        var assm = Assembly.GetExecutingAssembly();
        var item = assm.GetManifestResourceStream(name);
        using MemoryStream stream = new();
        item!.CopyTo(stream);
        return stream.ToArray();
    }

    public static void RemoveImage()
    {
        var image = BackBitmap;
        BackBitmap = null;
        OnPicUpdate();
        image?.Dispose();
    }

    public static void LoadMusic()
    {
        if (GuiConfigUtils.Config.ServerCustom.PlayMusic)
        {
            BaseBinding.MusicStart();
        }
    }

    public static async Task LoadImage()
    {
        RemoveImage();
        string file = GuiConfigUtils.Config.BackImage;
        if (!string.IsNullOrWhiteSpace(file))
        {
            BackBitmap = await ImageUtils.MakeBackImage(
                file, GuiConfigUtils.Config.BackEffect,
                GuiConfigUtils.Config.BackLimit ? GuiConfigUtils.Config.BackLimitValue
                : 100);
        }

        OnPicUpdate();

        Funtcions.RunGC();
    }

    public static void DownloaderUpdate(CoreRunState state)
    {
        if (state == CoreRunState.Init)
        {
            ShowDownload();
        }
        else if (state == CoreRunState.Start)
        {
            DownloadWindow?.Load();
        }
        else if (state == CoreRunState.End)
        {
            DownloadWindow?.Window.Close();
        }
    }

    public static void ShowCustom()
    {
        bool ok = true;
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 == null || string.IsNullOrWhiteSpace(config.Item2.ServerCustom.UIFile))
        {
            ok = false;
        }
        else
        {
            try
            {
                string file = config.Item2.ServerCustom.UIFile;
                if (!File.Exists(file))
                {
                    file = BaseBinding.GetRunDir() + config.Item2.ServerCustom.UIFile;
                    if (!File.Exists(file))
                    {
                        ok = false;
                    }
                }

                if (ok)
                {
                    var obj = JsonConvert.DeserializeObject<UIObj>(File.ReadAllText(file));
                    if (obj == null)
                    {
                        ok = false;
                    }
                    else
                    {
                        ShowCustom(obj);
                    }
                }
            }
            catch (Exception e)
            {
                var data = GetLanguage("Error10");
                Logs.Error(data, e);
                ShowError(data, e, true);
                ok = false;
            }
        }

        if (!ok)
        {
            ShowMain();
        }
    }

    public static void AWindow(IUserControl con)
    {
        if (ConfigBinding.WindowMode())
        {
            AllWindow?.Add((con as UserControl)!);
        }
        else
        {
            new SelfBaseWindow(con).Show();
        }
    }

    public static void AWindow1(IUserControl con)
    {
        if (ConfigBinding.WindowMode())
        {
            AllWindow?.Add((con as UserControl)!);
        }
        else
        {
            Window? temp = LastWindow;
            if (temp == null)
            {
                if (MainWindow != null && MainWindow?.Window is Window win)
                {
                    temp = win;
                }
                else if (CustomWindow != null && CustomWindow?.Window is Window win1)
                {
                    temp = win1;
                }
                else
                {
                    temp = AllWindow?.Window as Window;
                }
            }
            if (temp != null)
                new SelfBaseWindow(con).ShowDialog(temp);
        }
    }

    public static void ShowCustom(UIObj obj)
    {
        if (CustomWindow != null)
        {
            CustomWindow.Window.Activate();
        }
        else
        {
            CustomWindow = new();
            AWindow(CustomWindow);
        }

        CustomWindow?.Load(obj);
    }

    public static void ShowAddGame(string? file = null)
    {
        if (AddGameWindow != null)
        {
            AddGameWindow.Window.Activate();
        }
        else
        {
            AddGameWindow = new();
            AWindow(AddGameWindow);
        }
        if (!string.IsNullOrWhiteSpace(file))
        {
            AddGameWindow?.AddFile(file);
        }
    }

    public static void ShowDownload()
    {
        if (DownloadWindow != null)
        {
            DownloadWindow.Window.Activate();
        }
        else
        {
            DownloadWindow = new();
            AWindow(DownloadWindow);
        }
    }

    public static void ShowUser(string? url = null)
    {
        if (UserWindow != null)
        {
            UserWindow.Window.Activate();
        }
        else
        {
            UserWindow = new();
            AWindow(UserWindow);
        }

        if (!string.IsNullOrWhiteSpace(url))
        {
            UserWindow?.AddUrl(url);
        }
    }

    public static void ShowMain()
    {
        if (MainWindow != null)
        {
            MainWindow.Window.Activate();
        }
        else
        {
            MainWindow = new();
            AWindow(MainWindow);

            if (BaseBinding.ISNewStart)
            {
                var con = new HelloControl();
                AWindow1(con);
            }
        }
    }

    public static void ShowCurseForge()
    {
        if (AddModPackWindow != null)
        {
            AddModPackWindow.Window.Activate();
        }
        else
        {
            AddModPackWindow = new();
            AWindow(AddModPackWindow);
        }
    }

    public static void ShowSetting(SettingType type)
    {
        if (SettingWindow != null)
        {
            SettingWindow.Window.Activate();
        }
        else
        {
            SettingWindow = new();
            AWindow(SettingWindow);
        }

        SettingWindow?.GoTo(type);
    }

    public static void ShowSkin()
    {
        if (SkinWindow != null)
        {
            SkinWindow.Window.Activate();
        }
        else
        {
            SkinWindow = new();
            AWindow(SkinWindow);
        }
    }

    public static void ShowGameEdit(GameSettingObj obj, GameEditWindowType type
        = GameEditWindowType.Normal)
    {
        if (GameEditWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Window.Activate();
            win1.SetType(type);
        }
        else
        {
            var con = new GameEditControl(obj);
            GameEditWindows.Add(obj.UUID, con);
            AWindow(con);
            con.SetType(type);
        }
    }

    public static void ShowAddJava()
    {
        if (AddJavaWindow != null)
        {
            AddJavaWindow.Window.Activate();
        }
        else
        {
            AddJavaWindow = new();
            AWindow(AddJavaWindow);
        }
    }

    public static void ShowAdd(GameSettingObj obj, ModDisplayObj obj1)
    {
        var type1 = Funtcions.CheckNotNumber(obj1.PID) || Funtcions.CheckNotNumber(obj1.FID) ?
            SourceType.Modrinth : SourceType.CurseForge;

        if (AddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.Activate();
            value.GoFile(type1, obj1.PID);
        }
        else
        {
            var con = new AddControl(obj);
            AddWindows.Add(obj.UUID, con);
            AWindow(con);
            con.GoFile(type1, obj1.PID);
        }
    }

    public static Task ShowAddSet(GameSettingObj obj)
    {
        if (AddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.Activate();
            return value.GoSet();
        }
        else
        {
            var con = new AddControl(obj);
            AddWindows.Add(obj.UUID, con);
            AWindow(con);
            return con.GoSet();
        }
    }

    public static void ShowAdd(GameSettingObj obj, FileType type)
    {
        if (AddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.Activate();
            value.Go(type);
        }
        else
        {
            var con = new AddControl(obj);
            AddWindows.Add(obj.UUID, con);
            AWindow(con);
            con.Go(type);
        }
    }

    public static void ShowServerPack(GameSettingObj obj)
    {
        if (ServerPackWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.Activate();
        }
        else
        {
            var con = new ServerPackControl(obj);
            ServerPackWindows.Add(obj.UUID, con);
            AWindow(con);
        }
    }

    public static void ShowError(string? data, Exception? e, bool close = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var con = new ErrorControl();
            con.Show(data, e, close);
            AWindow(con);
        });
    }

    public static void ShowError(string data, string e, bool close = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var con = new ErrorControl();
            con.Show(data, e, close);
            AWindow(con);
        });
    }

    public static void CloseGameWindow(GameSettingObj obj)
    {
        if (GameEditWindows.TryGetValue(obj.UUID, out var win))
        {
            Dispatcher.UIThread.Post(win.Window.Close);
        }
        if (AddWindows.TryGetValue(obj.UUID, out var win1))
        {
            Dispatcher.UIThread.Post(win1.Window.Close);
        }
    }

    public static void Close()
    {
        Media.Close();
        Life?.Shutdown();
        BaseBinding.Exit();
        Environment.Exit(Environment.ExitCode);
    }

    public static void Update(Window? window, Image? image)
    {
        if (GuiConfigUtils.Config != null)
        {
            if (image != null)
            {
                if (BackBitmap != null)
                {
                    image.Source = BackBitmap;
                    if (GuiConfigUtils.Config.BackTran != 0)
                    {
                        image.Opacity = (double)(100 - GuiConfigUtils.Config.BackTran) / 100;
                    }
                    else
                    {
                        image.Opacity = 1.0;
                    }
                    image.IsVisible = true;
                }
                else
                {
                    image.IsVisible = false;
                    image.Source = null;
                }
            }


            if (window != null)
            {
                if (GuiConfigUtils.Config.WindowTran)
                {
                    window.TransparencyLevelHint = (WindowTransparencyLevel)
                           (GuiConfigUtils.Config.WindowTranType + 1);
                }
                else
                {
                    window.TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;
                }

                switch (GuiConfigUtils.Config.ColorType)
                {
                    case ColorType.Auto:
                        window.RequestedThemeVariant =
                            PlatformSettings.GetColorValues().ThemeVariant ==
                            PlatformThemeVariant.Light ? ThemeVariant.Light : ThemeVariant.Dark;
                        break;
                    case ColorType.Light:
                        window.RequestedThemeVariant = ThemeVariant.Light;
                        break;
                    case ColorType.Dark:
                        window.RequestedThemeVariant = ThemeVariant.Dark;
                        break;
                }
            }
        }
    }

    public static Task<bool> HaveUpdate(string data)
    {
        var window = GetMainWindow();
        if (window == null)
        {
            return Task.FromResult(false);
        }

        return window.Info6.ShowWait(GetLanguage("Gui.Info5"), data);
    }

    public static IBaseWindow? GetMainWindow()
    {
        if (MainWindow == null)
        {
            if (CustomWindow == null)
            {
                return null;
            }
            else
            {
                return CustomWindow.Window;
            }
        }

        return MainWindow.Window;
    }
}
