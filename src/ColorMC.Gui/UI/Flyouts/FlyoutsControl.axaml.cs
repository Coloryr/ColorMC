using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Flyouts;

public partial class FlyoutsControl : UserControl
{
    public class SelfFlyout : PopupFlyoutBase
    {
        private readonly FlyoutsControl _con;
        public SelfFlyout(FlyoutsControl con)
        {
            _con = con;
        }
        protected override Control CreatePresenter()
        {
            return _con;
        }
    }

    public FlyoutsControl((string, bool, Action)[] list, Control con)
    {
        InitializeComponent();

        var flyout = new SelfFlyout(this);
        foreach (var item in list)
        {
            var button = new ListBoxItem()
            {
                Content = item.Item1,
                IsEnabled = item.Item2
            };
            button.PointerPressed += (a, b) =>
            {
                flyout.Hide();
                item.Item3.Invoke();
            };
            StackPanel1.Children.Add(button);
        }
        flyout.ShowAt(con!, SystemInfo.Os != OsType.Android);
    }

    public FlyoutsControl()
    {
        InitializeComponent();
        var button = new ListBoxItem()
        {
            Content = "Test"
        };
        StackPanel1.Children.Add(button);
    }
}