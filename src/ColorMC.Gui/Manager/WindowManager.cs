using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls;
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


namespace ColorMC.Gui.Manager;

public static class WindowManager
{
    public static Window? LastWindow { get; set; }

    public static AllControl? AllWindow { get; set; }
    public static DownloadControl? DownloadWindow { get; set; }
    public static UsersControl? UserWindow { get; set; }
    public static MainControl? MainWindow { get; set; }
    public static AddGameControl? AddGameWindow { get; set; }
    public static DllAssembly? CustomWindow { get; set; }
    public static AddModPackControl? AddModPackWindow { get; set; }
    public static SettingControl? SettingWindow { get; set; }
    public static SkinControl? SkinWindow { get; set; }
    public static AddJavaControl? AddJavaWindow { get; set; }
    public static CountControl? CountWindow { get; set; }
    public static NetFrpControl? NetFrpWindow { get; set; }

    public static Dictionary<string, GameEditControl> GameEditWindows { get; } = [];
    public static Dictionary<string, GameConfigEditControl> ConfigEditWindows { get; } = [];
    public static Dictionary<string, AddControl> AddWindows { get; } = [];
    public static Dictionary<string, ServerPackControl> ServerPackWindows { get; } = [];
    public static Dictionary<string, GameLogControl> GameLogWindows { get; } = [];
    public static Dictionary<string, GameExportControl> GameExportWindows { get; } = [];
    public static Dictionary<string, GameCloudControl> GameCloudWindows { get; } = [];

    private static readonly WindowTransparencyLevel[] WindowTran =
    [
        WindowTransparencyLevel.None,
        WindowTransparencyLevel.Transparent,
        WindowTransparencyLevel.Blur,
        WindowTransparencyLevel.AcrylicBlur,
        WindowTransparencyLevel.Mica
    ];

    public static void StartWindow()
    {
        if (ConfigBinding.WindowMode())
        {
            if (SystemInfo.Os == OsType.Android)
            {
                AllWindow = new();
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

        if (!ShowCustom())
        {
            ShowMain();
        }
    }

    public static IBaseWindow FindRoot(object? con)
    {
        if (con is AllControl all)
            return all;
        else if (GuiConfigUtils.Config.WindowMode)
            return AllWindow!;
        else if (con is IBaseWindow win)
            return win;
        else if (con is BaseUserControl con1)
            return con1.Window;

        return AllWindow!;
    }

    public static bool ShowCustom(bool test = false)
    {
        if (CustomWindow != null)
        {
            CustomWindow.Window.Window.TopActivate();
            return true;
        }

        if (!test)
        {
            var config = GuiConfigUtils.Config.ServerCustom;
            if (config == null || config?.EnableUI == false)
            {
                return false;
            }
        }

        try
        {
            string file = BaseBinding.GetRunDir() + "ColorMC.CustomGui.dll";
            if (!File.Exists(file))
            {
                return false;
            }

            var dll = new DllAssembly();

            if (dll.IsLoad)
            {
                if (!test)
                {
                    CustomWindow = dll;
                }
                AWindow(dll.Window, test);
            }
            return true;
        }
        catch (Exception e)
        {
            var data = App.Lang("Gui.Error10");
            Logs.Error(data, e);
            ShowError(data, e, true);
        }

        return false;
    }

    public static void AWindow(BaseUserControl con, bool newwindow = false)
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
            App.TopLevel ??= win;
            con.SetBaseModel(win.Model);
            win.Show();
        }
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

    public static DownloadArg ShowDownload()
    {
        return Dispatcher.UIThread.Invoke(() =>
        {
            if (DownloadWindow != null)
            {
                DownloadWindow.Window.TopActivate();
            }
            else
            {
                DownloadWindow = new();
                AWindow(DownloadWindow);
            }

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

    public static void ShowSetting(SettingType type, int value = 0)
    {
        if (SettingWindow != null)
        {
            SettingWindow.Window.TopActivate();
        }
        else
        {
            SettingWindow = new(value);
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

    public static void ShowAddJava(int version)
    {
        if (AddJavaWindow != null)
        {
            AddJavaWindow.Window.TopActivate();
        }
        else
        {
            AddJavaWindow = new()
            {
                NeedJava = version
            };
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
                return CustomWindow.Window.Window;
            }
        }

        return MainWindow.Window;
    }

    public static void UpdateWindow(BaseModel model)
    {
        if (GuiConfigUtils.Config != null)
        {
            model.Background = GuiConfigUtils.Config.WindowTran ?
                ThemeManager.GetColor("WindowTranColor") : Brushes.White;
            model.Back = ImageManager.BackBitmap;
            if (ImageManager.BackBitmap != null)
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
                        App.ThisApp.PlatformSettings!.GetColorValues().ThemeVariant ==
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

    public static void Show()
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
            else if (CustomWindow?.Window.GetVisualRoot() is Window window1)
            {
                window1.Show();
                window1.WindowState = WindowState.Normal;
                window1.Activate();
            }
        }
    }

    public static void Hide()
    {
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
                (CustomWindow?.Window.GetVisualRoot() as Window)?.Close();
            }
            else if (CustomWindow?.Window.GetVisualRoot() is Window window1)
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
}
