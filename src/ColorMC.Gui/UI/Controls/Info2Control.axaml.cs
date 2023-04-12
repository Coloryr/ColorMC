using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info2Control : UserControl
{
    public Info2Control()
    {
        InitializeComponent();
    }

    public async void Show(string title)
    {
        var grid = new Grid()
        {
            Background = ColorSel.MainColor,
            Margin = new Thickness(0, 0, 0, 60),
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var text = new TextBlock()
        {
            Margin = new Thickness(2),
            Padding = new Thickness(5, 0, 5, 0),
            Background = ColorSel.BottomColor,
            FontSize = 20,
            Foreground = ColorSel.FontColor,
            Text = title
        };
        grid.Children.Add(text);

        Grid1.Children.Add(grid);
        await App.CrossFade200.Start(null, grid, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(2));
        await App.CrossFade200.Start(grid, null, CancellationToken.None);
        Grid1.Children.Remove(grid);
    }
}
