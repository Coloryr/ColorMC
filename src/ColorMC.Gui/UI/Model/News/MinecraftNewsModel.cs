using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.News;

public partial class MinecraftNewsModel(BaseModel model) : TopModel(model)
{
    public ObservableCollection<NewsItemModel> News { get; init; } = [];

    private int _newsPage = 0;

    private string _use = "MinecraftNewsModel";

    [RelayCommand]
    public async Task ReloadNews()
    {
        News.Clear();
        _newsPage = 0;
        Model.Progress(App.Lang("UserWindow.Info1"));
        var data = await WebBinding.LoadNews(_newsPage);
        Model.ProgressClose();
        if (data == null)
        {
            Model.Notify(App.Lang("MainWindow.Error9"));
            return;
        }

        foreach (var item in data.ArticleGrid)
        {
            News.Add(new(item));
        }
        Model.ProgressClose();
    }

    [RelayCommand]
    public async Task NewsNextPage()
    {
        Model.Progress(App.Lang("UserWindow.Info1"));
        var data = await WebBinding.LoadNews(++_newsPage);
        Model.ProgressClose();
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

    public async void LoadNews()
    {
        Model.SetChoiseContent(_use, App.Lang("Button.Refash"));
        Model.SetChoiseCall(_use, Reload);
        await ReloadNews();
    }

    public override void Close()
    {
        News.Clear();
    }

    private async void Reload()
    {
        await ReloadNews();
    }
}
