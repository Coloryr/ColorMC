using System.Collections.Generic;
using Avalonia.Controls;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.UI.Flyouts;

public class FlyoutsControl
{
    public FlyoutsControl(ICollection<FlyoutMenuObj> list, Control con)
    {
        var flyout = new MenuFlyout();
        flyout.ItemsSource = BuildItems(flyout, list);
        flyout.ShowAt(con, true);
    }

    private static List<MenuItem> BuildItems(MenuFlyout flyout, ICollection<FlyoutMenuObj> list)
    {
        var list1 = new List<MenuItem>();
        foreach (var item in list)
        {
            list1.Add(BuildItem(flyout, item));
        }
        return list1;
    }

    private static MenuItem BuildItem(MenuFlyout flyout, FlyoutMenuObj item)
    {
        var button = new MenuItem()
        {
            Header = item.Name,
            IsEnabled = item.Enable,
        };
        if (item.SubItem != null)
        {
            button.ItemsSource = BuildItems(flyout, item.SubItem);
        }
        if (item.Action != null)
        {
            button.PointerPressed += (a, b) =>
            {
                flyout.Hide();
                item.Action.Invoke();
            };
        }

        return button;
    }
}