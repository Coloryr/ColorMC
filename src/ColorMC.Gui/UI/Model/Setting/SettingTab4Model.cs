using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab4Model : BaseModel
{
    public List<string> GCTypeList { get; init; } = JavaBinding.GetGCTypes();

    [ObservableProperty]
    private string? _preCmd;
    [ObservableProperty]
    private string? _postCmd;
    [ObservableProperty]
    private string? _gCArg;
    [ObservableProperty]
    private string? _javaAgent;
    [ObservableProperty]
    private string? _jvmArg;
    [ObservableProperty]
    private string? _gameArg;

    [ObservableProperty]
    private bool _checkCore;
    [ObservableProperty]
    private bool _checkAssets;
    [ObservableProperty]
    private bool _checkLib;
    [ObservableProperty]
    private bool _checkMod;
    [ObservableProperty]
    private bool _checkCoreSha1;
    [ObservableProperty]
    private bool _checkAssetsSha1;
    [ObservableProperty]
    private bool _checkLibSha1;
    [ObservableProperty]
    private bool _checkModSha1;
    [ObservableProperty]
    private bool _preRun;
    [ObservableProperty]
    private bool _postRun;
    [ObservableProperty]
    private bool _fullScreen;
    [ObservableProperty]
    private bool _close;

    [ObservableProperty]
    private GCType _gC;

    [ObservableProperty]
    private uint _minMemory;
    [ObservableProperty]
    private uint _maxMemory;
    [ObservableProperty]
    private uint _width;
    [ObservableProperty]
    private uint _height;

    private bool _load = false;

    public SettingTab4Model(IUserControl con) : base(con)
    {

    }

    partial void OnMaxMemoryChanged(uint value)
    {
        SetMemory();
    }

    partial void OnMinMemoryChanged(uint value)
    {
        SetMemory();
    }

    partial void OnHeightChanged(uint value)
    {
        SetWindow();
    }

    partial void OnWidthChanged(uint value)
    {
        SetWindow();
    }

    partial void OnFullScreenChanged(bool value)
    {
        SetWindow();
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

    partial void OnGCArgChanged(string? value)
    {
        SetGc();
    }

    partial void OnCheckCoreSha1Changed(bool value)
    {
        SetCheck();
    }

    partial void OnCheckAssetsSha1Changed(bool value)
    {
        SetCheck();
    }

    partial void OnCheckLibSha1Changed(bool value)
    {
        SetCheck();
    }

    partial void OnCheckModSha1Changed(bool value)
    {
        SetCheck();
    }

    partial void OnCheckCoreChanged(bool value)
    {
        SetCheck();
    }

    partial void OnCheckAssetsChanged(bool value)
    {
        SetCheck();
    }

    partial void OnCheckLibChanged(bool value)
    {
        SetCheck();
    }

    partial void OnCheckModChanged(bool value)
    {
        SetCheck();
    }

    public void Load()
    {
        _load = true;
        var config = ConfigBinding.GetAllConfig();
        if (config.Item1 is { } con)
        {
            GC = (con.DefaultJvmArg.GC ?? GCType.G1GC);

            MinMemory = con.DefaultJvmArg.MinMemory ?? 512;
            MaxMemory = con.DefaultJvmArg.MaxMemory ?? 4096;
            Width = con.Window.Width ?? 1280;
            Height = con.Window.Height ?? 720;

            GCArg = con.DefaultJvmArg.GCArgument;
            JavaAgent = con.DefaultJvmArg.JavaAgent;
            JvmArg = con.DefaultJvmArg.JvmArgs;
            GameArg = con.DefaultJvmArg.GameArgs;
            PostCmd = con.DefaultJvmArg.LaunchPostData;
            PreCmd = con.DefaultJvmArg.LaunchPreData;

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
        }

        if (config.Item2 is { } con1)
        {
            Close = con1.CloseBeforeLaunch;
        }
        _load = false;
    }

    private void SetMemory()
    {
        if (_load)
            return;

        ConfigBinding.SetMemory(MinMemory, MaxMemory);
    }

    private void SetWindow()
    {
        if (_load)
            return;

        ConfigBinding.SetGameWindow(FullScreen, Width, Height);
    }

    private void SetGc()
    {
        if (_load)
            return;

        ConfigBinding.SetGc((GCType)GC, GCArg);
    }

    private void SetArg()
    {
        if (_load)
            return;

        ConfigBinding.SetRunArg(JavaAgent, JvmArg, GameArg);
    }

    private void SetCommand()
    {
        if (_load)
            return;

        ConfigBinding.SetRunCommand(PreRun, PostRun, PreCmd, PostCmd);
    }

    private void SetCheck()
    {
        if (_load)
            return;

        ConfigBinding.SetGameCheckConfig(CheckCore, CheckAssets, CheckLib, CheckMod,
            CheckCoreSha1, CheckAssetsSha1, CheckLibSha1, CheckModSha1);
    }
}
