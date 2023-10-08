using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Guide;

public partial class GuideModel : TopModel
{
    public List<MenuObj> TabItems { get; init; } = new()
    {
        new() { Text = App.GetLanguage("GuideWindow.Tabs.Text1") },
        new() { Text = App.GetLanguage("GuideWindow.Tabs.Text2") },
        new() { Text = App.GetLanguage("GuideWindow.Tabs.Text3") },
        new() { Text = App.GetLanguage("GuideWindow.Tabs.Text4") },
        new() { Text = App.GetLanguage("GuideWindow.Tabs.Text5") },
    };

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private int _nowView;

    [ObservableProperty]
    private bool _canLast;
    [ObservableProperty]
    private bool _canNext;

    public GuideModel(BaseModel model) : base(model)
    {
        _title = TabItems[0].Text;
        Update();
    }

    partial void OnNowViewChanged(int value)
    {
        Title = TabItems[NowView].Text;
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
