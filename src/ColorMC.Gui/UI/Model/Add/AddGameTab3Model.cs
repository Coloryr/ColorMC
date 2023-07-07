using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameTab3Model : AddGameControlModel
{
    private FilesPageViewModel FilesPageViewModel;

    [ObservableProperty]
    private string local;
    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> files;

    public AddGameTab3Model(IUserControl con) : base(con)
    {

    }

    partial void OnLocalChanged(string value)
    {
        if (Directory.Exists(value))
        {
            FilesPageViewModel = new FilesPageViewModel(value, new()
            { "assets", "libraries", "versions", "launcher_profiles.json" });
            Files = FilesPageViewModel.Source;
        }
    }

    [RelayCommand]
    public async void Add()
    {
        var window = Con.Window;
        if (string.IsNullOrWhiteSpace(Name))
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error2"));
            return;
        }

        window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab3.Info1"));
        var res = await GameBinding.AddGame(Name, Local, FilesPageViewModel.GetUnSelectItems(), Group);
        window.ProgressInfo.Close();

        if (!res)
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab3.Error1"));
            return;
        }

        App.MainWindow?.Window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab2.Info5"));
        App.MainWindow?.LoadMain();
        window.Close();
    }

    [RelayCommand]
    public async void SelectLocal()
    {
        var res = await BaseBinding.OpPath(Con.Window, FileType.Game);
        if (string.IsNullOrWhiteSpace(res))
        {
            return;
        }

        Local = res;
    }
}
