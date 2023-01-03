using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class TabHelloControl : UserControl
{
    private HelloWindow Window;
    public TabHelloControl()
    {
        InitializeComponent();

        Button_Next.Click += Button_Next_Click;
    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        Window.Next();
    }
}
