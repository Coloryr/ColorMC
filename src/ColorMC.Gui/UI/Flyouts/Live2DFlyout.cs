using ColorMC.Gui.UI.Controls.Main;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Flyouts;

public class Live2DFlyout
{
    public Live2DFlyout(Live2dRender live2d)
    {
        _ = new FlyoutsControl(
        [
            (App.Lang("Live2DFlyout.Text1"), true, ()=>
            {
                var list = live2d.GetMotions();
                if (list.Count != 0)
                {
                    var list1 = new List<(string, bool, Action)>();
                    list.ForEach(item =>
                    {
                        list1.Add((item, true, () => live2d.PlayMotion(item)));
                    });
                    _ = new FlyoutsControl([.. list1], live2d);
                }
            }),
            (App.Lang("Live2DFlyout.Text2"), true, ()=>
            {
                var list = live2d.GetExpressions();
                if (list.Count != 0)
                {
                    var list1 = new List<(string, bool, Action)>();
                    list.ForEach(item =>
                    {
                        list1.Add((item, true, () => live2d.PlayExpression(item)));
                    });
                    _ = new FlyoutsControl([.. list1], live2d);
                }
            }),
        ], live2d);
    }
}
