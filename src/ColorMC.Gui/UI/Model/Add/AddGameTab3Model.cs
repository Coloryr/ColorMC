using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameTab3Model : AddGameControlModel
{
    private FilesPage _model;

    [ObservableProperty]
    private string _local;
    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _files;

    public AddGameTab3Model(IUserControl con) : base(con)
    {

    }

    [RelayCommand]
    public async Task Refash()
    {
        if (Directory.Exists(Local))
        {
            var res = await ShowWait(string.Format(App.GetLanguage("AddGameWindow.Tab3.Info3"), Local));
            if (!res)
            {
                return;
            }
            Progress(App.GetLanguage("AddGameWindow.Tab3.Info2"));
            await Task.Run(() =>
            {
                _model = new FilesPage(Local, true, new()
                { "assets", "libraries", "versions", "launcher_profiles.json" });
            });
            ProgressClose();
            Files = _model.Source;
        }
        else
        {
            Show(string.Format(App.GetLanguage("AddGameWindow.Tab1.Error2"), Local));
        }
    }

    [RelayCommand]
    public async Task Add()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Show(string.Format(App.GetLanguage("AddGameWindow.Tab1.Error2"), Local));
            return;
        }

        Progress(App.GetLanguage("AddGameWindow.Tab3.Info1"));
        var res = await GameBinding.AddGame(Name, Local, _model.GetUnSelectItems(), Group);
        ProgressClose();

        if (!res)
        {
            Show(App.GetLanguage("AddGameWindow.Tab3.Error1"));
            return;
        }

        App.MainWindow?.Window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab2.Info5"));
        App.MainWindow?.LoadMain();
        Window.Close();
    }

    [RelayCommand]
    public async Task SelectLocal()
    {
        var res = await PathBinding.SelectPath(Window, FileType.Game);
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
            Show(string.Format(App.GetLanguage("AddGameWindow.Tab3.Error2"), res));
        }
    }

    public override void Close()
    {

    }
}
