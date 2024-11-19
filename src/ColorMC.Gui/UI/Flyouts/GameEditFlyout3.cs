using Avalonia.Controls;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout3
{
    private readonly ResourcepackObj _obj;
    private readonly GameEditModel _top;

    public GameEditFlyout3(Control con, ResourcePackModel model)
    {
        _top = model.TopModel;
        _obj = model.Pack;

        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenFileWithExplorer(_obj.Local);
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text12"), true, ()=>
            {
                _top.DeleteResource(_obj);
            })
        ], con);
    }
}
