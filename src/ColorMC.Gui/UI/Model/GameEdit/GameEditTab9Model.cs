using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab9Model : GameModel
{
    public ObservableCollection<ScreenshotModel> ScreenshotList { get; init; } = new();

    private ScreenshotModel? _last;

    public GameEditTab9Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public void Load()
    {
        Progress(App.GetLanguage("GameEditWindow.Tab9.Info3"));
        ScreenshotList.Clear();

        var res = GameBinding.GetScreenshots(Obj);
        ProgressClose();
        foreach (var item in res)
        {
            ScreenshotList.Add(new(this, item));
        }
    }

    [RelayCommand]
    public void Open()
    {
        PathBinding.OpPath(Obj, PathType.ScreenshotsPath);
    }

    [RelayCommand]
    public async Task Clear()
    {
        var res = await ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info2"), Obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.ClearScreenshots(Obj);
        Notify(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Load();
    }

    public async void Delete(ScreenshotModel obj)
    {
        var res = await ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info1"), obj.Screenshot));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteScreenshot(obj.Screenshot);
        Window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Load();
    }

    public void SetSelect(ScreenshotModel item)
    {
        if (_last != null)
        {
            _last.IsSelect = false;
        }
        _last = item;
        _last.IsSelect = true;
    }

    public override void Close()
    {
        ScreenshotList.Clear();
        _last = null;
    }
}
