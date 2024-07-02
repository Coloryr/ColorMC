using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.CustomGui;

public class CustomUI : ICustomControl
{
    public BaseUserControl GetControl()
    {
        return new UIControl();
    }
}
