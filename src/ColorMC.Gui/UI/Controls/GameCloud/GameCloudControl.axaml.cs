using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameCloud;

namespace ColorMC.Gui.UI.Controls.GameCloud;

public partial class GameCloudControl : MenuControl
{
    private Tab1Control _tab1;
    private Tab2Control _tab2;
    private Tab3Control _tab3;

    public GameSettingObj Obj { get; }

    public GameCloudControl(GameSettingObj obj)
    {
        Obj = obj;

        Title = string.Format(App.Lang("GameCloudWindow.Title"), obj.Name);
        UseName = (ToString() ?? "GameCloudControl") + ":" + obj.UUID;
    }

    public override async void Opened()
    {
        Window.SetTitle(Title);

        if (DataContext is GameCloudModel model && await model.Init())
        {
            model.NowView = 0;
        }
    }

    public override void Closed()
    {
        WindowManager.GameCloudWindows.Remove((DataContext as GameCloudModel)!.Obj.UUID);
    }

    public override TopModel GenModel(BaseModel model)
    {
        return new GameCloudModel(model, Obj);
    }

    public override Bitmap GetIcon()
    {
        var icon = ImageManager.GetGameIcon(Obj);
        return icon ?? ImageManager.GameIcon;
    }

    protected override Control ViewChange(int old, int index)
    {
        return index switch
        {
            0 => _tab1 ??= new(),
            1 => _tab2 ??= new(),
            2 => _tab3 ??= new(),
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
