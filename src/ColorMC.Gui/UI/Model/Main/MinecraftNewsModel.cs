using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel
{
    [ObservableProperty]
    private string? _displayNews;

    [ObservableProperty]
    private bool _isLoadNews;
    [ObservableProperty]
    private bool _isHaveNews;

    [ObservableProperty]
    private Bitmap? _newsImage;

    [RelayCommand]
    public async Task LoadNews()
    {
        IsHaveNews = true;
        IsLoadNews = true;
        DisplayNews = null;
        var temp = NewsImage;
        NewsImage = null;
        temp?.Dispose();
        var data = await WebBinding.LoadNews(0);
        IsLoadNews = false;
        if (data == null)
        {
            IsHaveNews = false;
            Model.Notify(App.Lang("MainWindow.Error9"));
            return;
        }

        if (data.ArticleCount > 0)
        {
            DisplayNews = data.ArticleGrid[0].DefaultTile.Title.ToUpper();
            var temp1 = NewsImage;
            NewsImage = await GetImage(data.ArticleGrid[0].DefaultTile.Image.ImageURL);
            temp1?.Dispose();
            IsHaveNews = true;
        }
        else
        {
            IsHaveNews = false;
        }
    }

    private async Task<Bitmap?> GetImage(string url)
    {
        Bitmap? _img = null;
        try
        {
            await Task.Run(() =>
            {
                _img = ImageManager.Load("https://www.minecraft.net" + url, false).Result;
            });
            return _img;
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("AddModPackWindow.Error5"), e);
        }

        return null;
    }
}
