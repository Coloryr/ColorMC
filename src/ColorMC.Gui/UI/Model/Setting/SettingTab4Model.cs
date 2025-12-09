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
    public string[] GCTypeList { get; init; } = LangUtils.GetGCTypes();

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
    /// 是否同时启动游戏
    /// </summary>
    [ObservableProperty]
    private bool _preRunSame;
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

    partial void OnColorASMChanged(bool value)
    {
        SetArg();
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
        var config = ConfigLoad.Config;
        if (config is { } con)
        {
            GC = con.DefaultJvmArg.GC ?? GCType.Auto;

            MinMemory = con.DefaultJvmArg.MinMemory;
            MaxMemory = con.DefaultJvmArg.MaxMemory;
            Width = con.Window.Width;
            Height = con.Window.Height;

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

        _argLoad = false;
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

        ConfigBinding.SetGc(GC);
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
