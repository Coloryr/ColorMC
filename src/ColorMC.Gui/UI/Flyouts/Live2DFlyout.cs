using Avalonia.Controls;
using ColorMC.Gui.UI.Controls.Main;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Flyouts;

public class Live2DFlyout
{
    private readonly Live2dRender _live2D;
    private readonly Control _con;
    public Live2DFlyout(Control con, Live2dRender live2d)
    {
        _con = con;
        _live2D = live2d;

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("Live2DFlyout.Text1"), true, Button1_Click),
            (App.GetLanguage("Live2DFlyout.Text2"), true, Button2_Click),
        }, con);
    }

    public void Button1_Click()
    {
        var list = _live2D.GetMotions();
        if (list.Count != 0)
        {
            var list1 = new List<(string, bool, Action)>();
            list.ForEach(item =>
            {
                list1.Add((item, true, () => _live2D.PlayMotion(item)));
            });
            _ = new FlyoutsControl(list1, _con);
        }
    }

    public void Button2_Click()
    {
        var list = _live2D.GetExpressions();
        if (list.Count != 0)
        {
            var list1 = new List<(string, bool, Action)>();
            list.ForEach(item =>
            {
                list1.Add((item, true, () => _live2D.PlayExpression(item)));
            });
            _ = new FlyoutsControl(list1, _con);
        }
    }
}
