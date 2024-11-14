using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel
{
    public ObservableCollection<NewsItemModel> News { get; init; } = [];

    [ObservableProperty]
    private string? _displayNews;

    [ObservableProperty]
    private bool _isLoadNews;
    [ObservableProperty]
    private bool _isHaveNews;

    [ObservableProperty]
    private Bitmap? _newsImage;

    private int _newsPage = 0;

    [RelayCommand]
    public async Task ReloadNews()
    {
        Model.Progress(App.Lang("UserWindow.Info1"));
        _newsPage = 0;
        await LoadNews();
        Model.ProgressClose();
    }

    [RelayCommand]
    public async Task LoadNews()
    {
        IsHaveNews = true;
        IsLoadNews = true;
        News.Clear();
        DisplayNews = null;
        var temp = NewsImage;
        NewsImage = null;
        temp?.Dispose();
        var data = await WebBinding.LoadNews(_newsPage++);
        IsLoadNews = false;
        if (data == null)
        {
            IsHaveNews = false;
            Model.Notify(App.Lang("MainWindow.Error9"));
            return;
        }

        foreach (var item in data.ArticleGrid)
        {
            News.Add(new(item));
        }

        if (News.Count > 0)
        {
            DisplayNews = News[0].Title;
            NewsImage = await News[0].Image;
            IsHaveNews = true;
        }
        else
        {
            IsHaveNews = false;
        }
    }

    [RelayCommand]
    public async Task NewsNextPage()
    {
        var data = await WebBinding.LoadNews(_newsPage++);
        if (data == null)
        {
            Model.Notify(App.Lang("MainWindow.Error9"));
            return;
        }

        foreach (var item in data.ArticleGrid)
        {
            News.Add(new(item));
        }
    }
}
