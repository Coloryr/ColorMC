using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Controls.Download;
using ColorMC.Gui.UI.Controls.Error;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UI.Controls.Hello;
using ColorMC.Gui.UI.Controls.Main;
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

    public static AllWindow? AllWindow { get; set; }
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

    private static readonly Language Language = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static string GetLanguage(string key)
        => Language.GetLanguage(key);

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Life = desktop;
        }

        try
        {
            if (Life != null)
            {
                Life.Exit += Life_Exit;
            }

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

            BaseBinding.Init();

            if (ConfigBinding.WindowMode())
            {
                AllWindow = new();
                AllWindow.Show();
            }

            ShowCustom();

            if (GuiConfigUtils.Config != null)
                await LoadImage(GuiConfigUtils.Config.BackImage,
                    GuiConfigUtils.Config.BackEffect);

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

    public static async Task LoadImage(string file, int eff)
    {
        RemoveImage();

        if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
        {
            BackBitmap = await ImageUtils.MakeBackImage(file, eff);
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

    private void Life_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        BaseBinding.Exit();
    }

    public static void ShowCustom()
    {
        bool ok;
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
                if (File.Exists(file))
                {
                    var obj = JsonConvert.DeserializeObject<UIObj>(File.ReadAllText(file));
                    if (obj == null)
                    {
                        ok = false;
                    }
                    else
                    {
                        ShowCustom(obj);
                        ok = true;
                    }
                }
                else
                {
                    ok = false;
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

    public static void ShowCustom(UIObj obj)
    {
        if (CustomWindow != null)
        {
            CustomWindow.Window.Activate();
        }
        else
        {
            CustomWindow = new();
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

            if (BaseBinding.ISNewStart)
            {
                new HelloControl();
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
        }
    }

    public static void ShowGameEdit(GameSettingObj obj, GameEditWindowType type
        = GameEditWindowType.Normal)
    {
        if (GameEditWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Window.Activate();
        }
        else
        {
            var con = new GameEditControl(obj);
            con.SetType(type);
            GameEditWindows.Add(obj.UUID, con);
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
        }
    }

    public static void ShowAdd(GameSettingObj obj, ModDisplayObj obj1)
    {
        var type1 = UIUtils.CheckNotNumber(obj1.PID) || UIUtils.CheckNotNumber(obj1.FID) ?
            SourceType.Modrinth : SourceType.CurseForge;

        if (AddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.Activate();
            value.GoFile(type1, obj1.PID);
        }
        else
        {
            var con = new AddControl(obj);
            con.GoFile(type1, obj1.PID);
            AddWindows.Add(obj.UUID, con);
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
            con.Go(type);
            AddWindows.Add(obj.UUID, con);
        }
    }

    public static void ShowError(string data, Exception e, bool close = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            new ErrorControl().Show(data, e, close);
        });
    }

    public static void ShowError(string data, string e, bool close = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            new ErrorControl().Show(data, e, close);
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
        Life?.Shutdown();

        Environment.Exit(Environment.ExitCode);
    }

    public static CornerRadius GetCornerRadius()
    {
        if (GuiConfigUtils.Config.CornerRadius)
        {
            return new CornerRadius(GuiConfigUtils.Config.Radius,
                GuiConfigUtils.Config.Radius, 0, 0);
        }
        else
        {
            return new CornerRadius(0);
        }
    }

    public static CornerRadius GetCornerRadius1()
    {
        if (GuiConfigUtils.Config.CornerRadius)
        {
            return new CornerRadius(0, 0, GuiConfigUtils.Config.Radius,
                GuiConfigUtils.Config.Radius);
        }
        else
        {
            return new CornerRadius(0);
        }
    }

    public static void Update(Window window, Image image, Border rec, Border rec1)
    {
        if (GuiConfigUtils.Config != null)
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
                    image.Opacity = 100;
                }
                image.IsVisible = true;
            }
            else
            {
                image.IsVisible = false;
                image.Source = null;
            }

            if (GuiConfigUtils.Config.WindowTran)
            {
                rec1.Background = ColorSel.AppBackColor1;
                window.TransparencyLevelHint = (WindowTransparencyLevel)
                    (GuiConfigUtils.Config.WindowTranType + 1);
            }
            else
            {
                window.TransparencyLevelHint = WindowTransparencyLevel.None;
                rec1.Background = ColorSel.AppBackColor;
            }

            rec.CornerRadius = rec1.CornerRadius = GetCornerRadius1();
        }
    }
}
