﻿using ColorMC.Core.Config;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    public string[] GCTypeList { get; init; } = LanguageBinding.GetGCTypes();

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
    private string? _jvmEnv;

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
    private bool _closeBefore;
    [ObservableProperty]
    private bool _safeLog4j;
    [ObservableProperty]
    private bool _customGc;

    [ObservableProperty]
    private GCType _gC;

    [ObservableProperty]
    private uint? _minMemory;
    [ObservableProperty]
    private uint? _maxMemory;
    [ObservableProperty]
    private uint? _width;
    [ObservableProperty]
    private uint? _height;

    private bool _argLoad = true;

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
        }
        _argLoad = false;
    }

    private void SetMemory()
    {
        if (_argLoad)
            return;

        ConfigBinding.SetMemory(MinMemory, MaxMemory);
    }

    private void SetWindow()
    {
        if (_argLoad)
            return;

        ConfigBinding.SetGameWindow(FullScreen, Width, Height);
    }

    private void SetGc()
    {
        if (_argLoad)
            return;

        ConfigBinding.SetGc(GC, GCArg);
    }

    private void SetArg()
    {
        if (_argLoad)
            return;

        ConfigBinding.SetRunArg(JavaAgent, JvmArg, GameArg, JvmEnv);
    }

    private void SetCommand()
    {
        if (_argLoad)
            return;

        ConfigBinding.SetRunCommand(PreRun, PostRun, PreCmd, PostCmd);
    }

    private void SetGameCheck()
    {
        if (_argLoad)
            return;

        ConfigBinding.SetGameCheckConfig(CheckCore, CheckAssets, CheckLib, CheckMod,
            CheckCoreSha1, CheckAssetsSha1, CheckLibSha1, CheckModSha1);
    }
}
