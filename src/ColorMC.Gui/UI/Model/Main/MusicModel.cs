using System.IO;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel
{
    private static readonly string[] _icons =
    [
        "/Resource/Icon/play.svg",
        "/Resource/Icon/pause.svg"
    ];

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
            AudioIcon = _icons[0];
            Model.Title = "ColorMC";
        }
        else
        {
            BaseBinding.MusicPlay();
            AudioIcon = _icons[1];
            Model.Title = "ColorMC " + App.Lang("MainWindow.Info33");
        }

        _isplay = !_isplay;
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
