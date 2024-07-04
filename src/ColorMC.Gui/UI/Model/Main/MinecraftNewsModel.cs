using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel
{
    public ObservableCollection<NewsItemModel> News { get; init; } = [];

    [ObservableProperty]
    private bool _isLoadNews;

    public async void LoadNews()
    {
        IsLoadNews = true;
        News.Clear();
        var data = await WebBinding.LoadNews();
        if (data == null)
        {
            IsLoadNews = false;
            Model.Show("News加载失败");
            return;
        }

        foreach (var item in data.ArticleGrid)
        {
            News.Add(new(item));
        }

        IsLoadNews = false;
    }
}
