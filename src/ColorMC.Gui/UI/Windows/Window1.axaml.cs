using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Controls.Main;

namespace ColorMC.Gui.UI.Windows;

public partial class Window1 : Window
{
    private MainControl control = new();
    public Window1()
    {
        InitializeComponent();

        control.SetBaseModel(new Model.BaseModel());

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Content1.Content = null;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Content1.Content = control;
    }
}
