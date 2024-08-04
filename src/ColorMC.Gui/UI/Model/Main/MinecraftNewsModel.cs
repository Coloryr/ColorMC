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
        var data = await WebBinding.LoadNews();
        IsLoadNews = false;
        if (data == null)
        {
            IsHaveNews = false;
            Model.Notify("News加载失败");
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
}
