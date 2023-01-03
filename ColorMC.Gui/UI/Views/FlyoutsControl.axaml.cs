using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ColorMC.Core.Objs;

namespace ColorMC.Gui.UI.Views;

public partial class FlyoutsControl : UserControl
{
    private GameSettingObj Obj;
    public FlyoutsControl()
    {
        InitializeComponent();
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }
}

public class MyFlyout : FlyoutBase
{
    private GameSettingObj Obj;
    public MyFlyout(GameSettingObj obj)
    {
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new FlyoutsControl();
        control.SetGame(Obj);
        return control;
    }
}
