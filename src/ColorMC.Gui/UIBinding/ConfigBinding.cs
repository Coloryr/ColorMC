using System.Collections.Generic;
using System.Threading.Tasks;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.MusicPlayer;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UIBinding;

public static class ConfigBinding
{
    /// <summary>
    /// 加载账户数据库
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool LoadAuthDatabase(string dir)
    {
        return AuthDatabase.LoadData(dir);
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool LoadConfig(string dir)
    {
        var res = ConfigUtils.Load(dir, true);
        if (res)
        {
            CoreHttpClient.Init();
        }

        return res;
    }
    /// <summary>
    /// 加载图形配置文件
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool LoadGuiConfig(string dir)
    {
        var res = GuiConfigUtils.Load(dir, true);
        if (res)
        {
            ThemeManager.Init();
        }

        return res;
    }

    /// <summary>
    /// 加载Frp配置文件
    /// </summary>
    /// <param name="local"></param>
    /// <returns></returns>
    public static bool LoadFrpConfig(string local)
    {
        return FrpConfigUtils.Load(local, true);
    }

    /// <summary>
    /// 设置RGB模式
    /// </summary>
    public static void SetRgb(bool enable)
    {
        GuiConfigUtils.Config.RGB = enable;
        GuiConfigUtils.Save();

        ThemeManager.Init();
    }

    /// <summary>
    /// 设置RGB值
    /// </summary>
    public static void SetRgb(int v1, int v2)
    {
        GuiConfigUtils.Config.RGBS = v1;
        GuiConfigUtils.Config.RGBV = v2;
        GuiConfigUtils.Save();

        ThemeManager.Init();
    }

    /// <summary>
    /// 设置启动器颜色
    /// </summary>
    public static void SetColor(string main)
    {
        GuiConfigUtils.Config.ColorMain = main;
        GuiConfigUtils.Save();

        ThemeManager.Init();
    }

    /// <summary>
    /// 重置启动器颜色
    /// </summary>
    public static void ResetColor()
    {
        SetColor(ThemeManager.MainColorStr);
    }

    /// <summary>
    /// 删除背景图片
    /// </summary>
    public static void DeleteGuiImageConfig()
    {
        GuiConfigUtils.Config.BackImage = null;
        GuiConfigUtils.Save();

        ImageManager.RemoveImage();
    }

    /// <summary>
    /// 设置背景图片
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task SetBackPic(bool en, string? dir, int data)
    {
        GuiConfigUtils.Config.EnableBG = en;
        GuiConfigUtils.Config.BackEffect = data;
#if Phone
        if (SystemInfo.Os == OsType.Android)
        {
            var bg = await PathBinding.CopyBG(dir!);
            GuiConfigUtils.Config.BackImage = bg;
        }
#else
        GuiConfigUtils.Config.BackImage = dir;
#endif

        GuiConfigUtils.Save();

        await ImageManager.LoadBGImage();
    }

    /// <summary>
    /// 设置背景图片分辨率限制
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="pix"></param>
    /// <returns></returns>
    public static async Task SetBackLimit(bool enable, int pix)
    {
        GuiConfigUtils.Config.BackLimitValue = pix;
        GuiConfigUtils.Config.BackLimit = enable;
        GuiConfigUtils.Save();

        await ImageManager.LoadBGImage();
    }

    /// <summary>
    /// 设置背景图片透明度
    /// </summary>
    /// <param name="data"></param>
    public static void SetBackTran(int data)
    {
        GuiConfigUtils.Config.BackTran = data;
        GuiConfigUtils.Save();

        ImageManager.OnPicUpdate();
    }

    /// <summary>
    /// 设置窗口透明
    /// </summary>
    /// <param name="open"></param>
    /// <param name="type"></param>
    public static void SetWindowTran(bool open, int type)
    {
        GuiConfigUtils.Config.WindowTranType = type;
        GuiConfigUtils.Config.WindowTran = open;
        GuiConfigUtils.Save();

        ThemeManager.Init();
        ImageManager.OnPicUpdate();
    }

    /// <summary>
    /// 设置下载源
    /// </summary>
    /// <param name="value"></param>
    public static void SetDownloadSource(SourceLocal value)
    {
        if (BaseBinding.IsDownload)
        {
            return;
        }

        ConfigUtils.Config.Http ??= ConfigUtils.MakeHttpConfig();
        ConfigUtils.Config.Http.Source = value;
        ConfigUtils.Save();

        CoreHttpClient.Init();
    }

    /// <summary>
    /// 设置下载线程数
    /// </summary>
    /// <param name="value"></param>
    public static void SetDownloadThread(int value)
    {
        if (BaseBinding.IsDownload)
        {
            return;
        }

        ConfigUtils.Config.Http ??= ConfigUtils.MakeHttpConfig();
        ConfigUtils.Config.Http.DownloadThread = value;
        ConfigUtils.Save();
    }

    /// <summary>
    /// 设置代理
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="user"></param>
    /// <param name="password"></param>
    public static void SetDownloadProxy(string ip, ushort port, string user, string password)
    {
        if (BaseBinding.IsDownload)
        {
            return;
        }

        ConfigUtils.Config.Http ??= ConfigUtils.MakeHttpConfig();
        var con = ConfigUtils.Config.Http;
        con.ProxyIP = ip;
        con.ProxyPort = port;
        con.ProxyUser = user;
        con.ProxyPassword = password;
        ConfigUtils.Save();

        CoreHttpClient.Init();
    }

    /// <summary>
    /// 设置代理开关
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    public static void SetDownloadProxyEnable(bool v1, bool v2, bool v3)
    {
        if (BaseBinding.IsDownload)
        {
            return;
        }

        ConfigUtils.Config.Http ??= ConfigUtils.MakeHttpConfig();
        var con = ConfigUtils.Config.Http;
        con.LoginProxy = v1;
        con.DownloadProxy = v2;
        con.GameProxy = v3;
        ConfigUtils.Save();

        CoreHttpClient.Init();
    }

    /// <summary>
    /// 设置下载验证
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    public static void SetDownloadCheck(bool v1, bool v2)
    {
        if (BaseBinding.IsDownload)
        {
            return;
        }

        ConfigUtils.Config.Http ??= ConfigUtils.MakeHttpConfig();
        var con = ConfigUtils.Config.Http;
        con.CheckFile = v1;
        con.AutoDownload = v2;
        ConfigUtils.Save();
    }

    /// <summary>
    /// 设置自动更新检查
    /// </summary>
    /// <param name="v1"></param>
    public static void SetUpdateCheck(bool v1)
    {
        GuiConfigUtils.Config.CheckUpdate = v1;
        GuiConfigUtils.Save();
    }

    /// <summary>
    /// 设置GC
    /// </summary>
    /// <param name="gc"></param>
    /// <param name="arg"></param>
    public static void SetGc(GCType gc, string? arg)
    {
        ConfigUtils.Config.DefaultJvmArg ??= ConfigUtils.MakeJvmArgConfig();
        var jvm = ConfigUtils.Config.DefaultJvmArg;
        jvm.GC = gc;
        jvm.GCArgument = arg;
        ConfigUtils.Save();
    }

    /// <summary>
    /// 设置运行指令
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="v4"></param>
    public static void SetRunCommand(bool v1, bool v2, string? v3, string? v4, bool v5)
    {
        ConfigUtils.Config.DefaultJvmArg ??= ConfigUtils.MakeJvmArgConfig();
        var jvm = ConfigUtils.Config.DefaultJvmArg;
        jvm.LaunchPre = v1;
        jvm.LaunchPost = v2;
        jvm.LaunchPreData = v3;
        jvm.LaunchPostData = v4;
        jvm.PreRunSame = v5;
        ConfigUtils.Save();
    }

    /// <summary>
    /// 设置运行参数
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="v4"></param>
    public static void SetRunArg(string? v1, string? v2, string? v3, string? v4, bool colorasm)
    {
        ConfigUtils.Config.DefaultJvmArg ??= ConfigUtils.MakeJvmArgConfig();
        var jvm = ConfigUtils.Config.DefaultJvmArg;
        jvm.JavaAgent = v1;
        jvm.JvmArgs = v2;
        jvm.GameArgs = v3;
        jvm.JvmEnv = v4;
        jvm.ColorASM = colorasm;
        ConfigUtils.Save();
    }

    /// <summary>
    /// 设置游戏窗口参数
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    public static void SetGameWindow(bool v1, uint? v2, uint? v3)
    {
        ConfigUtils.Config.Window ??= ConfigUtils.MakeWindowSettingConfig();
        var con = ConfigUtils.Config.Window;
        con.FullScreen = v1;
        con.Width = v2;
        con.Height = v3;
        ConfigUtils.Save();
    }

    /// <summary>
    /// 设置内存大小
    /// </summary>
    /// <param name="minMemory"></param>
    /// <param name="maxMemory"></param>
    public static void SetMemory(uint? minMemory, uint? maxMemory)
    {
        ConfigUtils.Config.DefaultJvmArg ??= ConfigUtils.MakeJvmArgConfig();
        var jvm = ConfigUtils.Config.DefaultJvmArg;
        jvm.MinMemory = minMemory;
        jvm.MaxMemory = maxMemory;
        ConfigUtils.Save();
    }

    /// <summary>
    /// 设置窗口设置
    /// </summary>
    /// <param name="obj"></param>
    public static void SetWindowSettingConfig(WindowSettingObj obj)
    {
        ConfigUtils.Config.Window = obj;
        ConfigUtils.Save();

        CoreHttpClient.Init();
    }

    /// <summary>
    /// 设置游戏检查选项
    /// </summary>
    /// <param name="obj"></param>
    public static void SetGameCheckConfig(bool v1, bool v2, bool v3, bool v4,
        bool v5, bool v6, bool v7, bool v8)
    {
        ConfigUtils.Config.GameCheck ??= ConfigUtils.MakeGameCheckConfig();
        var con = ConfigUtils.Config.GameCheck;
        con.CheckCore = v1;
        con.CheckAssets = v2;
        con.CheckLib = v3;
        con.CheckMod = v4;
        con.CheckCoreSha1 = v5;
        con.CheckAssetsSha1 = v6;
        con.CheckLibSha1 = v7;
        con.CheckModSha1 = v8;
        ConfigUtils.Save();
    }

    /// <summary>
    /// 设置字体设置
    /// </summary>
    /// <param name="name"></param>
    /// <param name="def"></param>
    public static void SetFont(string? name, bool def)
    {
        GuiConfigUtils.Config.FontName = name;
        GuiConfigUtils.Config.FontDefault = def;

        GuiConfigUtils.Save();

        ThemeManager.Init();
    }

    /// <summary>
    /// 重置配置文件
    /// </summary>
    public static void ResetConfig()
    {
        GuiConfigUtils.Config = GuiConfigUtils.MakeDefaultConfig();

        GuiConfigUtils.Save();
    }

    /// <summary>
    /// 设置启动后关闭启动器
    /// </summary>
    /// <param name="value"></param>
    public static void SetLaunchCloseConfig(bool value)
    {
        GuiConfigUtils.Config.CloseBeforeLaunch = value;

        GuiConfigUtils.Save();
    }

    /// <summary>
    /// 设置窗口模式
    /// </summary>
    /// <param name="value"></param>
    public static void SetWindowMode(bool value)
    {
#if !Phone
        GuiConfigUtils.Config.WindowMode = value;

        GuiConfigUtils.Save();

        ColorMCGui.Reboot();
#endif
    }

    /// <summary>
    /// 获取窗口模式
    /// </summary>
    /// <returns>true为单窗口false为多窗口</returns>
    public static bool WindowMode()
    {
#if Phone
        return true;
#endif
        return GuiConfigUtils.Config.WindowMode;
    }

    /// <summary>
    /// 设置语言
    /// </summary>
    /// <param name="type"></param>
    public static void SetLanguage(LanguageType type)
    {
        ConfigUtils.Config.Language = type;
        ConfigUtils.Save();

        LanguageHelper.Change(type);
    }

    /// <summary>
    /// 设置主题色
    /// </summary>
    /// <param name="type"></param>
    public static void SetColorType(ColorType type)
    {
        GuiConfigUtils.Config.ColorType = type;
        GuiConfigUtils.Save();

        ThemeManager.Init();
        _ = ImageManager.LoadBGImage();
    }

    /// <summary>
    /// 设置上次启动的游戏实例
    /// </summary>
    /// <param name="uuid"></param>
    public static void SetLastLaunch(string uuid)
    {
        GuiConfigUtils.Config.LastLaunch = uuid;
        GuiConfigUtils.Save();
    }

    /// <summary>
    /// 获取上次运行的游戏实例
    /// </summary>
    /// <returns></returns>
    public static string? GetLastLaunch()
    {
        return GuiConfigUtils.Config.LastLaunch;
    }

    /// <summary>
    /// 设置主界面服务器Motd
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="v4"></param>
    /// <param name="v5"></param>
    /// <param name="v6"></param>
    public static void SetMotd(string v1, ushort v2, bool v3, bool v4, string v5, string v6)
    {
        GuiConfigUtils.Config.ServerCustom ??= GuiConfigUtils.MakeServerCustomConfig();
        GuiConfigUtils.Config.ServerCustom.IP = v1;
        GuiConfigUtils.Config.ServerCustom.Port = v2;
        GuiConfigUtils.Config.ServerCustom.Motd = v3;
        GuiConfigUtils.Config.ServerCustom.JoinServer = v4;
        GuiConfigUtils.Config.ServerCustom.MotdColor = v5;
        GuiConfigUtils.Config.ServerCustom.MotdBackColor = v6;

        GuiConfigUtils.Save();

        WindowManager.MainWindow?.MotdLoad();

        ThemeManager.Init();
    }

    /// <summary>
    /// 设置锁定游戏实例
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    public static void SetLockGame(bool v1, string? v2)
    {
        GuiConfigUtils.Config.ServerCustom ??= GuiConfigUtils.MakeServerCustomConfig();
        GuiConfigUtils.Config.ServerCustom.LockGame = v1;
        GuiConfigUtils.Config.ServerCustom.GameName = v2;
        GuiConfigUtils.Save();

        WindowManager.MainWindow?.LoadGameItem();
    }

    /// <summary>
    /// 设置自定义UI
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="value"></param>
    public static void SetUI(bool enable, bool customIcon, bool customStart, string? startText, DisplayType display)
    {
        GuiConfigUtils.Config.ServerCustom ??= GuiConfigUtils.MakeServerCustomConfig();
        GuiConfigUtils.Config.ServerCustom.EnableUI = enable;
        GuiConfigUtils.Config.ServerCustom.CustomIcon = customIcon;
        GuiConfigUtils.Config.ServerCustom.CustomStart = customStart;
        GuiConfigUtils.Config.ServerCustom.DisplayType = display;
        GuiConfigUtils.Config.ServerCustom.StartText = startText;

        GuiConfigUtils.Save();

        WindowManager.ReloadIcon();
    }

    /// <summary>
    /// 设置背景音乐
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="v4"></param>
    /// <param name="v5"></param>
    public static void SetMusic(bool v1, bool v2, string? v3, int v4, bool v5, bool v6)
    {
        GuiConfigUtils.Config.ServerCustom ??= GuiConfigUtils.MakeServerCustomConfig();
        GuiConfigUtils.Config.ServerCustom.PlayMusic = v1;
        GuiConfigUtils.Config.ServerCustom.SlowVolume = v2;
        GuiConfigUtils.Config.ServerCustom.Music = v3;
        GuiConfigUtils.Config.ServerCustom.Volume = v4;
        GuiConfigUtils.Config.ServerCustom.RunPause = v5;
        GuiConfigUtils.Config.ServerCustom.MusicLoop = v6;
        GuiConfigUtils.Save();

        Media.Loop = v6;
    }

    /// <summary>
    /// 设置登录锁定
    /// </summary>
    /// <param name="enableOneLogin"></param>
    /// <param name="login"></param>
    /// <param name="url"></param>
    public static void SetLoginLock(bool enableOneLogin, List<LockLoginSetting> list)
    {
        GuiConfigUtils.Config.ServerCustom ??= GuiConfigUtils.MakeServerCustomConfig();
        GuiConfigUtils.Config.ServerCustom.LockLogin = enableOneLogin;
        GuiConfigUtils.Config.ServerCustom.LockLogins = list;
        GuiConfigUtils.Save();

        UserBinding.OnUserEdit();
        WindowManager.UserWindow?.LoadUsers();
    }

    /// <summary>
    /// 删除Live2D模型
    /// </summary>
    public static void DeleteLive2D()
    {
        GuiConfigUtils.Config.Live2D ??= GuiConfigUtils.MakeLive2DConfig();
        GuiConfigUtils.Config.Live2D.Model = null;
        GuiConfigUtils.Save();

        WindowManager.MainWindow?.DeleteModel();
    }

    /// <summary>
    /// 设置启用Live2D模型
    /// </summary>
    /// <param name="enable"></param>
    public static void SetLive2D(bool enable)
    {
        GuiConfigUtils.Config.Live2D ??= GuiConfigUtils.MakeLive2DConfig();
        GuiConfigUtils.Config.Live2D.Enable = enable;
        GuiConfigUtils.Save();

        WindowManager.MainWindow?.ChangeModel();
    }

    /// <summary>
    /// 设置Live2D模型
    /// </summary>
    /// <param name="live2DModel"></param>
    public static void SetLive2D(string? live2DModel)
    {
        GuiConfigUtils.Config.Live2D ??= GuiConfigUtils.MakeLive2DConfig();
        GuiConfigUtils.Config.Live2D.Model = live2DModel;
        GuiConfigUtils.Save();

        WindowManager.MainWindow?.ChangeModel();
    }

    /// <summary>
    /// 设置Live2D界面大小
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="pos"></param>
    public static void SetLive2DSize(int width, int height, int pos)
    {
        GuiConfigUtils.Config.Live2D ??= GuiConfigUtils.MakeLive2DConfig();
        GuiConfigUtils.Config.Live2D.Width = width;
        GuiConfigUtils.Config.Live2D.Height = height;
        GuiConfigUtils.Config.Live2D.Pos = pos;
        GuiConfigUtils.Save();

        WindowManager.MainWindow?.ChangeLive2DSize();
    }

    /// <summary>
    /// 设置启用Live2D低帧率
    /// </summary>
    /// <param name="value"></param>
    public static void SetLive2DMode(bool value)
    {
        GuiConfigUtils.Config.Live2D ??= GuiConfigUtils.MakeLive2DConfig();
        GuiConfigUtils.Config.Live2D.LowFps = value;
        GuiConfigUtils.Save();

        WindowManager.MainWindow?.ChangeLive2DMode();
    }

    /// <summary>
    /// 设置动画样式
    /// </summary>
    /// <param name="value"></param>
    public static void SetStyle(int value, bool value1, bool value2)
    {
        GuiConfigUtils.Config.Style ??= GuiConfigUtils.MakeStyleSettingConfig();
        GuiConfigUtils.Config.Style.EnableAm = value2;
        GuiConfigUtils.Config.Style.AmTime = value;
        GuiConfigUtils.Config.Style.AmFade = value1;
        GuiConfigUtils.Save();

        ThemeManager.LoadPageSlide();
    }

    /// <summary>
    /// 设置服务器密钥
    /// </summary>
    /// <param name="value"></param>
    public static void SetServerKey(string value)
    {
        GuiConfigUtils.Config.ServerKey = value;
        GuiConfigUtils.Save();
    }

    /// <summary>
    /// 启用安全log4j
    /// </summary>
    /// <param name="value"></param>
    public static void SetSafeLog4j(bool value)
    {
        ConfigUtils.Config.SafeLog4j = value;
        ConfigUtils.Save();
    }

    /// <summary>
    /// 设置Frp密钥
    /// </summary>
    /// <param name="key"></param>
    public static void SetFrpKeySakura(string key)
    {
        FrpConfigUtils.Config.SakuraFrp ??= new();
        FrpConfigUtils.Config.SakuraFrp.Key = key;
        FrpConfigUtils.Save();
    }

    /// <summary>
    /// 设置Frp密钥
    /// </summary>
    /// <param name="key"></param>
    public static void SetFrpKeyOpenFrp(string key)
    {
        FrpConfigUtils.Config.OpenFrp ??= new();
        FrpConfigUtils.Config.OpenFrp.Key = key;
        FrpConfigUtils.Save();
    }

    /// <summary>
    /// 新建输入配置
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static InputControlObj NewInput(string name)
    {
        var obj = JoystickConfig.MakeInputControl();
        obj.Name = name;
        JoystickConfig.PutConfig(obj);
        JoystickConfig.Save(obj);

        return obj;
    }

    /// <summary>
    /// 设置手柄配置
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="item"></param>
    public static void SaveInput(InputControlObj obj, bool item)
    {
        obj.ItemCycle = item;
        JoystickConfig.Save(obj);
    }

    /// <summary>
    /// 设置启用手柄支持
    /// </summary>
    /// <param name="item"></param>
    public static void SaveInputInfo(bool item)
    {
        GuiConfigUtils.Config.Input ??= new();
        GuiConfigUtils.Config.Input.Enable = item;
        GuiConfigUtils.Save();
    }

    /// <summary>
    /// 添加手柄按钮配置
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="key"></param>
    /// <param name="obj1"></param>
    public static void AddInput(InputControlObj obj, byte key, InputKeyObj obj1)
    {
        if (!obj.Keys.TryAdd(key, obj1))
        {
            obj.Keys[key] = obj1;
        }
        JoystickConfig.Save(obj);
    }

    /// <summary>
    /// 删除手柄按键配置
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="key"></param>
    public static void DeleteInput(InputControlObj obj, byte key)
    {
        obj.Keys.Remove(key);
        JoystickConfig.Save(obj);
    }

    /// <summary>
    /// 添加遥感配置
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="uuid"></param>
    /// <param name="obj1"></param>
    public static void AddAxisInput(InputControlObj obj, string uuid, InputAxisObj obj1)
    {
        if (!obj.AxisKeys.TryAdd(uuid, obj1))
        {
            obj.AxisKeys[uuid] = obj1;
        }
        JoystickConfig.Save(obj);
    }

    /// <summary>
    /// 删除摇杆设置
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="key"></param>
    public static void DeleteAxisInput(InputControlObj obj, string key)
    {
        if (obj.AxisKeys.Remove(key))
        {
            JoystickConfig.Save(obj);
        }
    }

    /// <summary>
    /// 设置物品栏循环按钮
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public static void SetItemCycle(InputControlObj obj, byte left, byte right)
    {
        obj.ItemCycleLeft = left;
        obj.ItemCycleRight = right;
        JoystickConfig.Save(obj);
    }

    /// <summary>
    /// 设置移动倍率
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    /// <param name="value1"></param>
    public static void SetInputRate(InputControlObj obj, float value, float value1, float value2)
    {
        obj.RotateRate = value;
        obj.CursorRate = value1;
        obj.DownRate = value2;
        JoystickConfig.Save(obj);
    }

    /// <summary>
    /// 设置输入摇杆
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="inputRotateAxis"></param>
    /// <param name="inputCursorAxis"></param>
    public static void SetInputAxis(InputControlObj obj, int inputRotateAxis, int inputCursorAxis)
    {
        obj.RotateAxis = inputRotateAxis;
        obj.CursorAxis = inputCursorAxis;
        JoystickConfig.Save(obj);
    }

    /// <summary>
    /// 设置死区大小
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="inputRotate"></param>
    /// <param name="inputCursor"></param>
    public static void SetInputDeath(InputControlObj obj, int inputRotate, int inputCursor)
    {
        obj.RotateDeath = inputRotate;
        obj.CursorDeath = inputCursor;
        JoystickConfig.Save(obj);
    }

    /// <summary>
    /// 设置目前手柄的配置文件
    /// </summary>
    /// <param name="uuid"></param>
    public static void SaveNowInputConfig(string? uuid)
    {
        GuiConfigUtils.Config.Input ??= new();
        GuiConfigUtils.Config.Input.NowConfig = uuid;
        GuiConfigUtils.Save();
    }

    /// <summary>
    /// 保存手柄配置
    /// </summary>
    /// <param name="obj"></param>
    public static void SaveInputConfig(InputControlObj obj)
    {
        JoystickConfig.PutConfig(obj);
        JoystickConfig.Save(obj);
    }

    /// <summary>
    /// 删除手柄配置
    /// </summary>
    /// <param name="obj"></param>
    public static void RemoveInputConfig(InputControlObj obj)
    {
        JoystickConfig.Remove(obj);
    }

    /// <summary>
    /// 设置头像角度
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void SetHeadXY(int x, int y)
    {
        GuiConfigUtils.Config.Head ??= new();
        GuiConfigUtils.Config.Head.X = x;
        GuiConfigUtils.Config.Head.Y = y;
        GuiConfigUtils.Save();

        UserBinding.ReloadSkin();
    }

    /// <summary>
    /// 设置头像角度
    /// </summary>
    public static void SetHeadType(HeadType type)
    {
        GuiConfigUtils.Config.Head ??= new();
        GuiConfigUtils.Config.Head.Type = type;
        GuiConfigUtils.Save();

        UserBinding.ReloadSkin();
    }

    /// <summary>
    /// 设置DNS
    /// </summary>
    /// <param name="dnsEnable"></param>
    /// <param name="dnsType"></param>
    public static void SetDns(bool dnsEnable, DnsType dnsType, bool v2)
    {
        if (BaseBinding.IsDownload)
        {
            return;
        }

        ConfigUtils.Config.Dns ??= ConfigUtils.MakeDnsConfig();
        ConfigUtils.Config.Dns.Enable = dnsEnable;
        ConfigUtils.Config.Dns.DnsType = dnsType;
        ConfigUtils.Config.Dns.HttpProxy = v2;
        ConfigUtils.Save();

        CoreHttpClient.Init();
    }

    /// <summary>
    /// 设置DNS
    /// </summary>
    public static void AddDns(string url, DnsType dnsOver)
    {
        if (BaseBinding.IsDownload)
        {
            return;
        }
        ConfigUtils.Config.Dns ??= ConfigUtils.MakeDnsConfig();
        if (dnsOver == DnsType.DnsOver)
        {
            ConfigUtils.Config.Dns.Dns.Add(url);
        }
        else
        {
            ConfigUtils.Config.Dns.Https.Add(url);
        }
        ConfigUtils.Save();

        CoreHttpClient.Init();
    }

    /// <summary>
    /// 删除Dns
    /// </summary>
    /// <param name="url"></param>
    /// <param name="dns"></param>
    public static void RemoveDns(string url, DnsType dns)
    {
        if (BaseBinding.IsDownload)
        {
            return;
        }
        ConfigUtils.Config.Dns ??= ConfigUtils.MakeDnsConfig();
        if (dns == DnsType.DnsOver)
        {
            ConfigUtils.Config.Dns.Dns.Remove(url);
        }
        else
        {
            ConfigUtils.Config.Dns.Https.Remove(url);
        }
        ConfigUtils.Save();

        CoreHttpClient.Init();
    }

    /// <summary>
    /// 设置日志颜色
    /// </summary>
    /// <param name="warnColor"></param>
    /// <param name="errorColor"></param>
    /// <param name="debugColor"></param>
    public static void SetLogColor(string warnColor, string errorColor, string debugColor)
    {
        GuiConfigUtils.Config.LogColor ??= GuiConfigUtils.MakeLogColorConfig();
        GuiConfigUtils.Config.LogColor.Warn = warnColor;
        GuiConfigUtils.Config.LogColor.Error = errorColor;
        GuiConfigUtils.Config.LogColor.Debug = debugColor;

        GuiConfigUtils.Save();

        ThemeManager.Init();
    }

    /// <summary>
    /// 设置管理员启动
    /// </summary>
    /// <param name="adminLaunch"></param>
    /// <param name="gameAdminLaunch"></param>
    public static void SetAdmin(bool adminLaunch, bool gameAdminLaunch)
    {
        GuiConfigUtils.Config.ServerCustom ??= new();
        GuiConfigUtils.Config.ServerCustom.AdminLaunch = adminLaunch;
        GuiConfigUtils.Config.ServerCustom.GameAdminLaunch = gameAdminLaunch;

        GuiConfigUtils.Save();
    }

    /// <summary>
    /// 设置卡片
    /// </summary>
    /// <param name="cardNews"></param>
    /// <param name="cardLast"></param>
    /// <param name="cardOnline"></param>
    public static void SetCard(bool cardNews, bool cardLast, bool cardOnline)
    {
        GuiConfigUtils.Config.Card ??= new();
        GuiConfigUtils.Config.Card.News = cardNews;
        GuiConfigUtils.Config.Card.Last = cardLast;
        GuiConfigUtils.Config.Card.Online = cardOnline;

        GuiConfigUtils.Save();
        WindowManager.MainWindow?.LoadDone();
    }

    /// <summary>
    /// 设置启动器检测
    /// </summary>
    /// <param name="user"></param>
    /// <param name="loader"></param>
    /// <param name="memory"></param>
    public static void SetCheck(bool user, bool loader, bool memory)
    {
        GuiConfigUtils.Config.LaunchCheck ??= new();
        GuiConfigUtils.Config.LaunchCheck.CheckUser = user;
        GuiConfigUtils.Config.LaunchCheck.CheckLoader = loader;
        GuiConfigUtils.Config.LaunchCheck.CheckMemory = memory;

        GuiConfigUtils.Save();
    }

    /// <summary>
    /// 设置主页面模式
    /// </summary>
    /// <param name="value"></param>
    public static void SetWindowSimple(bool value)
    {
        GuiConfigUtils.Config.Simple = value;

        GuiConfigUtils.Save();
        WindowManager.MainWindow?.LoadDone();
    }
}
