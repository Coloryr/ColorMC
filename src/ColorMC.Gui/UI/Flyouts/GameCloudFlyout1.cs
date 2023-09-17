using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Flyouts;

public class GameCloudFlyout1
{
    private readonly WorldCloudModel _model;

    public GameCloudFlyout1(Control con, WorldCloudModel model)
    {
        _model = model;

        _ = new FlyoutsControl(new()
        {
            (App.GetLanguage("Button.OpFile"), model.HaveLocal, Button1_Click),
            (App.GetLanguage("GameCloudWindow.Flyouts1.Text1"), model.HaveLocal, Button2_Click),
            (App.GetLanguage("GameCloudWindow.Flyouts1.Text2"), model.HaveCloud, Button3_Click),
            (App.GetLanguage("GameCloudWindow.Flyouts1.Text3"), model.HaveCloud, Button4_Click),
        }, con);
    }

    private void Button4_Click()
    {
        _model.DeleteCloud();
    }

    private void Button3_Click()
    {
        _model.Download();
    }

    private void Button2_Click()
    {
        _model.Upload();
    }

    private void Button1_Click()
    {
        PathBinding.OpPath(_model.World);
    }

}
