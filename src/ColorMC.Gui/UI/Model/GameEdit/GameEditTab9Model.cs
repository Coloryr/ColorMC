using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab9Model : GameEditTabModel, ILoadFuntion<ScreenshotModel>
{
    public ObservableCollection<ScreenshotModel> ScreenshotList { get; init; } = new();

    private ScreenshotModel? Last;

    public GameEditTab9Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public async Task Load()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab9.Info3"));
        ScreenshotList.Clear();

        var res = await GameBinding.GetScreenshots(Obj);
        window.ProgressInfo.Close();
        foreach (var item in res)
        {
            ScreenshotList.Add(new(Con, this, item));
        }
    }

    [RelayCommand]
    public void Open()
    {
        BaseBinding.OpPath(Obj, PathType.ScreenshotsPath);
    }

    [RelayCommand]
    public async Task Clear()
    {
        var Window = Con.Window;
        var res = await Window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info2"), Obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.ClearScreenshots(Obj);
        Window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        await Load();
    }

    public void SetSelect(ScreenshotModel item)
    {
        if (Last != null)
        {
            Last.IsSelect = false;
        }
        Last = item;
        Last.IsSelect = true;
    }
}
