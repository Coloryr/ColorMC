using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Custom;

namespace ColorMC.CustomGui;

public class CustomUI : ICustomControl
{
    public BaseUserControl GetControl()
    {
        return new UIControl();
    }
}
