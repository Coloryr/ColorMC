using Avalonia.Media;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab6Model : BaseModel
{
    private readonly List<string> _uuids = new();

    public ObservableCollection<string> GameList { get; init; } = new();

    public List<string> LoginList { get; init; } = UserBinding.GetLoginType();

    [ObservableProperty]
    private Color _color1;
    [ObservableProperty]
    private Color _color2;

    [ObservableProperty]
    private string _iP;
    [ObservableProperty]
    private string? _fileUI;
    [ObservableProperty]
    private string? _serverUrl;
    [ObservableProperty]
    private string? _music;
    [ObservableProperty]
    private string? _loginUrl;

    [ObservableProperty]
    private ushort _port;

    [ObservableProperty]
    private bool _enableMotd;
    [ObservableProperty]
    private bool _enableJoin;
    [ObservableProperty]
    private bool enableIP;
    [ObservableProperty]
    private bool _enableOneGame;
    [ObservableProperty]
    private bool _enableOneLogin;
    [ObservableProperty]
    private bool _enableOneLoginUrl;
    [ObservableProperty]
    private bool _enableServerPack;
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

    private bool _load;

    public SettingTab6Model(IUserControl con) : base(con)
    {

    }

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

    partial void OnEnableServerPackChanged(bool value)
    {
        SetServerPack();
    }

    partial void OnServerUrlChanged(string? value)
    {
        SetServerPack();
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

    partial void OnPortChanged(ushort value)
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
        var res = await PathBinding.SelectFile(Window, FileType.UI);
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
            Show(App.GetLanguage("Gui.Error8"));
            return;
        }
        var res = BaseBinding.TestCustomWindow(FileUI);
        if (!res.Item1)
        {
            Show(res.Item2!);
        }
    }

    [RelayCommand]
    public async Task Save()
    {
        var str = await PathBinding.SaveFile(Window, FileType.UI, null);
        if (str == null)
            return;

        if (str == false)
        {
            Show(App.GetLanguage("SettingWindow.Tab6.Error3"));
            return;
        }

        Notify(App.GetLanguage("SettingWindow.Tab6.Info4"));
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
        var file = await PathBinding.SelectFile(Window, FileType.Music);
        if (file == null)
        {
            return;
        }

        Music = file;
    }

    public void Load()
    {
        _load = true;

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
            IP = config.IP;
            Port = config.Port;
            FileUI = config.UIFile;
            ServerUrl = config.ServerUrl;
            Music = config.Music;

            EnableMotd = config.Motd;
            EnableJoin = config.JoinServer;
            EnableOneGame = config.LockGame;
            EnableServerPack = config.ServerPack;
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

        _load = false;
    }

    private void SetMusic()
    {
        if (_load)
            return;

        ConfigBinding.SetMusic(EnableMusic, SlowVolume, Music, Volume, RunPause);
    }

    private void SetLoginLock()
    {
        if (_load)
            return;

        ConfigBinding.SetLoginLock(EnableOneLogin, Login, LoginUrl!);
    }

    private void SetServerPack()
    {
        if (_load)
            return;

        ConfigBinding.SetServerPack(EnableServerPack, ServerUrl);
    }

    private void SetIP()
    {
        if (_load)
            return;

        ConfigBinding.SetMotd(IP, Port, EnableMotd,
            EnableJoin, Color1.ToString(), Color2.ToString());
    }

    private void SetOneGame()
    {
        if (_load)
            return;

        ConfigBinding.SetOneGame(EnableOneGame, Game == -1 ? null : _uuids[Game]);
    }
}
