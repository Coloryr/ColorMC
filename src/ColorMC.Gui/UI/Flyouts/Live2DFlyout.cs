using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Main;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// Live2d控件
/// 右键菜单
/// </summary>
public class Live2DFlyout
{
    public Live2DFlyout(Live2dRender live2d)
    {
        var list = live2d.GetMotions();
        var list1 = new List<FlyoutMenuObj>();
        if (list.Count != 0)
        {
            list.ForEach(item =>
            {
                list1.Add(new(item, true, () => live2d.PlayMotion(item)));
            });
        }
        else
        {
            list1.Add(new(App.Lang("Live2dControl.Flyouts.Text3"), false, null));
        }
        var list2 = live2d.GetExpressions();
        var list3 = new List<FlyoutMenuObj>();
        if (list2.Count != 0)
        {
            list2.ForEach(item =>
            {
                list3.Add(new(item, true, () => live2d.PlayExpression(item)));
            });
        }
        else
        {
            list3.Add(new(App.Lang("Live2dControl.Flyouts.Text4"), false, null));
        }
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Live2dControl.Flyouts.Text1"), true, null)
            {
                SubItem = list1
            },
            new FlyoutMenuObj(App.Lang("Live2dControl.Flyouts.Text2"), true, null)
            {
                SubItem = list3
            },
        ], live2d);
    }
}
