using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.ServerPack;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 服务器包
/// 文件右键菜单
/// </summary>
public static class ServerPackFlyout1
{
    public static void Show(Control con, ServerPackModel model, ServerPackConfigModel obj)
    {
        new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.Delete"), true, ()=>
            {
                model.DeleteFile(obj);
            }),
        ]).Show(con);
    }
}
