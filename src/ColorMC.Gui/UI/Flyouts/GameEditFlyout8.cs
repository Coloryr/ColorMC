using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout8
{
    private readonly IEnumerable<DataPackModel> _list;
    private readonly DataPackModel _obj;
    private readonly WorldModel _model;
    private readonly bool _single;
    public GameEditFlyout8(Control con, IList obj, WorldModel model)
    {
        _model = model;
        _list = obj.Cast<DataPackModel>();
        if (_list.Count() == 1)
        {
            _single = true;
            _obj = _list.ToList()[0];
        }

        var fy = new FlyoutsControl(new()
        {
            (App.Lang("GameEditWindow.Flyouts1.Text1"), !BaseBinding.IsGameRun(model.World.Game), Button1_Click),
            (App.Lang("Button.Delete"), !BaseBinding.IsGameRun(model.World.Game), Button2_Click)
        }, con);
    }

    private void Button1_Click()
    {
        if (_single)
        {
            _model.DisE(_obj);
        }
        else
        {
            _model.DisE(_list);
        }
    }

    private void Button2_Click()
    {
        if (_single)
        {
            _model.Delete(_obj);
        }
        else
        {
            _model.Delete(_list);
        }
    }
}