using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 存档右键菜单
/// </summary>
public static class GameEditFlyout8
{
    public static void Show(Control con, IList list, WorldModel model)
    {
        IEnumerable<DataPackModel> mods;
        DataPackModel obj = null!;
        bool single = false;
        mods = list.Cast<DataPackModel>();
        if (mods.Count() == 1)
        {
            single = true;
            obj = mods.ToList()[0];
        }

        var run = GameManager.IsGameRun(model.World.Game);

        new FlyoutsControl(
        [
            new FlyoutMenuModel(LanguageUtils.Get("GameEditWindow.Flyouts.Text1"),
                !run, ()=>
                {
                    if (single)
                    {
                        model.DisE(obj);
                    }
                    else
                    {
                        model.DisE(mods);
                    }
                }),
            new FlyoutMenuModel(LanguageUtils.Get("Button.Delete"), !run, ()=>
            {
                if (single)
                {
                    model.Delete(obj);
                }
                else
                {
                    model.Delete(mods);
                }
            })
        ]).Show(con);
    }
}