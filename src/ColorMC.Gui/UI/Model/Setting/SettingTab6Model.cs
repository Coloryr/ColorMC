using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Setting;

/// <summary>
/// 设置页面
/// </summary>
public partial class SettingModel
{
    public const string NameLockLogin = "AddLockLogin";

    /// <summary>
    /// UUID列表
    /// </summary>
    private readonly List<string> _uuids = [];
    /// <summary>
    /// 游戏列表
    /// </summary>
    public ObservableCollection<string> GameList { get; init; } = [];
    /// <summary>
    /// 账户锁定列表
    /// </summary>
    public ObservableCollection<LockLoginModel> Locks { get; init; } = [];
    /// <summary>
    /// 登录类型列表
    /// </summary>
    public string[] LoginList { get; init; } = LanguageBinding.GetLockLoginType();
    /// <summary>
    /// 自定义主界面动画类型
    /// </summary>
    public string[] DisplayList { get; init; } = LanguageBinding.GetDisplayList();

    /// <summary>
    /// 选中的锁定类型
    /// </summary>
    [ObservableProperty]
    private LockLoginModel _lockSelect;

    /// <summary>
    /// Motd字体颜色
    /// </summary>
    [ObservableProperty]
    private Color _motdFontColor;
    /// <summary>
    /// Motd背景颜色
    /// </summary>
    [ObservableProperty]
    private Color _motdBackColor;

    /// <summary>
    /// 服务器地址
    /// </summary>
    [ObservableProperty]
    private string _serverIP;
    /// <summary>
    /// 服务器背景音乐
    /// </summary>
    [ObservableProperty]
    private string? _music;
    /// <summary>
    /// 自定义启动页面文本
    /// </summary>
    [ObservableProperty]
    private string? _startText;

    /// <summary>
    /// 服务器端口
    /// </summary>
    [ObservableProperty]
    private ushort? _serverPort;

    /// <summary>
    /// 是否启用Motd
    /// </summary>
    [ObservableProperty]
    private bool _enableMotd;
    /// <summary>
    /// 是否自动加入服务器
    /// </summary>
    [ObservableProperty]
    private bool _enableJoin;
    /// <summary>
    /// 是否启用服务器
    /// </summary>
    [ObservableProperty]
    private bool _enableIP;
    /// <summary>
    /// 是否是锁定游戏实例
    /// </summary>
    [ObservableProperty]
    private bool _enableOneGame;
    /// <summary>
    /// 是否锁定登录账户类型
    /// </summary>
    [ObservableProperty]
    private bool _enableOneLogin;
    /// <summary>
    /// 是否启用背景音乐
    /// </summary>
    [ObservableProperty]
    private bool _enableMusic;
    /// <summary>
    /// 缓慢加载音乐
    /// </summary>
    [ObservableProperty]
    private bool _slowVolume;
    /// <summary>
    /// 是否启动游戏后暂停背景音乐
    /// </summary>
    [ObservableProperty]
    private bool _runPause;
    /// <summary>
    /// 是否启用自定义UI
    /// </summary>
    [ObservableProperty]
    private bool _enableUI;
    /// <summary>
    /// 是否启用背景音乐循环
    /// </summary>
    [ObservableProperty]
    private bool _loop;
    /// <summary>
    /// 是否管理员启动
    /// </summary>
    [ObservableProperty]
    private bool _adminLaunch;
    /// <summary>
    /// 是否游戏管理员启动
    /// </summary>
    [ObservableProperty]
    private bool _gameAdminLaunch;
    /// <summary>
    /// 是否自定义图标
    /// </summary>
    [ObservableProperty]
    private bool _customIcon;
    /// <summary>
    /// 是否自定义启动图标
    /// </summary>
    [ObservableProperty]
    private bool _customStart;

    /// <summary>
    /// 选中的游戏实例
    /// </summary>
    [ObservableProperty]
    private int _game = -1;
    /// <summary>
    /// 背景音乐音量
    /// </summary>
    [ObservableProperty]
    private int _volume;

    /// <summary>
    /// 图标图片
    /// </summary>
    [ObservableProperty]
    private Bitmap? _iconImage;
    /// <summary>
    /// 开始启动页面图片
    /// </summary>
    [ObservableProperty]
    private Bitmap? _startImage;

    /// <summary>
    /// 启动页面动画类型
    /// </summary>
    [ObservableProperty]
    private DisplayType _displayType;

    /// <summary>
    /// 配置是否加载中
    /// </summary>
    private bool _serverLoad = true;

    //配置修改
    partial void OnGameAdminLaunchChanged(bool value)
    {
        if (_serverLoad)
        {
            return;
        }

        SetAdmin();
    }

    partial void OnAdminLaunchChanged(bool value)
    {
        if (_serverLoad)
        {
            return;
        }

        SetAdmin();
    }

    partial void OnLoopChanged(bool value)
    {
        if (_serverLoad)
        {
            return;
        }

        SetMusic();
    }

    partial void OnEnableUIChanged(bool value)
    {
        SetUI();
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

    partial void OnServerIPChanged(string value)
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

    partial void OnCustomIconChanged(bool value)
    {
        SetUI();
    }

    partial void OnCustomStartChanged(bool value)
    {
        SetUI();
    }

    partial void OnDisplayTypeChanged(DisplayType value)
    {
        SetUI();
    }

    partial void OnStartTextChanged(string? value)
    {
        SetUI();
    }

    /// <summary>
    /// 选择开始页面图标
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SelectStartIcon()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SelectFile(top, FileType.StartIcon);
        if (res.Item1 == null)
        {
            return;
        }

        BaseBinding.SetStartIcon(res.Item1);
        StartImage = BaseBinding.GetStartIcon();
    }
    /// <summary>
    /// 选择图标
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SelectIcon()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SelectFile(top, FileType.Icon);
        if (res.Item1 == null)
        {
            return;
        }

        BaseBinding.SetWindowIcon(res.Item1);
        IconImage = BaseBinding.GetWindowIcon();
    }
    /// <summary>
    /// 测试自定义界面
    /// </summary>
    [RelayCommand]
    public void Test()
    {
        var res = BaseBinding.TestCustomWindow();
        if (!res)
        {
            Model.Show(App.Lang("BaseBinding.Error8"));
        }
    }
    /// <summary>
    /// 打开自定义页面教程
    /// </summary>
    [RelayCommand]
    public void UIGuide()
    {
        WebBinding.OpenWeb(WebType.UIGuide);
    }
    /// <summary>
    /// 背景音乐播放
    /// </summary>
    [RelayCommand]
    public void MusicPlay()
    {
        BaseBinding.MusicPlay();
    }
    /// <summary>
    /// 背景音乐暂停
    /// </summary>
    [RelayCommand]
    public void MusicPause()
    {
        BaseBinding.MusicPause();
    }
    /// <summary>
    /// 背景音乐开始
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task MusicStart()
    {
        if (string.IsNullOrWhiteSpace(Music))
        {
            return;
        }
        await BaseBinding.MusicStart(Music!, Loop, SlowVolume, Volume);
    }
    /// <summary>
    /// 背景音乐停止
    /// </summary>
    [RelayCommand]
    public void MusicStop()
    {
        BaseBinding.MusicStop();
    }
    /// <summary>
    /// 选择背景音乐
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SelectMusic()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.Music);
        if (file.Item1 == null)
        {
            return;
        }

        Music = file.Item1;
    }
    /// <summary>
    /// 添加账户类型锁定
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddLockLogin()
    {
        var model = new AddLockLoginModel();
        var res = await DialogHost.Show(model, NameLockLogin);
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
    /// <summary>
    /// 删除账户锁定类型
    /// </summary>
    /// <param name="model"></param>
    public void Delete(LockLoginModel model)
    {
        Locks.Remove(model);

        SetLoginLock();
    }
    /// <summary>
    /// 加载配置
    /// </summary>
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
            RunPause = config.RunPause;
            SlowVolume = config.SlowVolume;
            Loop = config.MusicLoop;
            AdminLaunch = config.AdminLaunch;
            GameAdminLaunch = config.GameAdminLaunch;

            MotdFontColor = ColorManager.MotdColor.ToColor();
            MotdBackColor = ColorManager.MotdBackColor.ToColor();
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

            EnableUI = config.EnableUI;
            CustomIcon = config.CustomIcon;
            IconImage = BaseBinding.GetWindowIcon();
            StartImage = BaseBinding.GetStartIcon();
            CustomStart = config.CustomStart;
            StartText = config.StartText;
            DisplayType = config.DisplayType;
        }

        _serverLoad = false;
    }

    //保存配置
    private void SetUI()
    {
        if (_serverLoad)
        {
            return;
        }

        ConfigBinding.SetUI(EnableUI, CustomIcon, CustomStart, StartText, DisplayType);
    }

    private void SetMusic()
    {
        if (_serverLoad)
        {
            return;
        }

        ConfigBinding.SetMusic(EnableMusic, SlowVolume, Music, Volume, RunPause, Loop);
    }

    private void SetLoginLock()
    {
        if (_serverLoad)
        {
            return;
        }

        var list = new List<LockLoginSetting>();
        foreach (var item in Locks)
        {
            list.Add(item.Login);
        }

        ConfigBinding.SetLoginLock(EnableOneLogin, list);
    }

    private void SetIP()
    {
        if (_serverLoad)
        {
            return;
        }

        ConfigBinding.SetMotd(ServerIP, ServerPort ?? 0, EnableMotd,
            EnableJoin, MotdFontColor.ToString(), MotdBackColor.ToString());
    }

    private void SetOneGame()
    {
        if (_serverLoad)
        {
            return;
        }

        ConfigBinding.SetLockGame(EnableOneGame, Game == -1 ? null : _uuids[Game]);
    }

    private void SetAdmin()
    {
        ConfigBinding.SetAdmin(AdminLaunch, GameAdminLaunch);
    }

    private void ShowBuildPack()
    {
        WindowManager.ShowBuildPack();
    }
}
