using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    [RelayCommand]
    public async Task LoadNews()
    {
        IsLoadNews = true;
        News.Clear();
        DisplayNews = null;
        var data = await WebBinding.LoadNews();
        if (data == null)
        {
            IsLoadNews = false;
            IsHaveNews = false;
            Model.Show("News加载失败");
            return;
        }

        foreach (var item in data.ArticleGrid)
        {
            News.Add(new(item));
        }

        if (News.Count > 0)
        {
            IsHaveNews = true;
            DisplayNews = News[0].Title;
        }
        else
        {
            IsHaveNews = false;
        }

        IsLoadNews = false;
    }
}
