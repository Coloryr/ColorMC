using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout3
{
    private readonly ResourcepackDisplayObj Obj;
    private readonly Tab8Control Con;

    public GameEditFlyout3(Tab8Control con, ResourcepackDisplayObj obj)
    {
        Con = con;
        Obj = obj;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts3.Text1"), true, Button2_Click)
        }, con);
    }

    private void Button2_Click()
    {
        Con.Delete(Obj);
    }

    private void Button1_Click()
    {
        BaseBinding.OpFile(Obj.Local);
    }
}
