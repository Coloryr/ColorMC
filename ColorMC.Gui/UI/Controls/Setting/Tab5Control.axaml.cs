using Avalonia.Controls;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab5Control : UserControl
{
    private SettingWindow Window;
    public Tab5Control()
    {
        InitializeComponent();
    }
    public void SetWindow(SettingWindow window)
    {
        Window = window;
    }

    public void Load()
    { 
        
    }
}
