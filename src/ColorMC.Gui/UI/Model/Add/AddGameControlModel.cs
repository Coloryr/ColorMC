using AvaloniaEdit.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.Add;

public abstract partial class AddGameTabModel : ObservableObject
{
    protected IUserControl Con;
    public ObservableCollection<string> GroupList { get; init; } = new();

    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string group;

    public AddGameTabModel(IUserControl con)
    {
        Con = con;

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    [RelayCommand]
    public async void AddGroup()
    {
        var window = Con.Window;
        await window.InputInfo.ShowOne(App.GetLanguage("AddGameWindow.Tab1.Info5"), false);
        if (window.InputInfo.Cancel)
        {
            return;
        }

        var res = window.InputInfo.Read().Item1;
        if (string.IsNullOrWhiteSpace(res))
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(res))
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error3"));
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info6"));

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }
}
