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

public partial class MainModel
{
    private bool _isplay = true;

    [ObservableProperty]
    private int _musicVolume;

    [ObservableProperty]
    private string _musicName;
    [ObservableProperty]
    private string _musicNow;

    [ObservableProperty]
    private Bitmap? _musicImage;

    [ObservableProperty]
    private bool _haveMusicImage;

    private bool _musicLoop;

    partial void OnMusicVolumeChanged(int value)
    {
        BaseBinding.SetVolume(value);
    }

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

    private void SetMusicInfo(string? name, MusicInfo? info)
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
