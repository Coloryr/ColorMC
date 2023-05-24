using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab4Model : ObservableObject
{
    private readonly IUserControl Con;

    public List<string> GCTypeList => JavaBinding.GetGCTypes();

    [ObservableProperty]
    private string? preCmd;
    [ObservableProperty]
    private string? postCmd;
    [ObservableProperty]
    private string? gCArg;
    [ObservableProperty]
    private string? javaAgent;
    [ObservableProperty]
    private string? jvmArg;
    [ObservableProperty]
    private string? gameArg;

    [ObservableProperty]
    private bool checkCore;
    [ObservableProperty]
    private bool checkAssets;
    [ObservableProperty]
    private bool checkLib;
    [ObservableProperty]
    private bool checkMod;
    [ObservableProperty]
    private bool preRun;
    [ObservableProperty]
    private bool postRun;
    [ObservableProperty]
    private bool fullScreen;
    [ObservableProperty]
    private bool close;

    [ObservableProperty]
    private int gC;

    [ObservableProperty]
    private uint minMemory;
    [ObservableProperty]
    private uint maxMemory;
    [ObservableProperty]
    private uint width;
    [ObservableProperty]
    private uint height;

    private bool load = false;

    public SettingTab4Model(IUserControl con)
    {
        Con = con;
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
        load = true;
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
            PostRun = config.Item1.DefaultJvmArg.LaunchPost;
            PreRun = config.Item1.DefaultJvmArg.LaunchPre;
        }

        if (config.Item2 != null)
        {
            Close = config.Item2.CloseBeforeLaunch;
        }
        load = false;
    }

    private void SetMemory()
    {
        if (load)
            return;

        ConfigBinding.SetMemory(MinMemory, MaxMemory);
    }

    private void SetWindow()
    {
        if (load)
            return;

        ConfigBinding.SetGameWindow(FullScreen, Width, Height);
    }

    private void SetGc()
    {
        if (load)
            return;

        ConfigBinding.SetGc((GCType)GC, GCArg);
    }

    private void SetArg()
    {
        if (load)
            return;

        ConfigBinding.SetRunArg(JavaAgent, JvmArg, GameArg);
    }

    private void SetCommand()
    {
        if (load)
            return;

        ConfigBinding.SetRunCommand(PreRun, PostRun, PreCmd, PostCmd);
    }

    private void SetCheck()
    {
        if (load)
            return;

        ConfigBinding.SetGameCheckConfig(CheckCore, CheckAssets, CheckLib, CheckMod);
    }
}
