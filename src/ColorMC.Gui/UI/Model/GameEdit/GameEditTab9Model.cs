using System.Collections.ObjectModel;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using ColorMC.Core.Game;

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
        Model.Progress(App.Lang("GameEditWindow.Tab9.Info3"));
        ScreenshotList.Clear();

        var res = _obj.GetScreenshots();
        Model.ProgressClose();
        foreach (var item in res)
        {
            ScreenshotList.Add(new(this, item));
        }
        ScreenshotEmptyDisplay = ScreenshotList.Count == 0;

        Model.Notify(App.Lang("GameEditWindow.Tab9.Info4"));
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
        var res = await Model.ShowAsync(
            string.Format(App.Lang("GameEditWindow.Tab9.Info2"), _obj.Name));
        if (!res)
        {
            return;
        }

        _obj.ClearScreenshots();
        Model.Notify(App.Lang("GameEditWindow.Tab4.Info3"));
        LoadScreenshot();
    }

    /// <summary>
    /// 删除截图
    /// </summary>
    /// <param name="obj"></param>
    public async void DeleteScreenshot(ScreenshotModel obj)
    {
        var res = await Model.ShowAsync(
            string.Format(App.Lang("GameEditWindow.Tab9.Info1"), obj.Screenshot));
        if (!res)
        {
            return;
        }

        await obj.Obj.DeleteAsync();
        Model.Notify(App.Lang("GameEditWindow.Tab4.Info3"));
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
