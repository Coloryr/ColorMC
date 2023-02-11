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
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui;

public partial class App : Application
{
    public static IClassicDesktopStyleApplicationLifetime Life;
    public static DownloadWindow? DownloadWindow;
    public static UserWindow? UserWindow;
    public static MainWindow? MainWindow;
    public static AddGameWindow? AddGameWindow;
    public static CustomWindow? CustomWindow;
    public static AddModPackWindow? AddModPackWindow;
    public static SettingWindow? SettingWindow;
    public static SkinWindow? SkinWindow;
    public static AddJavaWindow? AddJavaWindow;
    public static Dictionary<GameSettingObj, GameEditWindow> GameEditWindows = new();

    public static readonly CrossFade CrossFade300 = new(TimeSpan.FromMilliseconds(300));
    public static readonly CrossFade CrossFade200 = new(TimeSpan.FromMilliseconds(200));
    public static readonly CrossFade CrossFade100 = new(TimeSpan.FromMilliseconds(100));
    public static readonly PageSlide PageSlide500 = new(TimeSpan.FromMilliseconds(500));

    public static event Action PicUpdate;

    public static ResourceDictionary? Language;

    public delegate void Handler();
    public static event Handler UserEdit;
    public static event Handler SkinLoad;

    public static Bitmap? BackBitmap { get; private set; }
    public static Bitmap GameIcon { get; private set; }
    public static WindowIcon Icon { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
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

            var uri = new Uri("resm:ColorMC.Gui.Resource.Pic.game.png");

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using var asset = assets!.Open(uri);

            GameIcon = new Bitmap(asset);

            var uri1 = new Uri("resm:ColorMC.Gui.icon.ico");
            using var asset1 = assets!.Open(uri1);

            Icon = new(asset1);

            App.LoadLanguage(LanguageType.zh_cn);

            BaseBinding.Init();

            ShowCustom();

            ShowSkin();

            if (GuiConfigUtils.Config != null &&
                await LoadImage(GuiConfigUtils.Config.BackImage,
                    GuiConfigUtils.Config.BackEffect))
            {
                OnPicUpdate();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
        var item = assm.GetManifestResourceStream(name);
        if (item == null)
        {
            return;
        }
        using MemoryStream stream = new();
        item.CopyTo(stream);
        var temp = Encoding.UTF8.GetString(stream.ToArray());
        Language = AvaloniaRuntimeXamlLoader.Load(temp) as ResourceDictionary;
    }

    public static byte[] GetFile(string name)
    {
        var assm = Assembly.GetExecutingAssembly();
        var item = assm.GetManifestResourceStream(name);
        using MemoryStream stream = new();
        item.CopyTo(stream);
        return stream.ToArray();
    }

    public static void RemoveImage()
    {
        BackBitmap?.Dispose();
        BackBitmap = null;
    }

    public static async Task<bool> LoadImage(string file, int eff)
    {
        RemoveImage();

        if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
        {
            BackBitmap = await ImageUtils.MakeImageSharp(file, eff);
            return BackBitmap != null;
        }

        return false;
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
                CoreMain.OnError?.Invoke(Localizer.Instance["Error10"], e, true);
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

    public static void Update(Window window, Image iamge, Grid rec)
    {
        if (GuiConfigUtils.Config != null)
        {
            if (BackBitmap != null)
            {
                iamge.Source = BackBitmap;
                if (GuiConfigUtils.Config.BackTran != 0)
                {
                    iamge.Opacity = (double)(100 - GuiConfigUtils.Config.BackTran) / 100;
                }
                else
                {
                    iamge.Opacity = 100;
                }
                iamge.IsVisible = true;
            }
            else
            {
                iamge.IsVisible = false;
                iamge.Source = null;
            }

            if (GuiConfigUtils.Config.WindowTran)
            {
                rec.Background = Utils.LaunchSetting.ColorSel.AppBackColor1;
                window.TransparencyLevelHint = (WindowTransparencyLevel)
                    (GuiConfigUtils.Config.WindowTranType + 1);
            }
            else
            {
                window.TransparencyLevelHint = WindowTransparencyLevel.None;
                rec.Background = Utils.LaunchSetting.ColorSel.AppBackColor;
            }
        }
    }
}
