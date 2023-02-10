using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info2Control : UserControl
{
    private readonly static IBrush Back1 = Brush.Parse("#FFFFFF");

    public Info2Control()
    {
        InitializeComponent();
    }

    public async void Show(string title)
    {
        var grid = new Grid()
        {
            Background = Utils.LaunchSetting.ColorSel.MainColor,
            Margin = new Thickness(0, 0, 0, 60),
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var text = new TextBlock()
        {
            Margin = new Thickness(2),
            Padding = new Thickness(5, 0, 5, 0),
            Background = Back1,
            FontSize = 20,
            Foreground = Brushes.Black,
            Text = title
        };
        grid.Children.Add(text);

        Grid_List.Children.Add(grid);
        await App.CrossFade200.Start(null, grid, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(2));
        await App.CrossFade200.Start(grid, null, CancellationToken.None);
        Grid_List.Children.Remove(grid);
    }
}
