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
            ShowError(temp, e.ExceptionObject as Exception);
        };
    }
    public static Window? LastWindow { get; set; }

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
    public static CountControl? CountWindow { get; set; }
    public static NetFrpControl? NetFrpWindow { get; set; }

    public static TopLevel? TopLevel { get; set; }

    public readonly static Dictionary<string, GameEditControl> GameEditWindows = [];
    public readonly static Dictionary<string, GameConfigEditControl> ConfigEditWindows = [];
    public readonly static Dictionary<string, AddControl> AddWindows = [];
    public readonly static Dictionary<string, ServerPackControl> ServerPackWindows = [];
    public readonly static Dictionary<string, GameLogControl> GameLogWindows = [];
    public readonly static Dictionary<string, GameExportControl> GameExportWindows = [];
    public readonly static Dictionary<string, GameCloudControl> GameCloudWindows = [];

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

    public static Bitmap? BackBitmap { get; private set; }
    public static Bitmap GameIcon { get; private set; }
    public static Bitmap LoadIcon { get; private set; }
    public static WindowIcon? Icon { get; private set; }

    public static bool IsHide { get; private set; }
    public static bool IsClose { get; set; }

    public static PlatformThemeVariant NowTheme { get; private set; }

    private static readonly Language s_language = new();

    private static readonly WindowTransparencyLevel[] WindowTran =
    [
        WindowTransparencyLevel.None,
        WindowTransparencyLevel.Transparent,
        WindowTransparencyLevel.Blur,
        WindowTransparencyLevel.AcrylicBlur,
        WindowTransparencyLevel.Mica
    ];

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

        {
            using var asset = AssetLoader.Open(new Uri("resm:ColorMC.Gui.Resource.Pic.game.png"));
            GameIcon = new Bitmap(asset);
        }
        {
            using var asset1 = AssetLoader.Open(new Uri(SystemInfo.Os == OsType.MacOS
                ? "resm:ColorMC.Gui.macicon.ico" : "resm:ColorMC.Gui.icon.ico"));
            Icon = new(asset1!);
        }
        {
            using var asset1 = AssetLoader.Open(new Uri("resm:ColorMC.Gui.Resource.Pic.load.png"));
            LoadIcon = new(asset1!);
        }

        PlatformSettings!.ColorValuesChanged += PlatformSettings_ColorValuesChanged;

        ColorChange();

        BaseBinding.Init();

        if (ConfigBinding.WindowMode())
        {
            if (SystemInfo.Os == OsType.Android)
            {
                AllWindow = new();
                (Life as ISingleViewApplicationLifetime)!.MainView = AllWindow;
                AllWindow.Model.HeadDisplay = false;
                AllWindow.Opened();
            }
            else
            {
                var win = new SingleWindow();
                AllWindow = win.Win;
                win.Show();
            }
        }

        ShowCustom();
        if (ColorMCGui.RunType != RunType.AppBuilder)
        {
            Task.Run(() =>
            {
                ColorMCCore.Init1();
                BaseBinding.LoadDone();
            });
        }
        Dispatcher.UIThread.Post(() => _ = LoadImage());
        if (ConfigBinding.WindowMode())
        {
            Dispatcher.UIThread.Post(() =>
            {
                TopLevel ??= TopLevel.GetTopLevel(AllWindow);
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

    public static IBaseWindow FindRoot(object? con)
    {
        if (con is AllControl all)
            return all;
        else if (GuiConfigUtils.Config.WindowMode)
            return AllWindow!;
        else if (con is IBaseWindow win)
            return win;
        else if (con is IUserControl con1)
            return con1.Window;

        return AllWindow!;
    }

    private async void PlatformSettings_ColorValuesChanged(object? sender, PlatformColorValues e)
    {
        if (GuiConfigUtils.Config.ColorType == ColorType.Auto)
        {
            NowTheme = PlatformSettings!.GetColorValues().ThemeVariant;

            ColorSel.Load();
            StyleSel.Load();
            await LoadImage();
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

    public static void RemoveImage()
    {
        var image = BackBitmap;
        BackBitmap = null;
        OnPicUpdate();
        image?.Dispose();
    }

    public static async Task LoadImage()
    {
        RemoveImage();
        var file = GuiConfigUtils.Config.BackImage;
        if (string.IsNullOrWhiteSpace(file))
        {
            file = "https://www.todaybing.com/api/today/cn";
        }

        if (GuiConfigUtils.Config.EnableBG)
        {
            BackBitmap = await ImageUtils.MakeBackImage(
                    file, GuiConfigUtils.Config.BackEffect,
                    GuiConfigUtils.Config.BackLimit ? GuiConfigUtils.Config.BackLimitValue : 100);
        }

        OnPicUpdate();
        ColorSel.Load();
        FuntionUtils.RunGC();
    }

    public static void ShowCustom()
    {
        bool ok = true;
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 == null
            || config.Item2.ServerCustom?.EnableUI == false
            || string.IsNullOrWhiteSpace(config.Item2.ServerCustom?.UIFile))
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
                    file = BaseBinding.GetRunDir() + file;
                    if (!File.Exists(file))
                    {
                        ok = false;
                    }
                }

                if (ok)
                {
                    ShowCustom(file, false);
                }
            }
            catch (Exception e)
            {
                var data = Lang("Gui.Error10");
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

    public static void AWindow(IUserControl con, bool newwindow = false)
    {
        if (ConfigBinding.WindowMode())
        {
            if (newwindow)
            {
                if (SystemInfo.Os == OsType.Android)
                {
                    return;
                }

                var win = new SelfBaseWindow(con);
                con.SetBaseModel(win.Model);
                win.Show();
            }
            else
            {
                con.SetBaseModel(AllWindow!.Model);
                AllWindow.Add(con);
            }
        }
        else
        {
            var win = new SelfBaseWindow(con);
            TopLevel ??= win;
            con.SetBaseModel(win.Model);
            win.Show();
        }
    }

    public static void ShowCustom(string obj, bool newwindow)
    {
        if (CustomWindow != null)
        {
            CustomWindow.Window.TopActivate();
        }
        else
        {
            CustomWindow = new();
            AWindow(CustomWindow, newwindow);
        }

        CustomWindow.Load(obj);
    }

    public static void ShowAddGame(string? group, bool isDir = false, string? file = null)
    {
        if (AddGameWindow != null)
        {
            AddGameWindow.Window.TopActivate();
        }
        else
        {
            AddGameWindow = new();
            AWindow(AddGameWindow);
        }
        AddGameWindow.SetGroup(group);
        if (!string.IsNullOrWhiteSpace(file))
        {
            AddGameWindow.AddFile(file, isDir);
        }
    }

    public static Task<bool> StartDownload(ICollection<DownloadItemObj> list)
    {
        return Dispatcher.UIThread.Invoke(() =>
        {
            DownloadWindow?.Window.Close();

            DownloadWindow = new(list);
            AWindow(DownloadWindow);

            return DownloadWindow.Start();
        });
    }

    public static void ShowUser(bool add = false, string? url = null)
    {
        if (UserWindow != null)
        {
            UserWindow.Window.TopActivate();
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
        if (add)
        {
            UserWindow?.Add();
        }
    }

    public static void ShowMain()
    {
        if (MainWindow != null)
        {
            MainWindow.Window.TopActivate();
        }
        else
        {
            MainWindow = new();
            AWindow(MainWindow);
        }
    }

    public static void ShowAddModPack()
    {
        if (AddModPackWindow != null)
        {
            AddModPackWindow.Window.TopActivate();
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
            SettingWindow.Window.TopActivate();
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
            SkinWindow.Window.TopActivate();
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
            win1.Window.TopActivate();
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
            AddJavaWindow.Window.TopActivate();
        }
        else
        {
            AddJavaWindow = new();
            AWindow(AddJavaWindow);
        }
    }

    public static void ShowAdd(GameSettingObj obj, ModDisplayModel obj1)
    {
        var type1 = FuntionUtils.CheckNotNumber(obj1.PID) || FuntionUtils.CheckNotNumber(obj1.FID) ?
            SourceType.Modrinth : SourceType.CurseForge;

        if (AddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.TopActivate();
            value.GoFile(type1, obj1.PID!);
        }
        else
        {
            var con = new AddControl(obj);
            AddWindows.Add(obj.UUID, con);
            AWindow(con);
            con.GoFile(type1, obj1.PID!);
        }
    }

    public static Task ShowAddSet(GameSettingObj obj)
    {
        if (AddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.TopActivate();
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
            value.Window.TopActivate();
            value.GoTo(type);
        }
        else
        {
            var con = new AddControl(obj);
            AddWindows.Add(obj.UUID, con);
            AWindow(con);
            con.GoTo(type);
        }
    }

    public static void ShowServerPack(GameSettingObj obj)
    {
        if (ServerPackWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.TopActivate();
        }
        else
        {
            var con = new ServerPackControl(obj);
            ServerPackWindows.Add(obj.UUID, con);
            AWindow(con);
        }
    }

    public static void ShowGameLog(GameSettingObj obj)
    {
        if (GameLogWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.TopActivate();
        }
        else
        {
            var con = new GameLogControl(obj);
            GameLogWindows.Add(obj.UUID, con);
            AWindow(con);
        }
    }

    public static void ShowConfigEdit(GameSettingObj obj)
    {
        if (ConfigEditWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Window.TopActivate();
        }
        else
        {
            var con = new GameConfigEditControl(obj);
            ConfigEditWindows.Add(obj.UUID, con);
            AWindow(con);
        }
    }

    public static void ShowConfigEdit(WorldObj obj)
    {
        string key = obj.Game.UUID + ":" + obj.LevelName;
        if (ConfigEditWindows.TryGetValue(key, out var win1))
        {
            win1.Window.TopActivate();
        }
        else
        {
            var con = new GameConfigEditControl(obj);
            ConfigEditWindows.Add(key, con);
            AWindow(con);
        }
    }

    public static void ShowGameCloud(GameSettingObj obj, bool world = false)
    {
        string key = obj.UUID;
        if (GameCloudWindows.TryGetValue(key, out var win1))
        {
            win1.Window.TopActivate();
            if (world)
            {
                win1.GoWorld();
            }
        }
        else
        {
            var con = new GameCloudControl(obj);
            GameCloudWindows.Add(key, con);
            AWindow(con);
            if (world)
            {
                con.GoWorld();
            }
        }
    }

    public static void ShowCount()
    {
        if (CountWindow != null)
        {
            CountWindow.Window.TopActivate();
        }
        else
        {
            CountWindow = new();
            AWindow(CountWindow);
        }
    }

    public static void ShowGameExport(GameSettingObj obj)
    {
        if (GameExportWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.TopActivate();
        }
        else
        {
            var con = new GameExportControl(obj);
            GameExportWindows.Add(obj.UUID, con);
            AWindow(con);
        }
    }

    public static void ShowNetFrp()
    {
        if (NetFrpWindow != null)
        {
            NetFrpWindow.Window.TopActivate();
        }
        else
        {
            NetFrpWindow = new();
            AWindow(NetFrpWindow);
        }
    }

    public static void ShowError(string? data, Exception? e, bool close = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var con = new ErrorControl(data, e, close);
            AWindow(con);
        });
    }

    public static void ShowError(string data, string e, bool close = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var con = new ErrorControl(data, e, close);
            AWindow(con);
        });
    }

    public static void CloseGameWindow(GameSettingObj obj)
    {
        if (GameEditWindows.TryGetValue(obj.UUID, out var win))
        {
            Dispatcher.UIThread.Post(win.Window.Close);
        }
        if (GameLogWindows.TryGetValue(obj.UUID, out var win5))
        {
            Dispatcher.UIThread.Post(win5.Window.Close);
        }
        if (AddWindows.TryGetValue(obj.UUID, out var win1))
        {
            Dispatcher.UIThread.Post(win1.Window.Close);
        }
        if (GameCloudWindows.TryGetValue(obj.UUID, out var win2))
        {
            Dispatcher.UIThread.Post(win2.Window.Close);
        }
        if (GameExportWindows.TryGetValue(obj.UUID, out var win3))
        {
            Dispatcher.UIThread.Post(win3.Window.Close);
        }
        if (ServerPackWindows.TryGetValue(obj.UUID, out var win4))
        {
            Dispatcher.UIThread.Post(win4.Window.Close);
        }
        foreach (var item in ConfigEditWindows)
        {
            if (item.Key.StartsWith(obj.UUID))
            {
                Dispatcher.UIThread.Post(item.Value.Window.Close);
            }
        }

    }

    public static void Close()
    {
        IsClose = true;
        OnClose?.Invoke();
        CloseAllWindow();
        ColorMCCore.Close();
        (Life as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        Environment.Exit(Environment.ExitCode);
    }

    public static void UpdateWindow(BaseModel model)
    {
        if (GuiConfigUtils.Config != null)
        {
            model.Background = GuiConfigUtils.Config.WindowTran ?
                ColorSel.BottomTranColor : ColorSel.BottomColor;
            model.Back = BackBitmap;
            if (BackBitmap != null)
            {
                if (GuiConfigUtils.Config.BackTran != 0)
                {
                    model.BgOpacity = (double)(100 - GuiConfigUtils.Config.BackTran) / 100;
                }
                else
                {
                    model.BgOpacity = 1.0;
                }
                model.BgVisible = true;
            }
            else
            {
                model.BgVisible = false;
            }

            if (GuiConfigUtils.Config.WindowTran)
            {
                model.Hints = [WindowTran[GuiConfigUtils.Config.WindowTranType]];
            }
            else
            {
                model.Hints = [WindowTransparencyLevel.None];
            }

            switch (GuiConfigUtils.Config.ColorType)
            {
                case ColorType.Auto:
                    model.Theme =
                        ThisApp.PlatformSettings!.GetColorValues().ThemeVariant ==
                        PlatformThemeVariant.Light ? ThemeVariant.Light : ThemeVariant.Dark;
                    break;
                case ColorType.Light:
                    model.Theme = ThemeVariant.Light;
                    break;
                case ColorType.Dark:
                    model.Theme = ThemeVariant.Dark;
                    break;
            }
        }
    }

    public static void UpdateCheckFail()
    {
        var window = GetMainWindow();
        if (window == null)
        {
            return;
        }
        window.Model.Show(Lang("SettingWindow.Tab3.Error2"));
    }

    public static IBaseWindow? GetMainWindow()
    {
        if (ConfigBinding.WindowMode())
        {
            return AllWindow;
        }
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

    private static void Show()
    {
        if (ConfigBinding.WindowMode())
        {
            if (AllWindow?.GetVisualRoot() is Window window)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        }
        else
        {
            if (MainWindow?.GetVisualRoot() is Window window)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
            else if (CustomWindow?.GetVisualRoot() is Window window1)
            {
                window1.Show();
                window1.WindowState = WindowState.Normal;
                window1.Activate();
            }
        }
        IsHide = false;
    }

    public static void Hide()
    {
        IsHide = true;
        Media.Stop();
        if (ConfigBinding.WindowMode())
        {
            if (AllWindow?.GetVisualRoot() is Window window)
            {
                window.Hide();
            }
        }
        else
        {
            if (MainWindow?.GetVisualRoot() is Window window)
            {
                window.Hide();
                (CustomWindow?.GetVisualRoot() as Window)?.Close();
            }
            else if (CustomWindow?.GetVisualRoot() is Window window1)
            {
                window1.Hide();
            }
            CloseAllWindow();
        }
    }

    public static void CloseAllWindow()
    {
        (NetFrpWindow?.GetVisualRoot() as Window)?.Close();
        (CountWindow?.GetVisualRoot() as Window)?.Close();
        (AddJavaWindow?.GetVisualRoot() as Window)?.Close();
        (SkinWindow?.GetVisualRoot() as Window)?.Close();
        (SettingWindow?.GetVisualRoot() as Window)?.Close();
        (AddModPackWindow?.GetVisualRoot() as Window)?.Close();
        (AddGameWindow?.GetVisualRoot() as Window)?.Close();
        (UserWindow?.GetVisualRoot() as Window)?.Close();
        (DownloadWindow?.GetVisualRoot() as Window)?.Close();
        foreach (var item in GameEditWindows.Values)
        {
            (item.GetVisualRoot() as Window)?.Close();
        }
        foreach (var item in ConfigEditWindows.Values)
        {
            (item.GetVisualRoot() as Window)?.Close();
        }
        foreach (var item in AddWindows.Values)
        {
            (item.GetVisualRoot() as Window)?.Close();
        }
        foreach (var item in ServerPackWindows.Values)
        {
            (item.GetVisualRoot() as Window)?.Close();
        }
        foreach (var item in GameLogWindows.Values)
        {
            (item.GetVisualRoot() as Window)?.Close();
        }
        foreach (var item in GameExportWindows.Values)
        {
            (item.GetVisualRoot() as Window)?.Close();
        }
        foreach (var item in GameCloudWindows.Values)
        {
            (item.GetVisualRoot() as Window)?.Close();
        }
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
