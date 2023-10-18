using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model.Guide;

public partial class GuideModel : MenuModel
{
    public override List<MenuObj> TabItems { get; init; } = new()
    {
        new() { Text = App.GetLanguage("GuideWindow.Tabs.Text1") },
        new() { Text = App.GetLanguage("GuideWindow.Tabs.Text2") },
        new() { Text = App.GetLanguage("GuideWindow.Tabs.Text3") },
        new() { Text = App.GetLanguage("GuideWindow.Tabs.Text4") },
        new() { Text = App.GetLanguage("GuideWindow.Tabs.Text5") },
    };

    [ObservableProperty]
    private bool _canLast;
    [ObservableProperty]
    private bool _canNext;

    public GuideModel(BaseModel model) : base(model)
    {
        Update();
    }

    [RelayCommand]
    public void Last(object msg)
    {
        NowView--;
        Update();
    }

    [RelayCommand]
    public void Next(object msg)
    {
        NowView++;
        Update();
    }

    private void Update()
    {
        CanLast = NowView > 0;
        CanNext = NowView < 4;
    }

    protected override void Close()
    {

    }
}
