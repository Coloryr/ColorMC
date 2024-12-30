using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Custom;

namespace ColorMC.CustomGui;

public class CustomUI : ICustomControl
{
    public string LauncherApi => "A34";
    /// <summary>
    /// 返回主界面 一般不用动
    /// </summary>
    public BaseUserControl GetControl()
    {
        return new UIControl();
    }
}
