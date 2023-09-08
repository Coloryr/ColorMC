using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : GameModel
{
    public List<MenuObj> TabItems { get; init; } = new()
    {
        new() { Icon = "/Resource/Icon/GameEdit/item1.svg",
            Text = App.GetLanguage("GameEditWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/GameEdit/item2.svg",
            Text = App.GetLanguage("GameEditWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/GameEdit/item3.svg",
            Text = App.GetLanguage("GameEditWindow.Tabs.Text4") },
        new() { Icon = "/Resource/Icon/GameEdit/item4.svg",
            Text = App.GetLanguage("GameEditWindow.Tabs.Text5") },
        new() { Icon = "/Resource/Icon/GameEdit/item5.svg",
            Text = App.GetLanguage("GameEditWindow.Tabs.Text6") },
        new() { Icon = "/Resource/Icon/GameEdit/item6.svg",
            Text = App.GetLanguage("GameEditWindow.Tabs.Text7") },
        new() { Icon = "/Resource/Icon/GameEdit/item7.svg",
            Text = App.GetLanguage("GameEditWindow.Tabs.Text10") },
        new() { Icon = "/Resource/Icon/GameEdit/item8.svg",
            Text = App.GetLanguage("GameEditWindow.Tabs.Text11") },
        new() { Icon = "/Resource/Icon/GameEdit/item9.svg",
            Text = App.GetLanguage("GameEditWindow.Tabs.Text12") },
    };

    [ObservableProperty]
    private bool _displayFilter = true;

    [ObservableProperty]
    private int _nowView;

    [ObservableProperty]
    private string _title;

    [RelayCommand]
    public void ShowFilter()
    {
        DisplayFilter = !DisplayFilter;
    }

    public GameEditModel(BaseModel model, GameSettingObj obj) : base(model, obj)
    {
        _title = TabItems[0].Text;
        _titleText = string.Format(App.GetLanguage("GameEditWindow.Tab2.Text13"), Obj.Name);
        GameLoad();
        ConfigLoad();
    }

    partial void OnNowViewChanged(int value)
    {
        CloseSide();

        Title = TabItems[NowView].Text;
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
        _configLoad = true;
        _gameLoad = true;
        GameVersionList.Clear();
        LoaderVersionList.Clear();
        GroupList.Clear();
        JvmList.Clear();
        ModList.Clear();
        _modItems.Clear();
        foreach (var item in WorldList)
        {
            item.Close();
        }
        WorldList.Clear();
        _selectWorld = null;
        foreach (var item in ResourcePackList)
        {
            item.Close();
        }
        ResourcePackList.Clear();
        _lastResource = null;
        foreach (var item in ScreenshotList)
        {
            item.Close();
        }
        ScreenshotList.Clear();
        _lastScreenshot = null;
        ServerList.Clear();
        ShaderpackList.Clear();
        SchematicList.Clear();
    }
}
