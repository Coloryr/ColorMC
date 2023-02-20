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
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace ColorMC.Gui;

public partial class App : Application
{
    private static readonly Dictionary<string, string> Language = new();

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
    public static DownloadWindow? DownloadWindow { get; set; }
    public static UserWindow? UserWindow { get; set; }
    public static MainWindow? MainWindow { get; set; }
    public static AddGameWindow? AddGameWindow { get; set; }
    public static CustomWindow? CustomWindow { get; set; }
    public static AddModPackWindow? AddModPackWindow { get; set; }
    public static SettingWindow? SettingWindow { get; set; }
    public static SkinWindow? SkinWindow { get; set; }
    public static AddJavaWindow? AddJavaWindow { get; set; }
    public readonly static Dictionary<GameSettingObj, GameEditWindow> GameEditWindows = new();
    public readonly static Dictionary<GameSettingObj, AddModWindow> AddModWindows = new();
    public readonly static Dictionary<GameSettingObj, AddWorldWindow> AddWorldWindows = new();
    public readonly static Dictionary<GameSettingObj, AddResourcePackWindow> AddResourcePackWindows = new();

    public static readonly CrossFade CrossFade300 = new(TimeSpan.FromMilliseconds(300));
    public static readonly CrossFade CrossFade200 = new(TimeSpan.FromMilliseconds(200));
    public static readonly CrossFade CrossFade100 = new(TimeSpan.FromMilliseconds(100));
    public static readonly PageSlide PageSlide500 = new(TimeSpan.FromMilliseconds(500));

    public static event Action? PicUpdate;
    public static event Action? UserEdit;
    public static event Action? SkinLoad;

    public static Window? LastWindow;

    public static Bitmap? BackBitmap { get; private set; }
    public static Bitmap GameIcon { get; private set; }
    public static WindowIcon? Icon { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static string GetLanguage(string key)
    {
        if (Language.TryGetValue(key, out var res1))
            return res1!;

        return key;
    }

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

            var uri = new Uri("resm:ColorMC.Gui.Resource.Pic.game.png");

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using var asset = assets!.Open(uri);

            GameIcon = new Bitmap(asset);

            var uri1 = new Uri("resm:ColorMC.Gui.icon.ico");
            using var asset1 = assets!.Open(uri1);

            Icon = new(asset1);

            BaseBinding.Init();

            ShowCustom();

            if (GuiConfigUtils.Config != null)
                await LoadImage(GuiConfigUtils.Config.BackImage,
                    GuiConfigUtils.Config.BackEffect);
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
            LanguageType.en_us => "ColorMC.Gui.Resource.Language.en-us",
            _ => "ColorMC.Gui.Resource.Language.zh-cn"
        };
        var names = assm.GetManifestResourceNames();
        using var item = assm.GetManifestResourceStream(name);
        if (item == null)
        {
            return;
        }

        Language.Clear();
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(item);
        foreach(XmlNode item1 in xmlDoc.DocumentElement!.ChildNodes)
        {
            if (item1.Name == "String")
            {
                Language.Add(item1.Attributes!.GetNamedItem("Key")!.Value!, 
                    item1.FirstChild!.Value!);
            }
        }
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
            DownloadWindow?.Close();
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
                CoreMain.OnError?.Invoke(App.GetLanguage("Error10"), e, true);
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
            CustomWindow.Activate();
        }
        else
        {
            CustomWindow = new();
            CustomWindow.Show();
        }

        CustomWindow.Load(obj);
    }

    public static void ShowAddGame()
    {
        if (AddGameWindow != null)
        {
            AddGameWindow.Activate();
        }
        else
        {
            AddGameWindow = new();
            AddGameWindow.Show();
        }
    }

    public static void ShowDownload()
    {
        if (DownloadWindow != null)
        {
            DownloadWindow.Activate();
        }
        else
        {
            DownloadWindow = new();
            DownloadWindow.Show();
        }
    }

    public static void ShowUser()
    {
        if (UserWindow != null)
        {
            UserWindow.Activate();
        }
        else
        {
            UserWindow = new();
            UserWindow.Show();
        }
    }

    public static void ShowMain()
    {
        if (MainWindow != null)
        {
            MainWindow.Activate();
        }
        else
        {
            MainWindow = new();
            MainWindow.Show();
        }

        if (BaseBinding.ISNewStart)
        {
            new HelloWindow().ShowDialog(MainWindow);
        }
    }

    public static void ShowCurseForge()
    {
        if (AddModPackWindow != null)
        {
            AddModPackWindow.Activate();
        }
        else
        {
            AddModPackWindow = new();
            AddModPackWindow.Show();
        }
    }

    public static void ShowSetting()
    {
        if (SettingWindow != null)
        {
            SettingWindow.Activate();
        }
        else
        {
            SettingWindow = new();
            SettingWindow.Show();
        }
    }

    public static void ShowSkin()
    {
        if (SkinWindow != null)
        {
            SkinWindow.Activate();
        }
        else
        {
            SkinWindow = new();
            SkinWindow.Show();
        }
    }

    public static void ShowGameEdit(GameSettingObj obj, int type = 0)
    {
        if (GameEditWindows.TryGetValue(obj, out var win))
        {
            win.Activate();
        }
        else
        {
            GameEditWindow window = new();
            window.SetGame(obj);
            window.Show();
            window.SetType(type);
            GameEditWindows.Add(obj, window);
        }
    }

    public static void ShowAddJava()
    {
        if (AddJavaWindow != null)
        {
            AddJavaWindow.Activate();
        }
        else
        {
            AddJavaWindow = new();
            AddJavaWindow.Show();
        }
    }

    public static void ShowAddMod(GameSettingObj obj)
    {
        if (AddModWindows.TryGetValue(obj, out var value))
        {
            value.Activate();
        }
        else
        {
            var win = new AddModWindow();
            win.SetGame(obj);
            win.Show();
            AddModWindows.Add(obj, win);
        }
    }

    public static void ShowAddWorld(GameSettingObj obj)
    {
        if (AddWorldWindows.TryGetValue(obj, out var value))
        {
            value.Activate();
        }
        else
        {
            var win = new AddWorldWindow();
            win.SetGame(obj);
            win.Show();
            AddWorldWindows.Add(obj, win);
        }
    }

    public static void ShowAddResourcePack(GameSettingObj obj)
    {
        if (AddResourcePackWindows.TryGetValue(obj, out var value))
        {
            value.Activate();
        }
        else
        {
            var win = new AddResourcePackWindow();
            win.SetGame(obj);
            win.Show();
            AddResourcePackWindows.Add(obj, win);
        }
    }

    public static void ShowError(string data, Exception e, bool close = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            new ErrorWindow().Show(data, e, close);
        });
    }

    public static void ShowError(string data, string e, bool close = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            new ErrorWindow().Show(data, e, close);
        });
    }

    public static void CloseGameEdit(GameSettingObj obj)
    {
        if (GameEditWindows.TryGetValue(obj, out var win))
        {
            Dispatcher.UIThread.Post(win.Close);
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
