using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls.GameEdit.Items;
using ColorMC.Gui.UI.Windows;
using CommunityToolkit.Mvvm.Input;
using System;
using ColorMC.Core.LaunchPath;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab9Model : GameEditTabModel, IScreenshotFuntion
{
    public ObservableCollection<ScreenshotModel> ScreenshotList { get; init; } = new();

    private ScreenshotModel? Last;

    public GameEditTab9Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public async void Load()
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
        BaseBinding.OpPath(Obj.GetScreenshotsPath());
    }

    [RelayCommand]
    public async void Clear()
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
        Load();
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
