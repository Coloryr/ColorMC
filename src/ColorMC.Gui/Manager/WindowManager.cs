using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Config;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UI.Controls.BuildPack;
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

namespace ColorMC.Gui.Manager;

/// <summary>
/// 窗口管理
/// </summary>
public static class WindowManager
{
    /// <summary>
    /// 上一个窗口
    /// </summary>
    public static Window? LastWindow { get; private set; }

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
    /// 生成客户端包窗口
    /// </summary>
    public static BuildPackControl? BuildPackWindow { get; set; }

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

    private static readonly List<AMultiWindow> _windows = [];

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
        s_file = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameWindowFile);
        LoadState();
        if (s_WindowState == null)
        {
            s_WindowState = [];
            SaveState();
        }
        //单窗口模式
        if (ConfigBinding.WindowMode())
        {
#if Phone
            AllWindow = new();
            AllWindow.Model.HeadDisplay = false;
            AllWindow.WindowOpened();
#else
            if (SystemInfo.Os == OsType.Linux ||
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
#endif
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
                using var stream = PathHelper.OpenRead(s_file);
                var state = JsonUtils.ToObj(stream, JsonGuiType.DictionaryStringWindowStateObj);
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
        ConfigSave.AddItem(ConfigSaveObj.Build("ColorMC_Window", s_file, s_WindowState, JsonGuiType.DictionaryStringWindowStateObj));
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

    /// <summary>
    /// 设置窗口状态
    /// </summary>
    /// <param name="name"></param>
    /// <param name="state"></param>
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

    /// <summary>
    /// 找到基础窗口
    /// </summary>
    /// <param name="con"></param>
    /// <returns></returns>
    public static IBaseWindow? FindRoot(object? con)
    {
        if (con == null)
        {
            return null;
        }
        if (con is SingleControl all)
        {
            return all;
        }
        else if (GuiConfigUtils.Config.WindowMode)
        {
            return AllWindow!;
        }
        else if (con is IBaseWindow win)
        {
            return win;
        }
        else if (con is BaseUserControl con1)
        {
            return con1.Window;
        }

        return AllWindow!;
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="con">窗口</param>
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
        _windows.Add(win);
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="con">窗口</param>
    /// <param name="newwindow">是否独立打开</param>
    public static void AWindow(BaseUserControl con, bool newwindow = false)
    {
        if (ConfigBinding.WindowMode())
        {
            if (newwindow)
            {
#if Phone
                return;
#endif
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

    /// <summary>
    /// 显示自定义窗口
    /// </summary>
    /// <param name="test">是否为测试模式</param>
    /// <returns>是否启动成功</returns>
    public static bool ShowCustom(bool test = false)
    {
        if (CustomWindow != null)
        {
            CustomWindow.Icon.Window?.WindowActivate();
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
            //加载dll
            string file = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameCustomUIFile);
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
                AWindow(dll.Icon, test);
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ColorMC", "custom");
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, "custom");
                    CustomWindow?.Icon.Window?.Model.Show(App.Lang("WindowManager.Info1"));
                }
                return true;
            }
            else
            {
                dll.Unload();
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

    /// <summary>
    /// 显示生成客户端包窗口
    /// </summary>
    public static void ShowBuildPack()
    {
        if (BuildPackWindow != null)
        {
            BuildPackWindow.Window?.WindowActivate();
        }
        else
        {
            BuildPackWindow = new();
            AWindow(BuildPackWindow);
        }
    }

    /// <summary>
    /// 显示添加游戏实例窗口
    /// </summary>
    /// <param name="group">添加的游戏分组</param>
    /// <param name="isDir">是否为目录</param>
    /// <param name="file">位置</param>
    public static void ShowAddGame(string? group, bool isDir = false, string? file = null)
    {
        if (AddGameWindow != null)
        {
            AddGameWindow.Window?.WindowActivate();
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

    /// <summary>
    /// 显示下载窗口
    /// </summary>
    /// <returns>Gui调用参数</returns>
    public static DownloadArg ShowDownload()
    {
        return Dispatcher.UIThread.Invoke(() =>
        {
            if (DownloadWindow != null)
            {
                DownloadWindow.Window?.WindowActivate();
            }
            else
            {
                DownloadWindow = new();
                AWindow(DownloadWindow);
            }

            return DownloadWindow.Start();
        });
    }

    /// <summary>
    /// 显示用户窗口
    /// </summary>
    /// <param name="add">是否跳转到添加</param>
    /// <param name="relogin">是否为重新登录</param>
    /// <param name="url">添加的自定义验证地址</param>
    public static void ShowUser(bool add = false, bool relogin = false, string? url = null)
    {
        if (UserWindow != null)
        {
            UserWindow.Window?.WindowActivate();
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

    /// <summary>
    /// 显示主窗口
    /// </summary>
    public static void ShowMain()
    {
        if (MainWindow != null)
        {
            MainWindow.Window?.WindowActivate();
        }
        else
        {
            MainWindow = new();
            AWindow(MainWindow);
        }
    }

    /// <summary>
    /// 显示添加整合包窗口
    /// </summary>
    public static void ShowAddModPack()
    {
        if (AddModPackWindow != null)
        {
            AddModPackWindow.Window?.WindowActivate();
        }
        else
        {
            AddModPackWindow = new();
            AWindow(AddModPackWindow);
        }
    }

    /// <summary>
    /// 显示启动器设置窗口
    /// </summary>
    /// <param name="type">设置项目</param>
    /// <param name="value">传入值</param>
    public static void ShowSetting(SettingType type, int value = 0)
    {
        if (SettingWindow != null)
        {
            SettingWindow.Window?.WindowActivate();
        }
        else
        {
            SettingWindow = new(value);
            AWindow(SettingWindow);
        }

        SettingWindow?.GoTo(type);
    }

    /// <summary>
    /// 显示皮肤窗口
    /// </summary>
    public static void ShowSkin()
    {
        if (SkinWindow != null)
        {
            SkinWindow.Window?.WindowActivate();
        }
        else
        {
            SkinWindow = new();
            AWindow(SkinWindow);
        }
    }

    /// <summary>
    /// 显示游戏实例编辑窗口
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="type">需要编辑的项目</param>
    public static void ShowGameEdit(GameSettingObj obj, GameEditWindowType type
        = GameEditWindowType.Normal)
    {
        if (GameEditWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Window?.WindowActivate();
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

    /// <summary>
    /// 显示添加Java窗口
    /// </summary>
    /// <param name="version">Java主版本</param>
    public static void ShowAddJava(int version)
    {
        if (AddJavaWindow != null)
        {
            AddJavaWindow.Window?.WindowActivate();
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

    /// <summary>
    /// 显示添加游戏资源窗口
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="obj1">模组项目</param>
    public static void ShowAdd(GameSettingObj obj, ModDisplayModel obj1)
    {
        var type1 = GameDownloadHelper.TestSourceType(obj1.PID, obj1.FID);

        if (GameAddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window?.WindowActivate();
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
    /// <summary>
    /// 显示添加资源窗口，并进入标记模式
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static void ShowAddSet(GameSettingObj obj)
    {
        if (GameAddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window?.WindowActivate();
            value.GoSet();
        }
        else
        {
            var con = new AddControl(obj);
            GameAddWindows.Add(obj.UUID, con);
            AWindow(con);
            con.GoSet();
        }
    }
    /// <summary>
    /// 显示添加资源窗口，并跳转到模组升级
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="list">需要升级的模组</param>
    public static void ShowAddUpgade(GameSettingObj obj, ICollection<ModUpgradeModel> list)
    {
        if (GameAddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window?.WindowActivate();
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
    /// <summary>
    /// 显示添加资源窗口
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="type">添加的类型</param>
    public static void ShowAdd(GameSettingObj obj, FileType type)
    {
        if (GameAddWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window?.WindowActivate();
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

    /// <summary>
    /// 显示生成服务器包窗口
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ShowServerPack(GameSettingObj obj)
    {
        if (ServerPackWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window?.WindowActivate();
        }
        else
        {
            var con = new ServerPackControl(obj);
            ServerPackWindows.Add(obj.UUID, con);
            AWindow(con);
        }
    }

    /// <summary>
    /// 显示收藏窗口
    /// </summary>
    public static void ShowCollect()
    {
        if (CollectWindow != null)
        {
            CollectWindow.Window?.WindowActivate();
        }
        else
        {
            CollectWindow = new();
            AWindow(CollectWindow);
        }
    }

    /// <summary>
    /// 显示游戏日志窗口
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="code">游戏退出码</param>
    public static void ShowGameLog(GameSettingObj obj, int code = 0)
    {
        if (GameLogWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window?.WindowActivate();
            value.GameExit(code);
        }
        else
        {
            var con = new GameLogControl(obj);
            GameLogWindows.Add(obj.UUID, con);
            AWindow(con);
            con.GameExit(code);
        }
    }

    /// <summary>
    /// 显示游戏实例配置文件修改窗口
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ShowConfigEdit(GameSettingObj obj)
    {
        if (GameConfigEditWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Window?.WindowActivate();
        }
        else
        {
            var con = new GameConfigEditControl(obj);
            GameConfigEditWindows.Add(obj.UUID, con);
            AWindow(con);
        }
    }

    /// <summary>
    /// 显示游戏实例配置修改窗口
    /// </summary>
    /// <param name="obj">存档</param>
    public static void ShowConfigEdit(WorldObj obj)
    {
        string key = obj.Game.UUID + ":" + obj.LevelName;
        if (GameConfigEditWindows.TryGetValue(key, out var win1))
        {
            win1.Window?.WindowActivate();
        }
        else
        {
            var con = new GameConfigEditControl(obj);
            GameConfigEditWindows.Add(key, con);
            AWindow(con);
        }
    }

    /// <summary>
    /// 显示游戏实例云储存窗口
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="world">是否为保存</param>
    public static void ShowGameCloud(GameSettingObj obj, bool world = false)
    {
        string key = obj.UUID;
        if (GameCloudWindows.TryGetValue(key, out var win1))
        {
            win1.Window?.WindowActivate();
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

    /// <summary>
    /// 显示游戏统计窗口
    /// </summary>
    public static void ShowCount()
    {
        if (CountWindow != null)
        {
            CountWindow.Window?.WindowActivate();
        }
        else
        {
            CountWindow = new();
            AWindow(CountWindow);
        }
    }

    /// <summary>
    /// 显示游戏导出窗口
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ShowGameExport(GameSettingObj obj)
    {
        if (GameExportWindows.TryGetValue(obj.UUID, out var value))
        {
            value.Window?.WindowActivate();
        }
        else
        {
            var con = new GameExportControl(obj);
            GameExportWindows.Add(obj.UUID, con);
            AWindow(con);
        }
    }

    /// <summary>
    /// 显示映射联机窗口
    /// </summary>
    public static void ShowNetFrp()
    {
        if (NetFrpWindow != null)
        {
            NetFrpWindow.Window?.WindowActivate();
        }
        else
        {
            NetFrpWindow = new();
            AWindow(NetFrpWindow);
        }
    }

    /// <summary>
    /// 显示Minecraft News窗口
    /// </summary>
    public static void ShowNews()
    {
        if (NewsWindow != null)
        {
            NewsWindow.Window?.WindowActivate();
        }
        else
        {
            NewsWindow = new();
            AWindow(NewsWindow);
        }
    }

    /// <summary>
    /// 显示错误
    /// </summary>
    /// <param name="data"></param>
    /// <param name="e"></param>
    /// <param name="close"></param>
    public static void ShowError(string? data, Exception? e, bool close = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var con = new ErrorControl(data, e, close);
            AWindow(con);
        });
    }
    /// <summary>
    /// 显示错误
    /// </summary>
    /// <param name="data"></param>
    /// <param name="e"></param>
    /// <param name="close"></param>
    public static void ShowError(string data, string e, bool close = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var con = new ErrorControl(data, e, close);
            AWindow(con);
        });
    }

    /// <summary>
    /// 关闭该游戏实例的所有窗口
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void CloseGameWindow(GameSettingObj obj)
    {
        if (GameEditWindows.TryGetValue(obj.UUID, out var win))
        {
            win.Window?.Close();
        }
        if (GameLogWindows.TryGetValue(obj.UUID, out var win5))
        {
            win5.Window?.Close();
        }
        if (GameAddWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Window?.Close();
        }
        if (GameCloudWindows.TryGetValue(obj.UUID, out var win2))
        {
            win2.Window?.Close();
        }
        if (GameExportWindows.TryGetValue(obj.UUID, out var win3))
        {
            win3.Window?.Close();
        }
        if (ServerPackWindows.TryGetValue(obj.UUID, out var win4))
        {
            win4.Window?.Close();
        }
        foreach (var item in GameConfigEditWindows)
        {
            if (item.Key.StartsWith(obj.UUID))
            {
                item.Value.Window?.Close();
            }
        }
    }

    /// <summary>
    /// 获取当前主窗口
    /// </summary>
    /// <returns>窗口</returns>
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
                return CustomWindow.Icon.Window;
            }
        }

        return MainWindow.Window;
    }

    /// <summary>
    /// 更新窗口信息
    /// </summary>
    /// <param name="model">窗口模型</param>
    public static void UpdateWindow(BaseModel model)
    {
        model.BackImage = ImageManager.BackBitmap;
        //更新背景图
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

        //更新透明度
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

    /// <summary>
    /// 还原显示主窗口
    /// </summary>
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
            else if (CustomWindow?.Icon.GetVisualRoot() is Window window1)
            {
                window1.Show();
                window1.WindowState = WindowState.Normal;
                window1.Activate();
            }
        }
    }

    /// <summary>
    /// 隐藏主窗口
    /// </summary>
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
                (CustomWindow?.Icon.GetVisualRoot() as Window)?.Close();
            }
            else if (CustomWindow?.Icon.GetVisualRoot() is Window window1)
            {
                window1.Hide();
            }
            CloseAllWindow();
        }
    }

    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    public static void CloseAllWindow()
    {
        foreach (var item in _windows)
        {
            item.Close();
        }
    }

    /// <summary>
    /// 清理窗口状态
    /// </summary>
    public static void Reset()
    {
        s_WindowState.Clear();
        SaveState();
    }

    /// <summary>
    /// 获取窗口ID
    /// </summary>
    /// <typeparam name="T">窗口类型</typeparam>
    /// <returns>ID</returns>
    public static string GetUseName<T>() where T : BaseUserControl
    {
        return typeof(T).FullName ?? typeof(T).Name;
    }

    /// <summary>
    /// 获取窗口ID
    /// </summary>
    /// <typeparam name="T">窗口类型</typeparam>
    /// <param name="obj">游戏实例</param>
    /// <returns>ID</returns>
    public static string GetUseName<T>(GameSettingObj obj) where T : BaseUserControl
    {
        return (typeof(T).FullName ?? typeof(T).Name) + ":" + obj.UUID;
    }

    /// <summary>
    /// 更新窗口图标
    /// </summary>
    public static void ReloadIcon()
    {
        foreach (var item in _windows)
        {
            item.ReloadIcon();
        }
    }

    /// <summary>
    /// 窗口关闭
    /// </summary>
    /// <param name="window"></param>
    public static void ClosedWindow(AMultiWindow window)
    {
        if (LastWindow == window)
        {
            LastWindow = null;
        }

        _windows.Remove(window);
    }

    /// <summary>
    /// 窗口激活时
    /// </summary>
    /// <param name="window">窗口</param>
    public static void ActivatedWindow(Window window)
    {
        LastWindow = window;
    }

    /// <summary>
    /// 重载窗口标题
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ReloadTitle(GameSettingObj obj)
    {
        if (GameEditWindows.TryGetValue(obj.UUID, out var window))
        {
            window.ReloadTitle();
        }
        if (GameExportWindows.TryGetValue(obj.UUID, out var window1))
        {
            window1.ReloadTitle();
        }
        if (GameCloudWindows.TryGetValue(obj.UUID, out var window2))
        {
            window2.ReloadTitle();
        }
        if (GameLogWindows.TryGetValue(obj.UUID, out var window3))
        {
            window3.ReloadTitle();
        }
        if (GameConfigEditWindows.TryGetValue(obj.UUID, out var window4))
        {
            window4.ReloadTitle();
        }
        if (GameAddWindows.TryGetValue(obj.UUID, out var window5))
        {
            window5.ReloadTitle();
        }
        if (ServerPackWindows.TryGetValue(obj.UUID, out var window6))
        {
            window6.ReloadTitle();
        }
    }

    /// <summary>
    /// 重载窗口图标
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ReloadIcon(GameSettingObj obj)
    {
        if (GameEditWindows.TryGetValue(obj.UUID, out var window))
        {
            window.ReloadIcon();
        }
        if (GameExportWindows.TryGetValue(obj.UUID, out var window1))
        {
            window1.ReloadIcon();
        }
        if (GameCloudWindows.TryGetValue(obj.UUID, out var window2))
        {
            window2.ReloadIcon();
        }
        if (GameLogWindows.TryGetValue(obj.UUID, out var window3))
        {
            window3.ReloadIcon();
        }
        if (GameConfigEditWindows.TryGetValue(obj.UUID, out var window4))
        {
            window4.ReloadIcon();
        }
        if (GameAddWindows.TryGetValue(obj.UUID, out var window5))
        {
            window5.ReloadIcon();
        }
        if (ServerPackWindows.TryGetValue(obj.UUID, out var window6))
        {
            window6.ReloadIcon();
        }
    }
}
