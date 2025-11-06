using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.Utils;

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
            new FlyoutMenuModel(LanguageUtils.Get("Button.Delete"), true, ()=>
            {
                model.DeleteFile(obj);
            }),
        ]).Show(con);
    }
}
