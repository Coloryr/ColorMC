using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : GameModel
{
    public ObservableCollection<ScreenshotModel> ScreenshotList { get; init; } = new();

    private ScreenshotModel _lastScreenshot;

    [RelayCommand]
    public void LoadScreenshot()
    {
        Model.Progress(App.GetLanguage("GameEditWindow.Tab9.Info3"));
        ScreenshotList.Clear();

        var res = GameBinding.GetScreenshots(Obj);
        Model.ProgressClose();
        foreach (var item in res)
        {
            ScreenshotList.Add(new(this, item));
        }
    }

    [RelayCommand]
    public void OpenScreenshot()
    {
        PathBinding.OpPath(Obj, PathType.ScreenshotsPath);
    }

    [RelayCommand]
    public async Task ClearScreenshot()
    {
        var res = await Model.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info2"), Obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.ClearScreenshots(Obj);
        Model.Notify(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        LoadScreenshot();
    }

    public async void DeleteScreenshot(ScreenshotModel obj)
    {
        var res = await Model.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info1"), obj.Screenshot));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteScreenshot(obj.Screenshot);
        Model.Notify(App.GetLanguage("GameEditWindow.Tab4.Info3"));
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
