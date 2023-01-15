using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui;

public partial class App : Application
{
    public static readonly IBrush BackColor = Brush.Parse("#FFFFFFFF");
    public static readonly IBrush BackColor1 = Brush.Parse("#11FFFFFF"); //Brushes.Transparent;

    private static IClassicDesktopStyleApplicationLifetime Life;
    public static DownloadWindow? DownloadWindow;
    public static UserWindow? UserWindow;
    public static MainWindow? MainWindow;
    public static HelloWindow? HelloWindow;
    public static AddGameWindow? AddGameWindow;
    public static AddCurseForgeWindow? AddCurseForgeWindow;
    public static SettingWindow? SettingWindow;
    public static Dictionary<GameSettingObj, GameEditWindow> GameEditWindows = new();

    public static ResourceDictionary? Language;

    public static Bitmap? BackBitmap { get; private set; }
    public static Bitmap GameIcon { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        LoadLanguage(LanguageType.zh_cn);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Life = desktop;
            desktop.MainWindow = new InitWindow();
        }

        base.OnFrameworkInitializationCompleted();

        if (Life != null)
        {
            Life.Exit += Life_Exit;
        }

        var uri = new Uri("resm:ColorMC.Gui.Resource.Pic.game.png");

        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        var asset = assets!.Open(uri);

        GameIcon = new Bitmap(asset);
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
        var item = assm.GetManifestResourceStream(name);
        if (item == null)
        {
            return;
        }
        MemoryStream stream = new();
        item.CopyTo(stream);
        var temp = Encoding.UTF8.GetString(stream.ToArray());
        Language = AvaloniaRuntimeXamlLoader.Load(temp) as ResourceDictionary;
    }

    public static void RemoveImage()
    {
        if (BackBitmap != null)
        {
            BackBitmap.Dispose();
        }

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

    public static void ShowUser(bool add)
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

        if (add)
            UserWindow.SetAdd();
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
    }

    public static void ShowHello()
    {
        if (HelloWindow != null)
        {
            HelloWindow.Activate();
        }
        else
        {
            HelloWindow = new();
            HelloWindow.Show();
        }
    }

    public static void ShowCurseForge()
    {
        if (AddCurseForgeWindow != null)
        {
            AddCurseForgeWindow.Activate();
        }
        else
        {
            AddCurseForgeWindow = new();
            AddCurseForgeWindow.Show();
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
            try
            {
                SettingWindow = new();
                SettingWindow.Show();
            }
            catch (Exception e)
            {
                Logs.Error("窗口打开失败", e);
            }
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

    public static void ShowError(string data, Exception e, bool close = false)
    {
        new ErrorWindow().Show(data, e, close);
    }

    public static void Close()
    {
        Life?.Shutdown();
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
                rec.Background = BackColor1;
                window.TransparencyLevelHint = (WindowTransparencyLevel)
                    (GuiConfigUtils.Config.WindowTranType + 1);
            }
            else
            {
                window.TransparencyLevelHint = WindowTransparencyLevel.None;
                rec.Background = BackColor;
            }
        }
    }
}
