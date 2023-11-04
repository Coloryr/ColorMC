using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : MenuModel
{
    public ObservableCollection<ScreenshotModel> ScreenshotList { get; init; } = new();

    private ScreenshotModel _lastScreenshot;

    [RelayCommand]
    public void LoadScreenshot()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab9.Info3"));
        ScreenshotList.Clear();

        var res = GameBinding.GetScreenshots(_obj);
        Model.ProgressClose();
        foreach (var item in res)
        {
            ScreenshotList.Add(new(this, item));
        }
    }

    [RelayCommand]
    public void OpenScreenshot()
    {
        PathBinding.OpPath(_obj, PathType.ScreenshotsPath);
    }

    [RelayCommand]
    public async Task ClearScreenshot()
    {
        var res = await Model.ShowWait(
            string.Format(App.Lang("GameEditWindow.Tab9.Info2"), _obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.ClearScreenshots(_obj);
        Model.Notify(App.Lang("GameEditWindow.Tab4.Info3"));
        LoadScreenshot();
    }

    public async void DeleteScreenshot(ScreenshotModel obj)
    {
        var res = await Model.ShowWait(
            string.Format(App.Lang("GameEditWindow.Tab9.Info1"), obj.Screenshot));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteScreenshot(obj.Screenshot);
        Model.Notify(App.Lang("GameEditWindow.Tab4.Info3"));
        LoadScreenshot();
    }

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
