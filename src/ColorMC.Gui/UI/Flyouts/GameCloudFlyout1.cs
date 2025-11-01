using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 云同步页面
/// 存档右键菜单
/// </summary>
public static class GameCloudFlyout1
{
    public static void Show(Control con, WorldCloudModel model)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuModel(LanguageUtils.Get("Button.OpFile"), model.HaveLocal, () =>
            {
                PathBinding.OpenPath(model.World);
            }),
            new FlyoutMenuModel(LanguageUtils.Get("GameCloudWindow.Flyouts.Text1"),
                model.HaveLocal, model.Upload),
            new FlyoutMenuModel(LanguageUtils.Get("GameCloudWindow.Flyouts.Text2"),
                model.HaveCloud, model.Download),
            new FlyoutMenuModel(LanguageUtils.Get("GameCloudWindow.Flyouts.Text3"),
                model.HaveCloud,model.DeleteCloud),
        ]).Show(con);
    }
}
