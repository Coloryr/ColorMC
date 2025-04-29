using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.News;

/// <summary>
/// Minecraft news窗口
/// </summary>
/// <param name="model"></param>
public partial class MinecraftNewsModel : TopModel
{
    /// <summary>
    /// 新闻列表
    /// </summary>
    public ObservableCollection<NewsItemModel> News { get; init; } = [];

    /// <summary>
    /// 当前页数
    /// </summary>
    private int _newsPage = 0;

    private readonly string _use;

    public MinecraftNewsModel(BaseModel model) : base(model)
    {
        _use = ToString() ?? "MinecraftNewsModel";
    }

    /// <summary>
    /// 加载新闻列表
    /// </summary>
    /// <returns></returns>
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

        foreach (var item in data.Result.Results.Where(item => item.IsNews()))
        {
            News.Add(new(item));
        }
        Model.ProgressClose();
    }

    /// <summary>
    /// 获取下一页
    /// </summary>
    /// <returns></returns>
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

        foreach (var item in data.Result.Results.Where(item => item.Type != "Game"))
        {
            News.Add(new(item));
        }
    }

    /// <summary>
    /// 设置标题按钮
    /// </summary>
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

    /// <summary>
    /// 重新加载新闻列表
    /// </summary>
    private async void Reload()
    {
        await ReloadNews();
    }
}
