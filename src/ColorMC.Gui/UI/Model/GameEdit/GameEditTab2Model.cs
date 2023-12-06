using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel
{
    public List<string> GCTypeList { get; init; } = JavaBinding.GetGCTypes();
    public ObservableCollection<string> JvmList { get; init; } = [];

    [ObservableProperty]
    private string _titleText;
    [ObservableProperty]
    private string? _jvmLocal;
    [ObservableProperty]
    private string? _jvmName;
    [ObservableProperty]
    private string? _jvmGc;
    [ObservableProperty]
    private string? _perRunCmd;
    [ObservableProperty]
    private string? _postRunCmd;
    [ObservableProperty]
    private string? _javaAgent;
    [ObservableProperty]
    private string? _jvmArg;
    [ObservableProperty]
    private string? _gameArg;
    [ObservableProperty]
    private string? _classPath;
    [ObservableProperty]
    private string? _mainClass;
    [ObservableProperty]
    private string? _iP;
    [ObservableProperty]
    private string? _proxyIP;
    [ObservableProperty]
    private string? _proxyUser;
    [ObservableProperty]
    private string? _proxyPassword;
    [ObservableProperty]
    private string? _jvmEnv;

    [ObservableProperty]
    private ushort? _proxyPort;
    [ObservableProperty]
    private ushort? _port;
    [ObservableProperty]
    private int? _gc;
    [ObservableProperty]
    private uint? _minMem;
    [ObservableProperty]
    private uint? _maxMem;
    [ObservableProperty]
    private uint? _width;
    [ObservableProperty]
    private uint? _height;

    [ObservableProperty]
    private bool _enableGc;
    [ObservableProperty]
    private bool _enableJvmName;

    [ObservableProperty]
    private bool _perRun;
    [ObservableProperty]
    private bool _postRun;
    [ObservableProperty]
    private bool? _maxWindow;

    private bool _configLoad;

    partial void OnJvmEnvChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.JvmEnv = value;
        _obj.Save();
    }

    partial void OnProxyPasswordChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.ProxyHost ??= new();
        _obj.ProxyHost.Password = value;
        _obj.Save();
    }

    partial void OnProxyUserChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.ProxyHost ??= new();
        _obj.ProxyHost.User = value;
        _obj.Save();
    }

    partial void OnProxyPortChanged(ushort? value)
    {
        if (_configLoad)
            return;

        _obj.ProxyHost ??= new();
        _obj.ProxyHost.Port = value;
        _obj.Save();
    }

    partial void OnProxyIPChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.ProxyHost ??= new();
        _obj.ProxyHost.IP = value;
        _obj.Save();
    }

    partial void OnPortChanged(ushort? value)
    {
        if (_configLoad)
            return;

        _obj.StartServer ??= new();
        _obj.StartServer.Port = value;
        _obj.Save();
    }

    partial void OnIPChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.StartServer ??= new();
        _obj.StartServer.IP = value;
        _obj.Save();
    }

    partial void OnHeightChanged(uint? value)
    {
        if (_configLoad)
            return;

        _obj.Window ??= new();
        _obj.Window.Height = value;
        _obj.Save();
    }

    partial void OnWidthChanged(uint? value)
    {
        if (_configLoad)
            return;

        _obj.Window ??= new();
        _obj.Window.Width = value;
        _obj.Save();
    }

    partial void OnMaxWindowChanged(bool? value)
    {
        if (_configLoad)
            return;

        _obj.Window ??= new();
        _obj.Window.FullScreen = value;
        _obj.Save();
    }

    partial void OnMainClassChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.AdvanceJvm ??= new();
        _obj.AdvanceJvm.MainClass = value;
        _obj.Save();
    }

    partial void OnClassPathChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.AdvanceJvm ??= new();
        _obj.AdvanceJvm.ClassPath = value;
        _obj.Save();
    }

    partial void OnGameArgChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.GameArgs = value;
        _obj.Save();
    }

    partial void OnJvmArgChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.JvmArgs = value;
        _obj.Save();
    }

    partial void OnJavaAgentChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.JavaAgent = value;
        _obj.Save();
    }

    partial void OnMinMemChanged(uint? value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.MinMemory = value;
        _obj.Save();
    }

    partial void OnMaxMemChanged(uint? value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.MaxMemory = value;
        _obj.Save();
    }

    partial void OnPerRunChanged(bool value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.LaunchPre = value;
        _obj.Save();
    }

    partial void OnPerRunCmdChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.LaunchPreData = value;
        _obj.Save();
    }

    partial void OnPostRunChanged(bool value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.LaunchPost = value;
        _obj.Save();
    }

    partial void OnPostRunCmdChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.LaunchPostData = value;
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
            return;

        _obj.JvmName = JvmName;
        _obj.JvmLocal = JvmLocal;
        _obj.Save();
    }

    partial void OnJvmNameChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.JvmName = JvmName;
        _obj.Save();
    }

    partial void OnJvmGcChanged(string? value)
    {
        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.GCArgument = JvmGc;
        _obj.Save();
    }

    partial void OnGcChanged(int? value)
    {
        EnableGc = Gc == 5;

        if (_configLoad)
            return;

        _obj.JvmArg ??= new();
        _obj.JvmArg.GC = Gc == 0 ? null : (GCType?)(Gc - 1);
        _obj.Save();
    }

    [RelayCommand]
    public async Task DeleteConfig()
    {
        var res = await Model.ShowWait(App.Lang("GameEditWindow.Tab2.Info1"));
        if (res)
        {
            GameBinding.DeleteConfig(_obj);

            ConfigLoad();
        }
    }

    [RelayCommand]
    public async Task OpenJava()
    {
        var file = await PathBinding.SelectFile(FileType.Java);
        if (file != null)
        {
            JvmLocal = file;
        }
    }

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
            Gc = config.GC == null ? 0 : (int)(config.GC + 1);

            MinMem = config.MinMemory == null ? null : config.MinMemory;
            MaxMem = config.MaxMemory == null ? null : config.MaxMemory;

            JvmGc = config.GCArgument;
            JavaAgent = config.JavaAgent;
            JvmArg = config.JvmArgs;
            GameArg = config.GameArgs;
            JvmEnv = config.JvmEnv;
            PostRunCmd = config.LaunchPostData;
            PerRunCmd = config.LaunchPreData;

            PerRun = config.LaunchPre;
            PostRun = config.LaunchPost;
        }
        else
        {
            Gc = 0;
            MinMem = null;
            MaxMem = null;
            JvmGc = null;
            JavaAgent = null;
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
        }
        else
        {
            Width = null;
            Height = null;
            MaxWindow = false;
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
