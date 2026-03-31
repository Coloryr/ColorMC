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
    public partial string? PreCmd { get; set; }

    /// <summary>
    /// 启动后命令
    /// </summary>
    [ObservableProperty]
    public partial string? PostCmd { get; set; }

    /// <summary>
    /// Java参数
    /// </summary>
    [ObservableProperty]
    public partial string? JvmArg { get; set; }

    /// <summary>
    /// 游戏参数
    /// </summary>
    [ObservableProperty]
    public partial string? GameArg { get; set; }

    /// <summary>
    /// Java环境变量
    /// </summary>
    [ObservableProperty]
    public partial string? JvmEnv { get; set; }

    /// <summary>
    /// 是否检测游戏文件
    /// </summary>
    [ObservableProperty]
    public partial bool CheckCore { get; set; }

    /// <summary>
    /// 是否检测资源文件
    /// </summary>
    [ObservableProperty]
    public partial bool CheckAssets { get; set; }

    /// <summary>
    /// 是否检测运行库
    /// </summary>
    [ObservableProperty]
    public partial bool CheckLib { get; set; }

    /// <summary>
    /// 是否检测模组
    /// </summary>
    [ObservableProperty]
    public partial bool CheckMod { get; set; }

    /// <summary>
    /// 是否检测游戏文件完整性
    /// </summary>
    [ObservableProperty]
    public partial bool CheckCoreSha1 { get; set; }

    /// <summary>
    /// 是否检测资源文件完整性
    /// </summary>
    [ObservableProperty]
    public partial bool CheckAssetsSha1 { get; set; }

    /// <summary>
    /// 是否检测运行库完整性
    /// </summary>
    [ObservableProperty]
    public partial bool CheckLibSha1 { get; set; }

    /// <summary>
    /// 是否检测模组完整性
    /// </summary>
    [ObservableProperty]
    public partial bool CheckModSha1 { get; set; }

    /// <summary>
    /// 是否启动前运行
    /// </summary>
    [ObservableProperty]
    public partial bool PreRun { get; set; }

    /// <summary>
    /// 是否启动后运行
    /// </summary>
    [ObservableProperty]
    public partial bool PostRun { get; set; }

    /// <summary>
    /// 是否全屏显示
    /// </summary>
    [ObservableProperty]
    public partial bool FullScreen { get; set; }

    /// <summary>
    /// 是否同时启动游戏
    /// </summary>
    [ObservableProperty]
    public partial bool PreRunSame { get; set; }

    /// <summary>
    /// 是否禁用ColorMC ASM
    /// </summary>
    [ObservableProperty]
    public partial bool ColorASM { get; set; }

    /// <summary>
    /// GC类型
    /// </summary>
    [ObservableProperty]
    public partial GCType GC { get; set; }

    /// <summary>
    /// 最小内存
    /// </summary>
    [ObservableProperty]
    public partial uint? MinMemory { get; set; }

    /// <summary>
    /// 最大内存
    /// </summary>
    [ObservableProperty]
    public partial uint? MaxMemory { get; set; }

    /// <summary>
    /// 窗口宽度
    /// </summary>
    [ObservableProperty]
    public partial uint? Width { get; set; }

    /// <summary>
    /// 窗口高度
    /// </summary>
    [ObservableProperty]
    public partial uint? Height { get; set; }

    /// <summary>
    /// 内存信息
    /// </summary>
    [ObservableProperty]
    public partial string Memory { get; set; }

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

        ConfigBinding.SetRunArg(JvmArg, GameArg, JvmEnv, ColorASM);
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
