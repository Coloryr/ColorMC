using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Gui.Objs;
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

    public List<MenuObj> TabItems { get; init; } = new()
    {
        new() { Icon = "/Resource/Icon/AddMenu/item1.svg",
            Text = App.GetLanguage("AddGameWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/AddMenu/item2.svg",
            Text = App.GetLanguage("AddGameWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/AddMenu/item3.svg",
            Text = App.GetLanguage("AddGameWindow.Tabs.Text3") },
    };

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private string _group;

    [ObservableProperty]
    private int _nowView;

    private bool _side;

    public AddGameModel(BaseModel model) : base(model)
    {
        Title = TabItems[0].Text;

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

        GameVersionUpdate();

        ColorMCCore.PackState = PackState;
        ColorMCCore.PackUpdate = PackUpdate;
    }

    partial void OnNowViewChanged(int value)
    {
        CloseSide();

        Title = App.GetLanguage($"AddGameWindow.Tabs.Text{NowView + 1}");
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

    [RelayCommand]
    public void OpenSide()
    {
        _side = true;
        OnPropertyChanged("SideOpen");
    }

    [RelayCommand]
    public void CloseSide()
    {
        _side = false;
        OnPropertyChanged("SideClose");
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
