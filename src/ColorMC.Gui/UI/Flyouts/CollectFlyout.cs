using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 收藏页面
/// 收藏项目右键
/// </summary>
public static class CollectFlyout
{
    public static void Show(Control con, CollectItemModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(LanguageUtils.Get("CollectFlyout.Text1"), model.Add.HaveSelect(), model.Add.Install),
            new FlyoutMenuModel(LanguageUtils.Get("NetFrpWindow.Tab1.Text14"), true, ()=>
            {
                BaseBinding.OpenUrl(model.Obj.Url);
            }),
            new FlyoutMenuModel(LanguageUtils.Get("CollectFlyout.Text2"), model.Add.HaveSelect(), model.Add.DeleteSelect),
            new FlyoutMenuModel(LanguageUtils.Get("CollectFlyout.Text3"), model.Add.HaveGroup(), model.Add.GroupSelect),
        ]).Show(con);
    }
}
