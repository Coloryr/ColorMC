using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Info;

public partial class Info2Control : UserControl
{
    public Info2Control()
    {
        InitializeComponent();

        DataContextChanged += Info2Control_DataContextChanged;
    }

    private void Info2Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Info2Show")
        {
            Show((DataContext as BaseModel)!.NotifyText);
        }
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
