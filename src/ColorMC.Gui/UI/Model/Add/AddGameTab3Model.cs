using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameTab3Model : AddGameControlModel
{
    private FilesPageViewModel _model;

    [ObservableProperty]
    private string _local;
    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _files;

    public AddGameTab3Model(IUserControl con) : base(con)
    {

    }

    partial void OnLocalChanged(string value)
    {
        if (Directory.Exists(value))
        {
            _model = new FilesPageViewModel(value, new()
            { "assets", "libraries", "versions", "launcher_profiles.json" });
            Files = _model.Source;
        }
    }

    [RelayCommand]
    public async Task Add()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Show(App.GetLanguage("AddGameWindow.Tab1.Error2"));
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
        var res = await BaseBinding.OpPath(Window, FileType.Game);
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
}
