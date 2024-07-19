using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout8
{
    public GameEditFlyout8(Control con, IList list, WorldModel model)
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

        _ = new FlyoutsControl(
        [
            (App.Lang("GameEditWindow.Flyouts.Text1"),
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
            (App.Lang("Button.Delete"), !run, ()=>
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
        ], con);
    }
}