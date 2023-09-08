using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel : TopModel
{
    public ServerPackObj Obj { get; }

    public List<MenuObj> TabItems { get; init; } = new()
    {
        new() { Icon = "/Resource/Icon/GameExport/item1.svg",
            Text = App.GetLanguage("ServerPackWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/GameExport/item2.svg",
            Text = App.GetLanguage("ServerPackWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/GameExport/item3.svg",
            Text = App.GetLanguage("ServerPackWindow.Tabs.Text3") },
        new() { Icon = "/Resource/Icon/GameExport/item4.svg",
            Text = App.GetLanguage("ServerPackWindow.Tabs.Text4") },
    };

    [ObservableProperty]
    private int _nowView;

    [ObservableProperty]
    private string _title;

    public ServerPackModel(BaseModel model, ServerPackObj obj) : base(model)
    {
        Obj = obj;

        _title = TabItems[0].Text;
    }

    partial void OnNowViewChanged(int value)
    {
        CloseSide();

        Title = TabItems[NowView].Text;
    }

    [RelayCommand]
    public async Task Gen()
    {
        if (string.IsNullOrWhiteSpace(Obj.Url))
        {
            Model.Show(App.GetLanguage("ServerPackWindow.Tab1.Error1"));
            return;
        }

        if (string.IsNullOrWhiteSpace(Obj.Version))
        {
            Model.Show(App.GetLanguage("ServerPackWindow.Tab1.Error2"));
            return;
        }

        var local = await PathBinding.SelectPath(FileType.ServerPack);
        if (local == null)
            return;

        Model.Progress(App.GetLanguage("ServerPackWindow.Tab1.Info1"));
        var res = await GameBinding.GenServerPack(Obj, local);
        Model.ProgressClose();
        if (res)
        {
            Model.Notify(App.GetLanguage("ServerPackWindow.Tab1.Info2"));
        }
        else
        {
            Model.Show(App.GetLanguage("ServerPackWindow.Tab1.Error3"));
        }
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
        ModList.Clear();
        ConfigList.Clear();
        NameList.Clear();
    }
}
