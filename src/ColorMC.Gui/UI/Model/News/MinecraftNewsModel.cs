using System.Collections.ObjectModel;
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
public partial class MinecraftNewsModel : ControlModel
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

    public MinecraftNewsModel(WindowModel model) : base(model)
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
        var dialog = Window.ShowProgress(LanguageUtils.Get("Text.Loading"));
        var data = await WebBinding.LoadNewsAsync(_newsPage);
        Window.CloseDialog(dialog);
        if (data == null)
        {
            Window.Notify(LanguageUtils.Get("MainWindow.Text87"));
            return;
        }

        foreach (var item in data.ArticleGrid)
        {
            News.Add(new(item));
        }
        Window.CloseDialog(dialog);
    }

    /// <summary>
    /// 获取下一页
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task NewsNextPage()
    {
        var dialog = Window.ShowProgress(LanguageUtils.Get("Text.Loading"));
        var data = await WebBinding.LoadNewsAsync(++_newsPage);
        Window.CloseDialog(dialog);
        if (data == null)
        {
            Window.Notify(LanguageUtils.Get("MainWindow.Text87"));
            return;
        }

        foreach (var item in data.ArticleGrid)
        {
            News.Add(new(item));
        }
    }

    /// <summary>
    /// 设置标题按钮
    /// </summary>
    public async void LoadNews()
    {
        Window.SetChoiseContent(_use, LanguageUtils.Get("Button.Refash"));
        Window.SetChoiseCall(_use, Reload);
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
