using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Layout;
using Avalonia.Media;

namespace ColorMC.Gui.UI.Views;

public partial class Info2Control : UserControl
{
    private readonly static IBrush Back = Brush.Parse("#5ABED6");
    private readonly static IBrush Back1 = Brush.Parse("#FFFFFF");

    private readonly static CrossFade transition = new(TimeSpan.FromMilliseconds(200));

    public Info2Control()
    {
        InitializeComponent();
    }

    public async void Show(string title)
    {
        var grid = new Grid()
        {
            Background = Back,
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
        await transition.Start(null, grid, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(2));
        await transition.Start(grid, null, CancellationToken.None);
        Grid_List.Children.Remove(grid);
    }
}
