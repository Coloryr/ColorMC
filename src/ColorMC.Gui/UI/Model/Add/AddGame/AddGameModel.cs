using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add.AddGame;

public partial class AddGameModel : TopModel
{
    public ObservableCollection<string> GroupList { get; init; } = new();

    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private string _group;

    public AddGameModel(BaseModel model) : base(model)
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

        GameVersionUpdate();

        ColorMCCore.PackState = PackState;
        ColorMCCore.PackUpdate = PackUpdate;
    }

    [RelayCommand]
    public async Task AddGroup()
    {
        var (Cancel, Text) = await Model.ShowOne(App.GetLanguage("AddGameWindow.Tab1.Info5"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(Text))
        {
            Model.Show(App.GetLanguage("AddGameWindow.Tab1.Error3"));
            return;
        }

        Model.Notify(App.GetLanguage("AddGameWindow.Tab1.Info6"));

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
        Group = Text;
    }

    protected override void Close()
    {
        _load = true;
        GameVersionList.Clear();
        LoaderVersionList.Clear();
        _fileModel = null!;
        Files = null!;
    }

    public void WindowClose()
    {
        OnPropertyChanged("WindowClose");
    }
}
