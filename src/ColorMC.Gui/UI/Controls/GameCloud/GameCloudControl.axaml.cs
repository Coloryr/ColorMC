using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameCloud;
using System.ComponentModel;
using System.IO;

namespace ColorMC.Gui.UI.Controls.GameCloud;

public partial class GameCloudControl : MenuControl
{
    private Tab1Control _tab1;
    private Tab2Control _tab2;
    private Tab3Control _tab3;

    private Bitmap _icon;
    public override Bitmap GetIcon() => _icon;

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

        var icon = Obj.GetIconFile();
        if (File.Exists(icon))
        {
            _icon = new(icon);
            Window.SetIcon(_icon);
        }

        if (DataContext is GameCloudModel model && await model.Init())
        {
            model.NowView = 0;
        }
    }

    public override void Closed()
    {
        _icon?.Dispose();

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
