using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    private readonly List<string> _uuids = [];

    public ObservableCollection<string> GameList { get; init; } = [];

    public ObservableCollection<LockLoginModel> Locks { get; init; } = [];

    public string[] LoginList { get; init; } = UserBinding.GetLockLoginType();

    [ObservableProperty]
    private LockLoginModel? _lockSelect;

    [ObservableProperty]
    private Color _motdFontColor;
    [ObservableProperty]
    private Color _motdBackColor;

    [ObservableProperty]
    private string _serverIP;
    [ObservableProperty]
    private string? _music;

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
    private bool _enableMusic;
    [ObservableProperty]
    private bool _slowVolume;
    [ObservableProperty]
    private bool _runPause;
    [ObservableProperty]
    private bool _enableUI;
    [ObservableProperty]
    private bool _loop;

    [ObservableProperty]
    private int _game = -1;
    [ObservableProperty]
    private int _volume;

    private bool _serverLoad = true;

    partial void OnLoopChanged(bool value)
    {
        if (_serverLoad)
            return;

        SetMusic();
    }

    partial void OnEnableUIChanged(bool value)
    {
        if (_serverLoad)
            return;

        ConfigBinding.SetUI(value);
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

    partial void OnMotdFontColorChanged(Color value)
    {
        SetIP();
    }

    partial void OnMotdBackColorChanged(Color value)
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
    public void Test()
    {
        var res = BaseBinding.TestCustomWindow();
        if (!res.Item1)
        {
            Model.Show(res.Item2!);
        }
    }

    [RelayCommand]
    public void UIGuide()
    {
        WebBinding.OpenWeb(WebType.UIGuide);
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
        if (file.Item1 == null)
        {
            return;
        }

        Music = file.Item1;
    }

    [RelayCommand]
    public async Task AddLockLogin()
    {
        var model = new AddLockLoginModel();
        var res = await DialogHost.Show(model, "AddLockLogin");
        if (res is true)
        {
            if (model.Index == 0)
            {
                foreach (var item in Locks)
                {
                    if (item.AuthType == AuthType.OAuth)
                    {
                        Model.Show(App.Lang("SettingWindow.Tab6.Error4"));
                        return;
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.InputText)
                    || string.IsNullOrWhiteSpace(model.InputText1))
                {
                    Model.Show(App.Lang("SettingWindow.Tab6.Error5"));
                    return;
                }
                foreach (var item in Locks)
                {
                    if (item.Name == model.InputText)
                    {
                        Model.Show(App.Lang("SettingWindow.Tab6.Error6"));
                        return;
                    }
                }
            }

            Locks.Add(new(this, new()
            {
                Name = model.InputText,
                Data = model.InputText1,
                Type = (AuthType)(model.Index + 1)
            }));

            SetLoginLock();
        }
    }

    public void Delete(LockLoginModel model)
    {
        Locks.Remove(model);

        SetLoginLock();
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

        var config = GuiConfigUtils.Config.ServerCustom;
        if (config is { })
        {
            ServerIP = config.IP;
            ServerPort = config.Port;
            Music = config.Music;

            EnableMotd = config.Motd;
            EnableJoin = config.JoinServer;
            EnableOneGame = config.LockGame;
            EnableMusic = config.PlayMusic;
            EnableUI = config.EnableUI;
            RunPause = config.RunPause;
            SlowVolume = config.SlowVolume;
            Loop = config.MusicLoop;

            MotdFontColor = ColorSel.MotdColor.ToColor();
            MotdBackColor = ColorSel.MotdBackColor.ToColor();
            if (config.GameName == null)
            {
                Game = -1;
            }
            else
            {
                Game = _uuids.IndexOf(config.GameName);
            }

            Volume = config.Volume;

            EnableOneLogin = config.LockLogin;

            Locks.Clear();
            foreach (var item in config.LockLogins)
            {
                Locks.Add(new(this, item));
            }
        }

        _serverLoad = false;
    }

    private void SetMusic()
    {
        if (_serverLoad)
            return;

        ConfigBinding.SetMusic(EnableMusic, SlowVolume, Music, Volume, RunPause, Loop);
    }

    private void SetLoginLock()
    {
        if (_serverLoad)
            return;

        var list = new List<LockLoginSetting>();
        foreach (var item in Locks)
        {
            list.Add(item.login);
        }

        ConfigBinding.SetLoginLock(EnableOneLogin, list);
    }

    private void SetIP()
    {
        if (_serverLoad)
            return;

        ConfigBinding.SetMotd(ServerIP, ServerPort ?? 0, EnableMotd,
            EnableJoin, MotdFontColor.ToString(), MotdBackColor.ToString());
    }

    private void SetOneGame()
    {
        if (_serverLoad)
            return;

        ConfigBinding.SetLockGame(EnableOneGame, Game == -1 ? null : _uuids[Game]);
    }
}
