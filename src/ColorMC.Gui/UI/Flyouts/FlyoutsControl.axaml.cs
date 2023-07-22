using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Flyouts;

public partial class FlyoutsControl : UserControl
{
    public class SelfFlyout : PopupFlyoutBase
    {
        private readonly FlyoutsControl Con;
        public SelfFlyout(FlyoutsControl con)
        {
            Con = con;
        }
        protected override Control CreatePresenter()
        {
            return Con;
        }
    }
    private readonly SelfFlyout _flyoutBase;
    public FlyoutsControl(List<(string, bool, Action)>? list, Control? con)
    {
        InitializeComponent();

        if (list == null)
        {
            var button = new Button()
            {
                Width = 100,
                Height = 25,
                Content = "Test"
            };
            StackPanel1.Children.Add(button);
        }
        else
        {
            _flyoutBase = new(this);
            foreach (var item in list)
            {
                var button = new Button()
                {
                    Width = 100,
                    Height = 25,
                    Content = item.Item1,
                    IsEnabled = item.Item2
                };
                button.Click += (a, b) =>
                {
                    _flyoutBase.Hide();
                    item.Item3.Invoke();
                };
                StackPanel1.Children.Add(button);
            }
            _flyoutBase.ShowAt(con!, true);
        }
    }

    public FlyoutsControl() : this(null, null)
    {

    }
}