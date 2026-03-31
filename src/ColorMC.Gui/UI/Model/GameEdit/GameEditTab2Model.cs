using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel
{
    /// <summary>
    /// GC类型列表
    /// </summary>
    public string[] GCTypeList { get; init; } = LangUtils.GetGCTypes();
    /// <summary>
    /// Java列表
    /// </summary>
    public ObservableCollection<string> JvmList { get; init; } = [];

    /// <summary>
    /// 标题
    /// </summary>
    [ObservableProperty]
    public partial string TitleText { get; set; }

    /// <summary>
    /// Java路径
    /// </summary>
    [ObservableProperty]
    public partial string? JvmLocal { get; set; }

    /// <summary>
    /// 选择的Java
    /// </summary>
    [ObservableProperty]
    public partial string? JvmName { get; set; }

    /// <summary>
    /// 启动前执行
    /// </summary>
    [ObservableProperty]
    public partial string? PerRunCmd { get; set; }

    /// <summary>
    /// 启动后执行
    /// </summary>
    [ObservableProperty]
    public partial string? PostRunCmd { get; set; }

    /// <summary>
    /// Java启动参数
    /// </summary>
    [ObservableProperty]
    public partial string? JvmArg { get; set; }

    /// <summary>
    /// 游戏启动参数
    /// </summary>
    [ObservableProperty]
    public partial string? GameArg { get; set; }

    /// <summary>
    /// 自定义Classpath
    /// </summary>
    [ObservableProperty]
    public partial string? ClassPath { get; set; }

    /// <summary>
    /// 自定义主类
    /// </summary>
    [ObservableProperty]
    public partial string? MainClass { get; set; }

    /// <summary>
    /// 加入服务器地址
    /// </summary>
    [ObservableProperty]
    public partial string? IP { get; set; }

    /// <summary>
    /// 代理地址
    /// </summary>
    [ObservableProperty]
    public partial string? ProxyIP { get; set; }

    /// <summary>
    /// 代理用户
    /// </summary>
    [ObservableProperty]
    public partial string? ProxyUser { get; set; }

    /// <summary>
    /// 代理密码
    /// </summary>
    [ObservableProperty]
    public partial string? ProxyPassword { get; set; }

    /// <summary>
    /// 自定义环境变量
    /// </summary>
    [ObservableProperty]
    public partial string? JvmEnv { get; set; }

    /// <summary>
    /// 自定义游戏标题
    /// </summary>
    [ObservableProperty]
    public partial string? GameTitle { get; set; }

    /// <summary>
    /// 代理端口
    /// </summary>
    [ObservableProperty]
    public partial ushort? ProxyPort { get; set; }

    /// <summary>
    /// 服务器端口
    /// </summary>
    [ObservableProperty]
    public partial ushort? Port { get; set; }

    /// <summary>
    /// GC类型
    /// </summary>
    [ObservableProperty]
    public partial GCType? Gc { get; set; }

    /// <summary>
    /// 最小内存
    /// </summary>
    [ObservableProperty]
    public partial uint? MinMem { get; set; }

    /// <summary>
    /// 最大内存
    /// </summary>
    [ObservableProperty]
    public partial uint? MaxMem { get; set; }

    /// <summary>
    /// 游戏窗口宽度
    /// </summary>
    [ObservableProperty]
    public partial uint? Width { get; set; }

    /// <summary>
    /// 游戏窗口高度
    /// </summary>
    [ObservableProperty]
    public partial uint? Height { get; set; }

    /// <summary>
    /// 自定义标题变换间隔
    /// </summary>
    [ObservableProperty]
    public partial int TitleDelay { get; set; }

    /// <summary>
    /// 是否启用自定义Java
    /// </summary>
    [ObservableProperty]
    public partial bool EnableJvmName { get; set; }

    /// <summary>
    /// 是否启用启动前运行
    /// </summary>
    [ObservableProperty]
    public partial bool PerRun { get; set; }

    /// <summary>
    /// 是否启用启动后运行
    /// </summary>
    [ObservableProperty]
    public partial bool PostRun { get; set; }

    /// <summary>
    /// 是否最大化游戏窗口
    /// </summary>
    [ObservableProperty]
    public partial bool? MaxWindow { get; set; }

    /// <summary>
    /// 是否删除原来的Java参数
    /// </summary>
    [ObservableProperty]
    public partial bool RemoveJvmArg { get; set; }

    /// <summary>
    /// 是否删除游戏启动参数
    /// </summary>
    [ObservableProperty]
    public partial bool RemoveGameArg { get; set; }

    /// <summary>
    /// 是否启用随机标题
    /// </summary>
    [ObservableProperty]
    public partial bool RandomTitle { get; set; }

    /// <summary>
    /// 是否启用标题循环
    /// </summary>
    [ObservableProperty]
    public partial bool CycTitle { get; set; }

    /// <summary>
    /// 是否在游戏启动时同时执行启动前
    /// </summary>
    [ObservableProperty]
    public partial bool PreRunSame { get; set; }

    /// <summary>
    /// 是否禁用ColorMC ASM
    /// </summary>
    [ObservableProperty]
    public partial bool ColorASM { get; set; }

    /// <summary>
    /// 是否使用自定义标题
    /// </summary>
    [ObservableProperty]
    public partial bool EditTitle { get; set; }

    /// <summary>
    /// 当前内存大小
    /// </summary>
    [ObservableProperty]
    public partial string Memory { get; set; }

    /// <summary>
    /// 是否在加载配置文件
    /// </summary>
    private bool _configLoad;

    partial void OnEditTitleChanged(bool value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.Window ??= new();
        _obj.Window.EditTitle = value;
        _obj.Save();
    }

    partial void OnColorASMChanged(bool value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.ColorASM = value;
        _obj.Save();
    }

    partial void OnCycTitleChanged(bool value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.Window ??= new();
        _obj.Window.CycTitle = value;
        _obj.Save();
    }

    partial void OnRandomTitleChanged(bool value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.Window ??= new();
        _obj.Window.RandomTitle = value;
        _obj.Save();
    }

    partial void OnTitleDelayChanged(int value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.Window ??= new();
        _obj.Window.TitleDelay = value;
        _obj.Save();
    }

    partial void OnGameTitleChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.Window ??= new();
        _obj.Window.GameTitle = value;
        _obj.Save();
    }

    partial void OnRemoveJvmArgChanged(bool value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.RemoveJvmArg = value;
        _obj.Save();
    }

    partial void OnRemoveGameArgChanged(bool value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.RemoveGameArg = value;
        _obj.Save();
    }

    partial void OnJvmEnvChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.JvmEnv = value;
        _obj.Save();
    }

    partial void OnProxyPasswordChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.ProxyHost ??= new();
        _obj.ProxyHost.Password = value;
        _obj.Save();
    }

    partial void OnProxyUserChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.ProxyHost ??= new();
        _obj.ProxyHost.User = value;
        _obj.Save();
    }

    partial void OnProxyPortChanged(ushort? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.ProxyHost ??= new();
        _obj.ProxyHost.Port = value;
        _obj.Save();
    }

    partial void OnProxyIPChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.ProxyHost ??= new();
        _obj.ProxyHost.IP = value;
        _obj.Save();
    }

    partial void OnPortChanged(ushort? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.StartServer ??= new();
        _obj.StartServer.Port = value;
        _obj.Save();
    }

    partial void OnIPChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.StartServer ??= new();
        _obj.StartServer.IP = value;
        _obj.Save();
    }

    partial void OnHeightChanged(uint? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.Window ??= new();
        _obj.Window.Height = value;
        _obj.Save();
    }

    partial void OnWidthChanged(uint? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.Window ??= new();
        _obj.Window.Width = value;
        _obj.Save();
    }

    partial void OnMaxWindowChanged(bool? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.Window ??= new();
        _obj.Window.FullScreen = value;
        _obj.Save();
    }

    partial void OnMainClassChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.AdvanceJvm ??= new();
        _obj.AdvanceJvm.MainClass = value;
        _obj.Save();
    }

    partial void OnClassPathChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.AdvanceJvm ??= new();
        _obj.AdvanceJvm.ClassPath = value;
        _obj.Save();
    }

    partial void OnGameArgChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.GameArgs = value;
        _obj.Save();
    }

    partial void OnJvmArgChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.JvmArgs = value;
        _obj.Save();
    }

    partial void OnMinMemChanged(uint? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.MinMemory = value;
        _obj.Save();
    }

    partial void OnMaxMemChanged(uint? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.MaxMemory = value;
        _obj.Save();
    }

    partial void OnPerRunChanged(bool value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.LaunchPre = value;
        _obj.Save();
    }

    partial void OnPerRunCmdChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.LaunchPreData = value;
        _obj.Save();
    }

    partial void OnPostRunChanged(bool value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.LaunchPost = value;
        _obj.Save();
    }

    partial void OnPostRunCmdChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.LaunchPostData = value;
        _obj.Save();
    }

    partial void OnPreRunSameChanged(bool value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.PreRunSame = value;
        _obj.Save();
    }

    partial void OnJvmLocalChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(JvmLocal))
        {
            EnableJvmName = true;
        }
        else
        {
            EnableJvmName = false;
        }

        if (_configLoad)
        {
            return;
        }

        _obj.JvmName = JvmName;
        _obj.JvmLocal = JvmLocal;
        _obj.Save();
    }

    partial void OnJvmNameChanged(string? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmName = JvmName;
        _obj.Save();
    }

    partial void OnGcChanged(GCType? value)
    {
        if (_configLoad)
        {
            return;
        }

        _obj.JvmArg ??= new();
        _obj.JvmArg.GC = value;
        _obj.Save();
    }

    /// <summary>
    /// 选择自定义Java
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task OpenJava()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.Java);
        if (file.Path != null)
        {
            JvmLocal = file.Path;
        }
    }

    /// <summary>
    /// 删除所有配置
    /// </summary>
    private async void DeleteConfig()
    {
        var res = await Window.ShowChoice(LangUtils.Get("GameEditWindow.Tab2.Text44"));
        if (res)
        {
            GameBinding.DeleteConfig(_obj);

            ConfigLoad();
        }
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    public void ConfigLoad()
    {
        _configLoad = true;

        var list = new List<string>()
        {
            ""
        };
        list.AddRange(JavaBinding.GetJavaName());
        JvmList.Clear();
        JvmList.AddRange(list);

        JvmName = _obj.JvmName;
        JvmLocal = _obj.JvmLocal;

        if (string.IsNullOrWhiteSpace(JvmLocal))
        {
            EnableJvmName = true;
        }
        else
        {
            EnableJvmName = false;
        }

        var config = _obj.JvmArg;
        if (config != null)
        {
            Gc = config.GC;

            MinMem = config.MinMemory == null ? null : config.MinMemory;
            MaxMem = config.MaxMemory == null ? null : config.MaxMemory;

            JvmArg = config.JvmArgs;
            GameArg = config.GameArgs;
            JvmEnv = config.JvmEnv;
            PostRunCmd = config.LaunchPostData;
            PerRunCmd = config.LaunchPreData;
            PreRunSame = config.PreRunSame;
            RemoveJvmArg = config.RemoveJvmArg ?? false;
            RemoveGameArg = config.RemoveGameArg ?? false;
            ColorASM = config.ColorASM;

            PerRun = config.LaunchPre;
            PostRun = config.LaunchPost;
        }
        else
        {
            Gc = 0;
            MinMem = null;
            MaxMem = null;
            JvmArg = null;
            GameArg = null;
            JvmEnv = null;
            PostRunCmd = null;
            PerRunCmd = null;
            PerRun = false;
            PostRun = false;
        }

        var config1 = _obj.Window;
        if (config1 != null)
        {
            Width = config1.Width;
            Height = config1.Height;
            MaxWindow = config1.FullScreen;
            GameTitle = config1.GameTitle;
            RandomTitle = config1.RandomTitle;
            CycTitle = config1.CycTitle;
            EditTitle = config1.EditTitle;
            TitleDelay = config1.TitleDelay;
        }
        else
        {
            Width = null;
            Height = null;
            MaxWindow = false;
            GameTitle = null;
        }

        var config2 = _obj.StartServer;
        if (config2 != null)
        {
            IP = config2.IP;
            Port = config2.Port;
        }
        else
        {
            IP = null;
            Port = null;
        }

        var config3 = _obj.ProxyHost;
        if (config3 != null)
        {
            ProxyIP = config3.IP;
            ProxyPort = config3.Port;
            ProxyUser = config3.User;
            ProxyPassword = config3.Password;
        }
        else
        {
            ProxyIP = null;
            ProxyPort = null;
            ProxyUser = null;
            ProxyPassword = null;
        }

        var config4 = _obj.AdvanceJvm;
        if (config4 != null)
        {
            MainClass = config4.MainClass;
            ClassPath = config4.ClassPath;
        }
        else
        {
            MainClass = null;
            ClassPath = null;
        }

        _configLoad = false;
    }
}
