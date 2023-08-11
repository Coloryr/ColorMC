using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab2Model : GameModel
{
    public List<string> GCTypeList { get; init; } = JavaBinding.GetGCTypes();
    public ObservableCollection<string> JvmList { get; init; } = new();

    [ObservableProperty]
    private string _title;
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

    private bool _load;

    public GameEditTab2Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {
        _title = string.Format(App.GetLanguage("GameEditWindow.Tab2.Text13"), Obj.Name);

        Load();
    }

    partial void OnProxyPasswordChanged(string? value)
    {
        if (_load)
            return;

        Obj.ProxyHost ??= new();
        Obj.ProxyHost.Password = value;
        Obj.Save();
    }

    partial void OnProxyUserChanged(string? value)
    {
        if (_load)
            return;

        Obj.ProxyHost ??= new();
        Obj.ProxyHost.User = value;
        Obj.Save();
    }

    partial void OnProxyPortChanged(ushort? value)
    {
        if (_load)
            return;

        Obj.ProxyHost ??= new();
        Obj.ProxyHost.Port = value;
        Obj.Save();
    }

    partial void OnProxyIPChanged(string? value)
    {
        if (_load)
            return;

        Obj.ProxyHost ??= new();
        Obj.ProxyHost.IP = value;
        Obj.Save();
    }

    partial void OnPortChanged(ushort? value)
    {
        if (_load)
            return;

        Obj.StartServer ??= new();
        Obj.StartServer.Port = value;
        Obj.Save();
    }

    partial void OnIPChanged(string? value)
    {
        if (_load)
            return;

        Obj.StartServer ??= new();
        Obj.StartServer.IP = value;
        Obj.Save();
    }

    partial void OnHeightChanged(uint? value)
    {
        if (_load)
            return;

        Obj.Window ??= new();
        Obj.Window.Height = value;
        Obj.Save();
    }

    partial void OnWidthChanged(uint? value)
    {
        if (_load)
            return;

        Obj.Window ??= new();
        Obj.Window.Width = value;
        Obj.Save();
    }

    partial void OnMaxWindowChanged(bool? value)
    {
        if (_load)
            return;

        Obj.Window ??= new();
        Obj.Window.FullScreen = value;
        Obj.Save();
    }

    partial void OnMainClassChanged(string? value)
    {
        if (_load)
            return;

        Obj.AdvanceJvm ??= new();
        Obj.AdvanceJvm.MainClass = value;
        Obj.Save();
    }

    partial void OnClassPathChanged(string? value)
    {
        if (_load)
            return;

        Obj.AdvanceJvm ??= new();
        Obj.AdvanceJvm.ClassPath = value;
        Obj.Save();
    }

    partial void OnGameArgChanged(string? value)
    {
        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.GameArgs = value;
        Obj.Save();
    }

    partial void OnJvmArgChanged(string? value)
    {
        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.JvmArgs = value;
        Obj.Save();
    }

    partial void OnJavaAgentChanged(string? value)
    {
        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.JavaAgent = value;
        Obj.Save();
    }

    partial void OnMinMemChanged(uint? value)
    {
        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.MinMemory = value;
        Obj.Save();
    }

    partial void OnMaxMemChanged(uint? value)
    {
        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.MaxMemory = value;
        Obj.Save();
    }

    partial void OnPerRunChanged(bool value)
    {
        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.LaunchPre = value;
        Obj.Save();
    }

    partial void OnPerRunCmdChanged(string? value)
    {
        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.LaunchPreData = value;
        Obj.Save();
    }

    partial void OnPostRunChanged(bool value)
    {
        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.LaunchPost = value;
        Obj.Save();
    }

    partial void OnPostRunCmdChanged(string? value)
    {
        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.LaunchPostData = value;
        Obj.Save();
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

        if (_load)
            return;

        Obj.JvmName = JvmName;
        Obj.JvmLocal = JvmLocal;
        Obj.Save();
    }

    partial void OnJvmNameChanged(string? value)
    {
        if (_load)
            return;

        Obj.JvmName = JvmName;
        Obj.Save();
    }

    partial void OnJvmGcChanged(string? value)
    {
        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.GCArgument = JvmGc;
        Obj.Save();
    }

    partial void OnGcChanged(int? value)
    {
        EnableGc = Gc == 5;

        if (_load)
            return;

        Obj.JvmArg ??= new();
        Obj.JvmArg.GC = Gc == 0 ? null : (GCType?)(Gc - 1);
        Obj.Save();
    }

    [RelayCommand]
    public async Task Delete()
    {
        var res = await ShowWait(App.GetLanguage("GameEditWindow.Tab2.Info1"));
        if (res)
        {
            GameBinding.DeleteConfig(Obj);

            Load();
        }
    }

    [RelayCommand]
    public async Task Open()
    {
        var file = await PathBinding.SelectFile(Window, FileType.Java);
        if (file != null)
        {
            JvmLocal = file;
        }
    }

    public void Load()
    {
        _load = true;

        var list = new List<string>()
        {
            ""
        };
        list.AddRange(JavaBinding.GetJavaName());
        JvmList.Clear();
        JvmList.AddRange(list);

        JvmName = Obj.JvmName;
        JvmLocal = Obj.JvmLocal;

        if (string.IsNullOrWhiteSpace(JvmLocal))
        {
            EnableJvmName = true;
        }
        else
        {
            EnableJvmName = false;
        }

        var config = Obj.JvmArg;
        if (config != null)
        {
            Gc = config.GC == null ? 0 : (int)(config.GC + 1);

            MinMem = config.MinMemory == null ? null : config.MinMemory;
            MaxMem = config.MaxMemory == null ? null : config.MaxMemory;

            JvmGc = config.GCArgument;
            JavaAgent = config.JavaAgent;
            JvmArg = config.JvmArgs;
            GameArg = config.GameArgs;
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
            PostRunCmd = null;
            PerRunCmd = null;
            PerRun = false;
            PostRun = false;
        }

        var config1 = Obj.Window;
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

        var config2 = Obj.StartServer;
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

        var config3 = Obj.ProxyHost;
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

        var config4 = Obj.AdvanceJvm;
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

        _load = false;
    }

    public override void Close()
    {
        JvmList.Clear();
    }
}
