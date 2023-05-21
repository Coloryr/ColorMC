using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab6Model : GameEditTabModel
{
    private FilesPageViewModel FilesPageViewModel;

    [ObservableProperty]
    private bool isGameRun;

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> source;

    public GameEditTab6Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public async void Export()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab6.Info2"));
        var file = await BaseBinding.SaveFile(window, FileType.Game, new object[]
            { Obj, FilesPageViewModel.GetUnSelectItems(), PackType.ColorMC });
        window.ProgressInfo.Close();
        if (file == null)
            return;

        if (file == false)
        {
            window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab6.Error1"));
        }
        else
        {
            window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab6.Info3"));
        }
    }

    public void Load()
    {
        FilesPageViewModel = new FilesPageViewModel(Obj.GetBasePath());
        Source = FilesPageViewModel.Source;
        IsGameRun = BaseBinding.IsGameRun(Obj);
    }
}
