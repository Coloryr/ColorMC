using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameCloud;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.GameCloud;

public partial class GameCloudControl : MenuControl
{
    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();

    public GameSettingObj Obj { get; }

    public override string Title =>
        string.Format(App.Lang("GameCloudWindow.Title"), Obj.Name);

    public override string UseName { get; }

    public GameCloudControl(GameSettingObj obj)
    {
        UseName = (ToString() ?? "GameCloudControl") + ":" + obj.UUID;

        Obj = obj;
    }

    public override async void Opened()
    {
        Window.SetTitle(Title);

        Content1.Child = _tab1;
        await (DataContext as GameCloudModel)!.Init();
    }

    public override void Closed()
    {
        App.GameCloudWindows.Remove((DataContext as GameCloudModel)!.Obj.UUID);
    }

    protected override MenuModel SetModel(BaseModel model)
    {
        return new GameCloudModel(model, Obj);
    }

    protected override Control ViewChange(bool iswhell, int old, int index)
    {
        return index switch
        {
            0 => _tab1,
            1 => _tab2,
            2 => _tab3,
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    public void GoWorld()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var model = (DataContext as GameCloudModel)!;
            model.NowView = 2;
        });
    }
}
