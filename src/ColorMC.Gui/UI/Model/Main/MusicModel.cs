using System;
using System.IO;
using Avalonia.Animation;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 主界面
/// </summary>
public partial class MainModel
{
    /// <summary>
    /// 音乐是否在播放
    /// </summary>
    private bool _isplay = true;

    /// <summary>
    /// 音乐音量
    /// </summary>
    [ObservableProperty]
    private int _musicVolume;

    /// <summary>
    /// 音乐名字
    /// </summary>
    [ObservableProperty]
    private string _musicName;
    /// <summary>
    /// 当前播放进度
    /// </summary>
    [ObservableProperty]
    private string _musicNow;

    /// <summary>
    /// 音乐背景图
    /// </summary>
    [ObservableProperty]
    private Bitmap? _musicImage;

    /// <summary>
    /// 是否有音乐背景图
    /// </summary>
    [ObservableProperty]
    private bool _haveMusicImage;

    /// <summary>
    /// 是否循环播放
    /// </summary>
    private bool _musicLoop;

    partial void OnMusicVolumeChanged(int value)
    {
        BaseBinding.SetVolume(value);
    }

    /// <summary>
    /// 音乐暂停
    /// </summary>
    [RelayCommand]
    public void MusicPause()
    {
        if (_isplay)
        {
            BaseBinding.MusicPause();
            AudioIcon = ImageManager.MusicIcons[0];
            Model.Title = "ColorMC";
        }
        else
        {
            if (BaseBinding.GetPlayState() == PlayState.Stop)
            {
                LoadMusic();
            }
            else
            {
                BaseBinding.MusicPlay();
            }
            AudioIcon = ImageManager.MusicIcons[1];
            Model.Title = "ColorMC " + App.Lang("MainWindow.Info33");
        }

        _isplay = !_isplay;
    }

    /// <summary>
    /// 音乐信息更新
    /// </summary>
    private void MusicLoopStart()
    {
        _musicLoop = true;
        DispatcherTimer.Run(() =>
        {
            var temp = BaseBinding.GetMusicNow();
            MusicNow = temp.Item2;
            if (temp.Item1 == PlayState.Stop)
            {
                _musicLoop = false;
                AudioIcon = ImageManager.MusicIcons[0];
                Model.Title = "ColorMC";
                _isplay = false;
            }
            return _musicLoop;
        }, TimeSpan.FromMilliseconds(500));
    }

    /// <summary>
    /// 加载音乐
    /// </summary>
    private async void LoadMusic()
    {
        var config = GuiConfigUtils.Config.ServerCustom;
        if (config?.PlayMusic == true && !string.IsNullOrWhiteSpace(config.Music))
        {
            Model.Title = "ColorMC " + App.Lang("MainWindow.Info33");
            MusicDisplay = true;

            MusicLoopStart();

            var res = await BaseBinding.MusicStart(config.Music, config.MusicLoop, config.SlowVolume, config.Volume);
            if (!res.Res)
            {
                if (!string.IsNullOrWhiteSpace(res.Message))
                {
                    Model.Show(res.Message);
                }
            }
            else
            {
                SetMusicInfo(res.Message, res.MusicInfo);
            }
        }
        else
        {
            MusicDisplay = false;
        }
    }

    /// <summary>
    /// 设置音乐信息
    /// </summary>
    /// <param name="name"></param>
    /// <param name="info"></param>
    private void SetMusicInfo(string? name, MusicInfoObj? info)
    {
        if (info == null)
        {
            MusicName = name!;
        }
        else
        {
            MusicName = info.Title + "\n" + info.Auther + "\n" + info.Album;
            var temp = MusicImage;
            if (info.Image != null)
            {
                HaveMusicImage = true;
                using var stream = new MemoryStream(info.Image);
                MusicImage = new(stream);
            }
            else
            {
                HaveMusicImage = false;
                MusicImage = null;
            }
            temp?.Dispose();
        }
    }
}
