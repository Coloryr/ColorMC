using Avalonia.Media;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab6Model : ObservableObject
{
    private readonly IUserControl Con;

    private readonly List<string> uuids = new();

    public ObservableCollection<string> GameList { get; init; } = new();

    [ObservableProperty]
    private Color color1;
    [ObservableProperty]
    private Color color2;

    [ObservableProperty]
    private string iP;
    [ObservableProperty]
    private string? fileUI;
    [ObservableProperty]
    private string? serverUrl;
    [ObservableProperty]
    private string? music;

    [ObservableProperty]
    private ushort port;

    [ObservableProperty]
    private bool enableMotd;
    [ObservableProperty]
    private bool enableJoin;
    [ObservableProperty]
    private bool enableIP;
    [ObservableProperty]
    private bool enableOneGame;
    [ObservableProperty]
    private bool enableServerPack;
    [ObservableProperty]
    private bool enableMusic;
    [ObservableProperty]
    private bool slowVolume;

    [ObservableProperty]
    private int game = -1;
    [ObservableProperty]
    private int volume;

    private bool load;

    public SettingTab6Model(IUserControl con)
    {
        Con = con;
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
    public async void SelectUI()
    {
        var window = Con.Window;
        var res = await BaseBinding.OpFile(window, FileType.UI);
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
        var window = Con.Window;
        var file = FileUI;
        if (string.IsNullOrWhiteSpace(file))
        {
            window.OkInfo.Show(App.GetLanguage("Gui.Error8"));
            return;
        }

        if (!File.Exists(file))
        {
            file = BaseBinding.GetRunDir() + file;
            if (!File.Exists(file))
            {
                window.OkInfo.Show(App.GetLanguage("Gui.Error9"));
                return;
            }
        }
        try
        {
            var obj = JsonConvert.DeserializeObject<UIObj>(File.ReadAllText(file));
            if (obj == null)
            {
                window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab6.Error1"));
                return;
            }

            App.ShowCustom(obj);
        }
        catch (Exception ex)
        {
            var data = App.GetLanguage("SettingWindow.Tab6.Error2");
            Logs.Error(data, ex);
            App.ShowError(data, ex);
        }
    }

    [RelayCommand]
    public async void Save()
    {
        var window = Con.Window;
        var str = await BaseBinding.SaveFile(window, FileType.UI, null);
        if (str == null)
            return;

        if (str == false)
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab6.Error3"));
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab6.Info4"));
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
    public async void SelectMusic()
    {
        var window = Con.Window;
        var file = await BaseBinding.OpFile(window, FileType.Music);
        if (file == null)
        {
            return;
        }

        Music = file;
    }

    public void Load()
    {
        load = true;

        var list = from item in GameBinding.GetGames() select (item.UUID, item.Name);
        var list1 = new List<string>();

        uuids.Clear();
        foreach (var (UUID, Name) in list)
        {
            list1.Add(Name);
            uuids.Add(UUID);
        }

        GameList.Clear();
        GameList.AddRange(list1);

        var config = ConfigBinding.GetAllConfig().Item2?.ServerCustom;

        if (config != null)
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
            SlowVolume = config.SlowVolume;

            Color1 = ColorSel.MotdColor.ToColor();
            Color2 = ColorSel.MotdBackColor.ToColor();

            Game = uuids.IndexOf(config.GameName);

            Volume = config.Volume;
        }

        load = false;
    }

    private void SetMusic()
    {
        if (load)
            return;

        ConfigBinding.SetMusic(EnableMusic, SlowVolume, Music, Volume);
    }

    private void SetServerPack()
    {
        if (load)
            return;

        ConfigBinding.SetServerPack(EnableServerPack, ServerUrl);
    }

    private void SetIP()
    {
        if (load)
            return;

        ConfigBinding.SetMotd(IP, Port, EnableMotd,
            EnableJoin, Color1.ToString(), Color2.ToString());
    }

    private void SetOneGame()
    {
        if (load)
            return;

        ConfigBinding.SetOneGame(EnableOneGame, Game == -1 ? null : uuids[Game]);
    }
}
