using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
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
    /// 显示的Minecraft news
    /// </summary>
    [ObservableProperty]
    private string? _displayNews;

    /// <summary>
    /// 是否加载news
    /// </summary>
    [ObservableProperty]
    private bool _isLoadNews;
    /// <summary>
    /// 是否有news
    /// </summary>
    [ObservableProperty]
    private bool _isHaveNews;

    /// <summary>
    /// news图片
    /// </summary>
    [ObservableProperty]
    private Bitmap? _newsImage;

    /// <summary>
    /// 获取news
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task GetNews()
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

        if (data?.Result?.Results?.Count > 0)
        {
            var item = data.Result.Results.First(item => item.IsNews());
            DisplayNews = item.Title;
            var temp1 = NewsImage;
            NewsImage = await GetImage(item.Image);
            temp1?.Dispose();
            IsHaveNews = true;
        }
        else
        {
            IsHaveNews = false;
        }
    }

    /// <summary>
    /// 加载news
    /// </summary>
    private async void LoadNews()
    {
        await GetNews();
    }

    /// <summary>
    /// 获取图片
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static async Task<Bitmap?> GetImage(string url)
    {
        Bitmap? _img = null;
        try
        {
            await Task.Run(() =>
            {
                _img = ImageManager.Load(url, false).Result;
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
