using ColorMC.Core.Config;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Setting;

/// <summary>
/// 设置页面
/// </summary>
public partial class SettingModel
{
    /// <summary>
    /// GC类型
    /// </summary>
    public string[] GCTypeList { get; init; } = LanguageBinding.GetGCTypes();

    /// <summary>
    /// 启动前命令
    /// </summary>
    [ObservableProperty]
    private string? _preCmd;
    /// <summary>
    /// 启动后命令
    /// </summary>
    [ObservableProperty]
    private string? _postCmd;
    /// <summary>
    /// GC参数
    /// </summary>
    [ObservableProperty]
    private string? _gCArg;
    /// <summary>
    /// JavaAgent参数
    /// </summary>
    [ObservableProperty]
    private string? _javaAgent;
    /// <summary>
    /// Java参数
    /// </summary>
    [ObservableProperty]
    private string? _jvmArg;
    /// <summary>
    /// 游戏参数
    /// </summary>
    [ObservableProperty]
    private string? _gameArg;
    /// <summary>
    /// Java环境变量
    /// </summary>
    [ObservableProperty]
    private string? _jvmEnv;

    /// <summary>
    /// 是否检测游戏文件
    /// </summary>
    [ObservableProperty]
    private bool _checkCore;
    /// <summary>
    /// 是否检测资源文件
    /// </summary>
    [ObservableProperty]
    private bool _checkAssets;
    /// <summary>
    /// 是否检测运行库
    /// </summary>
    [ObservableProperty]
    private bool _checkLib;
    /// <summary>
    /// 是否检测模组
    /// </summary>
    [ObservableProperty]
    private bool _checkMod;
    /// <summary>
    /// 是否检测游戏文件完整性
    /// </summary>
    [ObservableProperty]
    private bool _checkCoreSha1;
    /// <summary>
    /// 是否检测资源文件完整性
    /// </summary>
    [ObservableProperty]
    private bool _checkAssetsSha1;
    /// <summary>
    /// 是否检测运行库完整性
    /// </summary>
    [ObservableProperty]
    private bool _checkLibSha1;
    /// <summary>
    /// 是否检测模组完整性
    /// </summary>
    [ObservableProperty]
    private bool _checkModSha1;
    /// <summary>
    /// 是否启动前运行
    /// </summary>
    [ObservableProperty]
    private bool _preRun;
    /// <summary>
    /// 是否启动后运行
    /// </summary>
    [ObservableProperty]
    private bool _postRun;
    /// <summary>
    /// 是否全屏显示
    /// </summary>
    [ObservableProperty]
    private bool _fullScreen;
    /// <summary>
    /// 是否启动后关闭启动器
    /// </summary>
    [ObservableProperty]
    private bool _closeBefore;
    /// <summary>
    /// 是否启用安全Log4j
    /// </summary>
    [ObservableProperty]
    private bool _safeLog4j;
    /// <summary>
    /// 是否使用自定义GC
    /// </summary>
    [ObservableProperty]
    private bool _customGc;
    /// <summary>
    /// 是否同时启动游戏
    /// </summary>
    [ObservableProperty]
    private bool _preRunSame;
    /// <summary>
    /// 是否检测用户是否占用
    /// </summary>
    [ObservableProperty]
    private bool _checkUser;
    /// <summary>
    /// 是否检测加载器是否启用
    /// </summary>
    [ObservableProperty]
    private bool _checkLoader;
    /// <summary>
    /// 是否检测内存分配
    /// </summary>
    [ObservableProperty]
    private bool _checkMemory;
    /// <summary>
    /// 是否禁用ColorMC ASM
    /// </summary>
    [ObservableProperty]
    private bool _colorASM;

    /// <summary>
    /// GC类型
    /// </summary>
    [ObservableProperty]
    private GCType _gC;

    /// <summary>
    /// 最小内存
    /// </summary>
    [ObservableProperty]
    private uint? _minMemory;
    /// <summary>
    /// 最大内存
    /// </summary>
    [ObservableProperty]
    private uint? _maxMemory;
    /// <summary>
    /// 窗口宽度
    /// </summary>
    [ObservableProperty]
    private uint? _width;
    /// <summary>
    /// 窗口高度
    /// </summary>
    [ObservableProperty]
    private uint? _height;

    /// <summary>
    /// 内存信息
    /// </summary>
    [ObservableProperty]
    private string _memory;

    /// <summary>
    /// 是否在加载配置
    /// </summary>
    private bool _argLoad = true;

    //配置修改
    partial void OnCloseBeforeChanged(bool value)
    {
        ConfigBinding.SetLaunchCloseConfig(value);
    }

    partial void OnColorASMChanged(bool value)
    {
        SetArg();
    }

    partial void OnCheckUserChanged(bool value)
    {
        SetCheck();
    }

    partial void OnCheckLoaderChanged(bool value)
    {
        SetCheck();
    }

    partial void OnCheckMemoryChanged(bool value)
    {
        SetCheck();
    }

    partial void OnSafeLog4jChanged(bool value)
    {
        if (_argLoad)
        {
            return;
        }

        ConfigBinding.SetSafeLog4j(value);
    }

    partial void OnMaxMemoryChanged(uint? value)
    {
        SetMemory();
    }

    partial void OnMinMemoryChanged(uint? value)
    {
        SetMemory();
    }

    partial void OnHeightChanged(uint? value)
    {
        SetWindow();
    }

    partial void OnWidthChanged(uint? value)
    {
        SetWindow();
    }

    partial void OnFullScreenChanged(bool value)
    {
        SetWindow();
    }

    partial void OnJvmEnvChanged(string? value)
    {
        SetArg();
    }

    partial void OnGameArgChanged(string? value)
    {
        SetArg();
    }

    partial void OnJvmArgChanged(string? value)
    {
        SetArg();
    }

    partial void OnJavaAgentChanged(string? value)
    {
        SetArg();
    }

    partial void OnPreRunSameChanged(bool value)
    {
        SetCommand();
    }

    partial void OnPreRunChanged(bool value)
    {
        SetCommand();
    }

    partial void OnPostRunChanged(bool value)
    {
        SetCommand();
    }

    partial void OnPreCmdChanged(string? value)
    {
        SetCommand();
    }

    partial void OnPostCmdChanged(string? value)
    {
        SetCommand();
    }

    partial void OnGCChanged(GCType value)
    {
        CustomGc = value == GCType.User;

        SetGc();
    }

    partial void OnGCArgChanged(string? value)
    {
        SetGc();
    }

    partial void OnCheckCoreSha1Changed(bool value)
    {
        SetGameCheck();
    }

    partial void OnCheckAssetsSha1Changed(bool value)
    {
        SetGameCheck();
    }

    partial void OnCheckLibSha1Changed(bool value)
    {
        SetGameCheck();
    }

    partial void OnCheckModSha1Changed(bool value)
    {
        SetGameCheck();
    }

    partial void OnCheckCoreChanged(bool value)
    {
        SetGameCheck();
    }

    partial void OnCheckAssetsChanged(bool value)
    {
        SetGameCheck();
    }

    partial void OnCheckLibChanged(bool value)
    {
        SetGameCheck();
    }

    partial void OnCheckModChanged(bool value)
    {
        SetGameCheck();
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    public void LoadArg()
    {
        _argLoad = true;
        var config = ConfigUtils.Config;
        if (config is { } con)
        {
            GC = con.DefaultJvmArg.GC ?? GCType.G1GC;

            MinMemory = con.DefaultJvmArg.MinMemory;
            MaxMemory = con.DefaultJvmArg.MaxMemory;
            Width = con.Window.Width;
            Height = con.Window.Height;

            GCArg = con.DefaultJvmArg.GCArgument;
            JavaAgent = con.DefaultJvmArg.JavaAgent;
            JvmArg = con.DefaultJvmArg.JvmArgs;
            GameArg = con.DefaultJvmArg.GameArgs;
            PostCmd = con.DefaultJvmArg.LaunchPostData;
            PreCmd = con.DefaultJvmArg.LaunchPreData;
            JvmEnv = con.DefaultJvmArg.JvmEnv;
            PreRunSame = con.DefaultJvmArg.PreRunSame;
            ColorASM = con.DefaultJvmArg.ColorASM;

            FullScreen = con.Window.FullScreen ?? false;
            CheckCore = con.GameCheck.CheckCore;
            CheckAssets = con.GameCheck.CheckAssets;
            CheckLib = con.GameCheck.CheckLib;
            CheckMod = con.GameCheck.CheckMod;
            CheckCoreSha1 = con.GameCheck.CheckCoreSha1;
            CheckAssetsSha1 = con.GameCheck.CheckAssetsSha1;
            CheckLibSha1 = con.GameCheck.CheckLibSha1;
            CheckModSha1 = con.GameCheck.CheckModSha1;

            PostRun = con.DefaultJvmArg.LaunchPost;
            PreRun = con.DefaultJvmArg.LaunchPre;
            SafeLog4j = con.SafeLog4j;
        }

        var config1 = GuiConfigUtils.Config;
        if (config1 is { } con1)
        {
            CloseBefore = con1.CloseBeforeLaunch;

            CheckUser = con1.LaunchCheck.CheckUser;
            CheckLoader = con1.LaunchCheck.CheckLoader;
            CheckMemory = con1.LaunchCheck.CheckMemory;
        }
        _argLoad = false;
    }

    //保存配置
    private void SetCheck()
    {
        if (_argLoad)
        {
            return;
        }

        ConfigBinding.SetCheck(CheckUser, CheckLoader, CheckMemory);
    }

    private void SetMemory()
    {
        if (_argLoad)
        {
            return;
        }

        ConfigBinding.SetMemory(MinMemory, MaxMemory);
    }

    private void SetWindow()
    {
        if (_argLoad)
        {
            return;
        }

        ConfigBinding.SetGameWindow(FullScreen, Width, Height);
    }

    private void SetGc()
    {
        if (_argLoad)
        {
            return;
        }

        ConfigBinding.SetGc(GC, GCArg);
    }

    private void SetArg()
    {
        if (_argLoad)
        {
            return;
        }

        ConfigBinding.SetRunArg(JavaAgent, JvmArg, GameArg, JvmEnv, ColorASM);
    }

    private void SetCommand()
    {
        if (_argLoad)
        {
            return;
        }

        ConfigBinding.SetRunCommand(PreRun, PostRun, PreCmd, PostCmd, PreRunSame);
    }

    private void SetGameCheck()
    {
        if (_argLoad)
        {
            return;
        }

        ConfigBinding.SetGameCheckConfig(CheckCore, CheckAssets, CheckLib, CheckMod,
            CheckCoreSha1, CheckAssetsSha1, CheckLibSha1, CheckModSha1);
    }
}
