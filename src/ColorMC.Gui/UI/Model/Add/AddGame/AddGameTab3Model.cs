using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add.AddGame;

public partial class AddGameModel : TopModel
{
    private FilesPage _fileModel;

    [ObservableProperty]
    private string _local;

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _files;

    [RelayCommand]
    public async Task Refash()
    {
        if (Directory.Exists(Local))
        {
            var res = await Model.ShowWait(string.Format(App.GetLanguage("AddGameWindow.Tab3.Info3"), Local));
            if (!res)
            {
                return;
            }
            Model.Progress(App.GetLanguage("AddGameWindow.Tab3.Info2"));
            await Task.Run(() =>
            {
                _fileModel = new FilesPage(Local, true, new()
                { "assets", "libraries", "versions", "launcher_profiles.json" });
            });
            Model.ProgressClose();
            Files = _fileModel.Source;
        }
        else
        {
            Model.Show(string.Format(App.GetLanguage("AddGameWindow.Tab1.Error2"), Local));
        }
    }

    [RelayCommand]
    public async Task Add()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Model.Show(string.Format(App.GetLanguage("AddGameWindow.Tab1.Error2"), Local));
            return;
        }

        Model.Progress(App.GetLanguage("AddGameWindow.Tab3.Info1"));
        var res = await GameBinding.AddGame(Name, Local, _fileModel.GetUnSelectItems(), Group);
        Model.ProgressClose();

        if (!res)
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab3.Error1"));
            return;
        }

        var model = (App.MainWindow?.DataContext as MainModel);
        model?.Model.Notify(App.GetLanguage("AddGameWindow.Tab2.Info5"));
        App.MainWindow?.LoadMain();
        WindowClose();
    }

    [RelayCommand]
    public async Task SelectLocal()
    {
        var res = await PathBinding.SelectPath(FileType.Game);
        if (string.IsNullOrWhiteSpace(res))
        {
            return;
        }

        if (Directory.Exists(res))
        {
            Local = res;
        }
        else
        {
            Model.Show(string.Format(App.GetLanguage("AddGameWindow.Tab3.Error2"), res));
        }
    }
}
