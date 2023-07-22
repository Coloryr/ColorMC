using Avalonia.Controls;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab6Model : GameEditTabModel
{
    private FilesPageViewModel _model;

    [ObservableProperty]
    private bool _isGameRun;

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _source;

    public GameEditTab6Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public async Task Export()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab6.Info2"));
        var file = await BaseBinding.SaveFile(window, FileType.Game, new object[]
            { Obj, _model.GetUnSelectItems(), PackType.ColorMC });
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
        _model = new FilesPageViewModel(Obj.GetBasePath());
        Source = _model.Source;
        IsGameRun = BaseBinding.IsGameRun(Obj);
    }
}
