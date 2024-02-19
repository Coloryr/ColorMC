using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Flyouts;

public class FlyoutsControl
{
    public FlyoutsControl((string, bool, Action)[] list, Control con)
    {
        var flyout = new MenuFlyout();
        var list1 = new List<MenuItem>();
        foreach (var item in list)
        {
            var button = new MenuItem()
            {
                Header = item.Item1,
                IsEnabled = item.Item2,
            };
            button.PointerPressed += (a, b) =>
            {
                flyout.Hide();
                item.Item3.Invoke();
            };
            list1.Add(button);
        }
        flyout.ItemsSource = list1;
        flyout.ShowAt(con, true);
    }
}