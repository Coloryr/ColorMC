using Avalonia.Controls;

namespace ColorMC.Gui.UI.Views.Hello;

public partial class Tab3Control : UserControl
{
    private HelloWindow Window;
    public Tab3Control()
    {
        InitializeComponent();
    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }
}
