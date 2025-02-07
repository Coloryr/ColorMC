using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UI.Controls.Collect;
using ColorMC.Gui.UI.Controls.Count;
using ColorMC.Gui.UI.Controls.Download;
using ColorMC.Gui.UI.Controls.Error;
using ColorMC.Gui.UI.Controls.GameCloud;
using ColorMC.Gui.UI.Controls.GameConfigEdit;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UI.Controls.GameExport;
using ColorMC.Gui.UI.Controls.GameLog;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Controls.NetFrp;
using ColorMC.Gui.UI.Controls.News;
using ColorMC.Gui.UI.Controls.ServerPack;
using ColorMC.Gui.UI.Controls.Setting;
using ColorMC.Gui.UI.Controls.Skin;
using ColorMC.Gui.UI.Controls.User;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using Newtonsoft.Json;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 窗口管理
/// </summary>
public static class WindowManager
{
    public const string Name1 = "window.json";

    /// <summary>
    /// 上一个窗口
    /// </summary>
    public static Window? LastWindow { get; set; }

    /// <summary>
    /// 单窗口
    /// </summary>
    public static SingleControl? AllWindow { get; set; }
    /// <summary>
    /// 下载窗口
    /// </summary>
    public static DownloadControl? DownloadWindow { get; set; }
    /// <summary>
    /// 用户窗口
    /// </summary>
    public static UsersControl? UserWindow { get; set; }
    /// <summary>
    /// 主窗口
    /// </summary>
    public static MainControl? MainWindow { get; set; }
    /// <summary>
    /// 添加游戏实例窗口
    /// </summary>
    public static AddGameControl? AddGameWindow { get; set; }
    /// <summary>
    /// 自定义启动窗口
    /// </summary>
    public static UIAssembly? CustomWindow { get; set; }
    /// <summary>
    /// 下载整合包窗口
    /// </summary>
    public static AddModPackControl? AddModPackWindow { get; set; }
    /// <summary>
    /// 设置窗口
    /// </summary>
    public static SettingControl? SettingWindow { get; set; }
    /// <summary>
    /// 皮肤查看窗口
    /// </summary>
    public static SkinControl? SkinWindow { get; set; }
    /// <summary>
    /// 下载JAVA窗口
    /// </summary>
    public static AddJavaControl? AddJavaWindow { get; set; }
    /// <summary>
    /// 启动统计窗口
    /// </summary>
    public static CountControl? CountWindow { get; set; }
    /// <summary>
    /// 映射窗口
    /// </summary>
    public static NetFrpControl? NetFrpWindow { get; set; }
    /// <summary>
    /// 新闻窗口
    /// </summary>
    public static MinecraftNewsControl? NewsWindow { get; set; }
    /// <summary>
    /// 收藏窗口
    /// </summary>
    public static CollectControl? CollectWindow { get; set; }

    /// <summary>
    /// 游戏实例编辑窗口
    /// </summary>
    public static Dictionary<string, GameEditControl> GameEditWindows { get; } = [];
    /// <summary>
    /// 游戏实例配置修改窗口
    /// </summary>
    public static Dictionary<string, GameConfigEditControl> GameConfigEditWindows { get; } = [];
    /// <summary>
    /// 游戏实例添加资源窗口
    /// </summary>
    public static Dictionary<string, AddControl> GameAddWindows { get; } = [];
    /// <summary>
    /// 游戏实例生成服务器整合包窗口
    /// </summary>
    public static Dictionary<string, ServerPackControl> ServerPackWindows { get; } = [];
    /// <summary>
    /// 游戏实例日志窗口
    /// </summary>
    public static Dictionary<string, GameLogControl> GameLogWindows { get; } = [];
    /// <summary>
    /// 游戏实例导出窗口
    /// </summary>
    public static Dictionary<string, GameExportControl> GameExportWindows { get; } = [];
    /// <summary>
    /// 游戏实例云存档窗口
    /// </summary>
    public static Dictionary<string, GameCloudControl> GameCloudWindows { get; } = [];

    /// <summary>
    /// 窗口透明选项
    /// </summary>
    private static readonly WindowTransparencyLevel[] WindowTran =
    [
        WindowTransparencyLevel.None,
        WindowTransparencyLevel.Transparent,
        WindowTransparencyLevel.Blur,
        WindowTransparencyLevel.AcrylicBlur,
        WindowTransparencyLevel.Mica
    ];

    /// <summary>
    /// 窗口位置信息
    /// </summary>
    private static Dictionary<string, WindowStateObj> s_WindowState;

    private static string s_file;

    /// <summary>
    /// 初始化窗口位置信息
    /// </summary>
    public static void Init()
    {
        s_file = Path.Combine(ColorMCGui.RunDir, Name1);
        LoadState();
        if (s_WindowState == null)
        {
            s_WindowState = [];
            SaveState();
        }
        //单窗口模式
        if (ConfigBinding.WindowMode())
        {
            if (SystemInfo.Os == OsType.Android)
            {
                AllWindow = new();
                AllWindow.Model.HeadDisplay = false;
                AllWindow.TopOpened();
            }
            else if (SystemInfo.Os == OsType.Linux ||
                (SystemInfo.Os == OsType.Windows && !SystemInfo.IsWin11))
            {
                var win = new SingleBorderWindow();
                AllWindow = win.Win;
                win.Show();
            }
            else
            {
                var win = new SingleWindow();
                AllWindow = win.Win;
                win.Show();
            }
        }

        //长按取消处理
        InputElement.PointerReleasedEvent.AddClassHandler<DataGridCell>((x, e) =>
        {
            LongPressed.Released();
        }, handledEventsToo: true);

        //显示自定义窗口
        if (!ShowCustom())
        {
            //显示主窗口
            ShowMain();
        }
    }

    /// <summary>
    /// 加载窗口位置文件
    /// </summary>
    private static void LoadState()
    {
        if (File.Exists(s_file))
        {
            try
            {
                var data = PathHelper.ReadText(s_file);
                if (data == null)
                {
                    return;
                }
                var state = JsonConvert.DeserializeObject<Dictionary<string, WindowStateObj>>(data);
                if (state == null)
                {
                    return;
                }

                s_WindowState = state;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("App.Error3"), e);
            }
        }
    }

    /// <summary>
    /// 保存窗口状态
    /// </summary>
    private static void SaveState()
    {
        ConfigSave.AddItem(new()
        {
            Name = "ColorMC_Window",
            File = s_file,
            Obj = s_WindowState
        });
    }

    /// <summary>
    /// 获取窗口状态
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static WindowStateObj? GetWindowState(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        if (s_WindowState.TryGetValue(name, out var state))
        {
            return state;
        }

        return null;
    }

    public static void SaveWindowState(string name, WindowStateObj state)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }
        if (!s_WindowState.TryAdd(name, state))
        {
            s_WindowState[name] = state;
        }

        SaveState();
    }

    public static IBaseWindow FindRoot(object? con)
    {
        if (con is SingleControl all)
            return all;
        else if (GuiConfigUtils.Config.WindowMode)
            return AllWindow!;
        else if (con is IBaseWindow win)
            return win;
        else if (con is BaseUserControl con1)
            return con1.Window;

        return AllWindow!;
    }

    private static void ShowWindow(BaseUserControl con)
    {
        AMultiWindow win;
        if (SystemInfo.Os == OsType.Linux ||
            (SystemInfo.Os == OsType.Windows && !SystemInfo.IsWin11))
        {
            win = new MultiBorderWindow(con);
        }
        else
        {
            win = new MultiWindow(con);
        }
        win.Show();
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

                ShowWindow(con);
            }
            else
            {
                con.SetBaseModel(AllWindow!.Model);
                AllWindow.Add(con);
            }
        }
        else
        {
            ShowWindow(con);
        }
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

            var dll = new UIAssembly();

            if (dll.IsLoad)
            {
                if (!test)
                {
                    CustomWindow = dll;
                }
                AWindow(dll.Window, test);
                var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/ColorMC/custom";
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, "custom");
                    CustomWindow?.Window.Window.Model.Show(App.Lang("WindowManager.Info1"));
                }
                return true;
            }
        }
        catch (Exception e)
        {
            var data = App.Lang("WindowManager.Error1");
            Logs.Error(data, e);
            ShowError(data, e, !test);
        }

        return false;
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

    public static void ShowUser(bool add = false, bool relogin = false, string? url = null)
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
        if (relogin)
        {
            UserWindow?.Relogin();
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
        var type1 = DownloadItemHelper.TestSourceType(obj1.PID, obj1.FID);

        if (GameAddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.TopActivate();
            value.GoFile(type1, obj1.PID!);
        }
        else
        {
            var con = new AddControl(obj);
            GameAddWindows.Add(obj.UUID, con);
            AWindow(con);
            con.GoFile(type1, obj1.PID!);
        }
    }

    public static Task ShowAddSet(GameSettingObj obj)
    {
        if (GameAddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.TopActivate();
            return value.GoSet();
        }
        else
        {
            var con = new AddControl(obj);
            GameAddWindows.Add(obj.UUID, con);
            AWindow(con);
            return con.GoSet();
        }
    }

    public static void ShowAddUpgade(GameSettingObj obj, ICollection<ModUpgradeModel> list)
    {
        if (GameAddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.TopActivate();
            value.GoUpgrade(list);
        }
        else
        {
            var con = new AddControl(obj);
            GameAddWindows.Add(obj.UUID, con);
            AWindow(con);
            con.GoUpgrade(list);
        }
    }

    public static void ShowAdd(GameSettingObj obj, FileType type)
    {
        if (GameAddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.TopActivate();
            value.GoTo(type);
        }
        else
        {
            var con = new AddControl(obj);
            GameAddWindows.Add(obj.UUID, con);
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

    public static void ShowCollect()
    {
        if (CollectWindow != null)
        {
            CollectWindow.Window.TopActivate();
        }
        else
        {
            CollectWindow = new();
            AWindow(CollectWindow);
        }
    }

    public static void ShowGameLog(GameSettingObj obj, int code = 0)
    {
        if (GameLogWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window.TopActivate();
            value.GameCrash(code);
        }
        else
        {
            var con = new GameLogControl(obj);
            GameLogWindows.Add(obj.UUID, con);
            AWindow(con);
            con.GameCrash(code);
        }
    }

    public static void ShowConfigEdit(GameSettingObj obj)
    {
        if (GameConfigEditWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Window.TopActivate();
        }
        else
        {
            var con = new GameConfigEditControl(obj);
            GameConfigEditWindows.Add(obj.UUID, con);
            AWindow(con);
        }
    }

    public static void ShowConfigEdit(WorldObj obj)
    {
        string key = obj.Game.UUID + ":" + obj.LevelName;
        if (GameConfigEditWindows.TryGetValue(key, out var win1))
        {
            win1.Window.TopActivate();
        }
        else
        {
            var con = new GameConfigEditControl(obj);
            GameConfigEditWindows.Add(key, con);
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

    public static void ShowNews()
    {
        if (NewsWindow != null)
        {
            NewsWindow.Window.TopActivate();
        }
        else
        {
            NewsWindow = new();
            AWindow(NewsWindow);
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
            win.Window.Close();
        }
        if (GameLogWindows.TryGetValue(obj.UUID, out var win5))
        {
            win5.Window.Close();
        }
        if (GameAddWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Window.Close();
        }
        if (GameCloudWindows.TryGetValue(obj.UUID, out var win2))
        {
            win2.Window.Close();
        }
        if (GameExportWindows.TryGetValue(obj.UUID, out var win3))
        {
            win3.Window.Close();
        }
        if (ServerPackWindows.TryGetValue(obj.UUID, out var win4))
        {
            win4.Window.Close();
        }
        foreach (var item in GameConfigEditWindows)
        {
            if (item.Key.StartsWith(obj.UUID))
            {
                item.Value.Window.Close();
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

        model.Theme = ThemeManager.NowTheme ==
            PlatformThemeVariant.Light ? ThemeVariant.Light : ThemeVariant.Dark;
    }

    public static void Show()
    {
        if (ConfigBinding.WindowMode())
        {
            MainWindow?.Show();
            if (AllWindow?.GetVisualRoot() is Window window)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        }
        else
        {
            MainWindow?.Show();
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
            MainWindow?.Hide();
            if (AllWindow?.GetVisualRoot() is Window window)
            {
                window.Hide();
            }
        }
        else
        {
            MainWindow?.Hide();
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
        foreach (var item in GameConfigEditWindows.Values)
        {
            (item.GetVisualRoot() as Window)?.Close();
        }
        foreach (var item in GameAddWindows.Values)
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

    public static void Reset()
    {
        s_WindowState.Clear();
        SaveState();
    }

    public static string GetUseName<T>() where T : BaseUserControl
    {
        return typeof(T).FullName ?? typeof(T).Name;
    }

    public static string GetUseName<T>(GameSettingObj obj) where T : BaseUserControl
    {
        return typeof(T).FullName ?? typeof(T).Name + ":" + obj.UUID;
    }
}
