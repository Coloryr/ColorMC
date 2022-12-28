using Avalonia.Controls;

namespace ColorMC.Gui.UI.Views.Hello;

public partial class Tab4Control : UserControl
{
    private HelloWindow Window;
    public Tab4Control()
    {
        InitializeComponent();

    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }
}
