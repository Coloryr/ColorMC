using Avalonia.Media;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel : MenuModel
{
    private readonly List<string> _uuids = new();

    public ObservableCollection<string> GameList { get; init; } = new();

    public List<string> LoginList { get; init; } = UserBinding.GetLoginType();

    [ObservableProperty]
    private Color _color1;
    [ObservableProperty]
    private Color _color2;

    [ObservableProperty]
    private string _serverIP;
    [ObservableProperty]
    private string? _fileUI;
    [ObservableProperty]
    private string? _music;
    [ObservableProperty]
    private string? _loginUrl;

    [ObservableProperty]
    private ushort? _serverPort;

    [ObservableProperty]
    private bool _enableMotd;
    [ObservableProperty]
    private bool _enableJoin;
    [ObservableProperty]
    private bool _enableIP;
    [ObservableProperty]
    private bool _enableOneGame;
    [ObservableProperty]
    private bool _enableOneLogin;
    [ObservableProperty]
    private bool _enableOneLoginUrl;
    [ObservableProperty]
    private bool _enableMusic;
    [ObservableProperty]
    private bool _slowVolume;
    [ObservableProperty]
    private bool _runPause;

    [ObservableProperty]
    private int _game = -1;
    [ObservableProperty]
    private int _volume;
    [ObservableProperty]
    private int _login = -1;

    private bool _serverLoad;

    partial void OnLoginUrlChanged(string? value)
    {
        SetLoginLock();
    }

    partial void OnLoginChanged(int value)
    {
        var type = (AuthType)(value + 1);
        if (type is AuthType.Nide8 or AuthType.AuthlibInjector or AuthType.SelfLittleSkin)
        {
            EnableOneLoginUrl = true;
        }
        else
        {
            EnableOneLoginUrl = false;
        }

        SetLoginLock();
    }

    partial void OnEnableOneLoginChanged(bool value)
    {
        SetLoginLock();
    }

    partial void OnVolumeChanged(int value)
    {
        BaseBinding.SetVolume(value);

        SetMusic();
    }

    partial void OnMusicChanged(string? value)
    {
        SetMusic();
    }

    partial void OnEnableMusicChanged(bool value)
    {
        SetMusic();
    }

    partial void OnSlowVolumeChanged(bool value)
    {
        SetMusic();
    }

    partial void OnRunPauseChanged(bool value)
    {
        SetMusic();
    }

    partial void OnFileUIChanged(string? value)
    {
        ConfigBinding.SetUI(value);
    }

    partial void OnColor1Changed(Color value)
    {
        SetIP();
    }

    partial void OnColor2Changed(Color value)
    {
        SetIP();
    }

    partial void OnEnableOneGameChanged(bool value)
    {
        SetOneGame();
    }

    partial void OnGameChanged(int value)
    {
        SetOneGame();
    }

    partial void OnServerPortChanged(ushort? value)
    {
        SetIP();
    }

    partial void OnIPChanged(string value)
    {
        SetIP();
    }

    partial void OnEnableMotdChanged(bool value)
    {
        EnableIP = EnableJoin || EnableMotd;

        SetIP();
    }

    partial void OnEnableJoinChanged(bool value)
    {
        EnableIP = EnableJoin || EnableMotd;

        SetIP();
    }

    [RelayCommand]
    public async Task SelectUI()
    {
        var res = await PathBinding.SelectFile(FileType.UI);
        if (res != null)
        {
            FileUI = res;
        }
    }

    [RelayCommand]
    public void Delete()
    {
        FileUI = "";
    }

    [RelayCommand]
    public void Test()
    {
        if (string.IsNullOrWhiteSpace(FileUI))
        {
            Model.Show(App.GetLanguage("Gui.Error8"));
            return;
        }
        var res = BaseBinding.TestCustomWindow(FileUI);
        if (!res.Item1)
        {
            Model.Show(res.Item2!);
        }
    }

    [RelayCommand]
    public async Task Save()
    {
        var str = await PathBinding.SaveFile(FileType.UI, null);
        if (str == null)
            return;

        if (str == false)
        {
            Model.Show(App.GetLanguage("SettingWindow.Tab6.Error3"));
            return;
        }

        Model.Notify(App.GetLanguage("SettingWindow.Tab6.Info4"));
    }

    [RelayCommand]
    public void MusicPlay()
    {
        BaseBinding.MusicPlay();
    }

    [RelayCommand]
    public void MusicPause()
    {
        BaseBinding.MusicPause();
    }

    [RelayCommand]
    public void MusicStart()
    {
        BaseBinding.MusicStart();
    }

    [RelayCommand]
    public void MusicStop()
    {
        BaseBinding.MusicStop();
    }

    [RelayCommand]
    public async Task SelectMusic()
    {
        var file = await PathBinding.SelectFile(FileType.Music);
        if (file == null)
        {
            return;
        }

        Music = file;
    }

    public void LoadServer()
    {
        _serverLoad = true;

        var list = from item in GameBinding.GetGames() select (item.UUID, item.Name);
        var list1 = new List<string>();

        _uuids.Clear();
        foreach (var (UUID, Name) in list)
        {
            list1.Add(Name);
            _uuids.Add(UUID);
        }

        GameList.Clear();
        GameList.AddRange(list1);

        if (ConfigBinding.GetAllConfig().Item2?.ServerCustom is { } config)
        {
            ServerIP = config.IP;
            ServerPort = config.Port;
            FileUI = config.UIFile;
            Music = config.Music;

            EnableMotd = config.Motd;
            EnableJoin = config.JoinServer;
            EnableOneGame = config.LockGame;
            EnableMusic = config.PlayMusic;
            RunPause = config.RunPause;
            SlowVolume = config.SlowVolume;

            Color1 = ColorSel.MotdColor.ToColor();
            Color2 = ColorSel.MotdBackColor.ToColor();
            if (config.GameName == null)
            {
                Game = -1;
            }
            else
            {
                Game = _uuids.IndexOf(config.GameName);
            }

            Volume = config.Volume;

            LoginUrl = config.LoginUrl;
            Login = config.LoginType;
            EnableOneLogin = config.LockLogin;
        }

        _serverLoad = false;
    }

    private void SetMusic()
    {
        if (_serverLoad)
            return;

        ConfigBinding.SetMusic(EnableMusic, SlowVolume, Music, Volume, RunPause);
    }

    private void SetLoginLock()
    {
        if (_serverLoad)
            return;

        ConfigBinding.SetLoginLock(EnableOneLogin, Login, LoginUrl!);
    }

    private void SetIP()
    {
        if (_serverLoad)
            return;

        ConfigBinding.SetMotd(ServerIP, ServerPort ?? 0, EnableMotd,
            EnableJoin, Color1.ToString(), Color2.ToString());
    }

    private void SetOneGame()
    {
        if (_serverLoad)
            return;

        ConfigBinding.SetOneGame(EnableOneGame, Game == -1 ? null : _uuids[Game]);
    }
}
