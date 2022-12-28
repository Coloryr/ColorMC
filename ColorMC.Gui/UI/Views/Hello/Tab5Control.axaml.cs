using Avalonia.Controls;

namespace ColorMC.Gui.UI.Views.Hello;

public partial class Tab5Control : UserControl
{
    private HelloWindow Window;
    public Tab5Control()
    {
        InitializeComponent();


    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }
}
