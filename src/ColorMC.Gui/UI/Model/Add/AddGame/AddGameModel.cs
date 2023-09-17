using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
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

    public AddGameModel(BaseModel model) : base(model)
    {
        _title = TabItems[0].Text;

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

        GameVersionUpdate();

        ColorMCCore.PackState = PackState;
        ColorMCCore.PackUpdate = PackUpdate;

        CloudEnable = GameCloudUtils.Connect;
    }

    partial void OnNowViewChanged(int value)
    {
        CloseSide();

        Title = TabItems[NowView].Text;
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
        OnPropertyChanged("SideOpen");
    }

    [RelayCommand]
    public void CloseSide()
    {
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
