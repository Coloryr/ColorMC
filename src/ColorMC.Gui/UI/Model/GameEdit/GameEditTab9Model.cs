using System.Collections.ObjectModel;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel
{
    /// <summary>
    /// 截图列表
    /// </summary>
    public ObservableCollection<ScreenshotModel> ScreenshotList { get; init; } = [];

    /// <summary>
    /// 选中的截图
    /// </summary>
    private ScreenshotModel _lastScreenshot;

    /// <summary>
    /// 是否没有截图
    /// </summary>
    [ObservableProperty]
    private bool _screenshotEmptyDisplay;

    /// <summary>
    /// 加载截图列表
    /// </summary>
    public void LoadScreenshot()
    {
        var dialog = Window.ShowProgress(LanguageUtils.Get("GameEditWindow.Tab9.Text6"));
        ScreenshotList.Clear();

        var res = _obj.GetScreenshots();
        Window.CloseDialog(dialog);
        foreach (var item in res)
        {
            ScreenshotList.Add(new(this, item));
        }
        ScreenshotEmptyDisplay = ScreenshotList.Count == 0;

        Window.Notify(LanguageUtils.Get("GameEditWindow.Tab9.Text7"));
    }

    /// <summary>
    /// 打开截图目录
    /// </summary>
    private void OpenScreenshot()
    {
        PathBinding.OpenPath(_obj, PathType.ScreenshotsPath);
    }

    /// <summary>
    /// 清理所有截图
    /// </summary>
    private async void ClearScreenshot()
    {
        var res = await Window.ShowChoice(
            string.Format(LanguageUtils.Get("GameEditWindow.Tab9.Text5"), _obj.Name));
        if (!res)
        {
            return;
        }

        _obj.ClearScreenshots();
        Window.Notify(LanguageUtils.Get("Text.DeleteDone"));
        LoadScreenshot();
    }

    /// <summary>
    /// 删除截图
    /// </summary>
    /// <param name="obj"></param>
    public async void DeleteScreenshot(ScreenshotModel obj)
    {
        var res = await Window.ShowChoice(
            string.Format(LanguageUtils.Get("GameEditWindow.Tab9.Text4"), obj.Screenshot));
        if (!res)
        {
            return;
        }

        await obj.Obj.DeleteAsync();
        Window.Notify(LanguageUtils.Get("Text.DeleteDone"));
        LoadScreenshot();
    }

    /// <summary>
    /// 选中截图
    /// </summary>
    /// <param name="item"></param>
    public void SetSelectScreenshot(ScreenshotModel item)
    {
        if (_lastScreenshot != null)
        {
            _lastScreenshot.IsSelect = false;
        }
        _lastScreenshot = item;
        _lastScreenshot.IsSelect = true;
    }
}
