using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab4Model : BaseModel
{
    public List<string> GCTypeList => JavaBinding.GetGCTypes();

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
    private int _gC;

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

    partial void OnGCChanged(int value)
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
        if (config.Item1 != null)
        {
            GC = (int)(config.Item1.DefaultJvmArg.GC ?? 0);

            MinMemory = config.Item1.DefaultJvmArg.MinMemory ?? 512;
            MaxMemory = config.Item1.DefaultJvmArg.MaxMemory ?? 4096;
            Width = config.Item1.Window.Width ?? 1280;
            Height = config.Item1.Window.Height ?? 720;

            GCArg = config.Item1.DefaultJvmArg.GCArgument;
            JavaAgent = config.Item1.DefaultJvmArg.JavaAgent;
            JvmArg = config.Item1.DefaultJvmArg.JvmArgs;
            GameArg = config.Item1.DefaultJvmArg.GameArgs;
            PostCmd = config.Item1.DefaultJvmArg.LaunchPostData;
            PreCmd = config.Item1.DefaultJvmArg.LaunchPreData;

            FullScreen = config.Item1.Window.FullScreen ?? false;
            CheckCore = config.Item1.GameCheck.CheckCore;
            CheckAssets = config.Item1.GameCheck.CheckAssets;
            CheckLib = config.Item1.GameCheck.CheckLib;
            CheckMod = config.Item1.GameCheck.CheckMod;
            CheckCoreSha1 = config.Item1.GameCheck.CheckCoreSha1;
            CheckAssetsSha1 = config.Item1.GameCheck.CheckAssetsSha1;
            CheckLibSha1 = config.Item1.GameCheck.CheckLibSha1;
            CheckModSha1 = config.Item1.GameCheck.CheckModSha1;
            PostRun = config.Item1.DefaultJvmArg.LaunchPost;
            PreRun = config.Item1.DefaultJvmArg.LaunchPre;
        }

        if (config.Item2 != null)
        {
            Close = config.Item2.CloseBeforeLaunch;
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
