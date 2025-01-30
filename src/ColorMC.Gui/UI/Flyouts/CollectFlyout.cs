using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class CollectFlyout
{
    public CollectFlyout(Control con, CollectItemModel model)
    {
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("CollectFlyout.Text1"), model.Add.HaveSelect(), model.Add.Install),
            new FlyoutMenuObj(App.Lang("NetFrpWindow.Tab1.Text14"), true, ()=>
            {
                BaseBinding.OpenUrl(model.Obj.Url);
            }),
            new FlyoutMenuObj(App.Lang("CollectFlyout.Text2"), model.Add.HaveSelect(), model.Add.DeleteSelect),
            new FlyoutMenuObj(App.Lang("CollectFlyout.Text3"), model.Add.HaveGroup(), model.Add.GroupSelect),
        ], con);
    }
}
