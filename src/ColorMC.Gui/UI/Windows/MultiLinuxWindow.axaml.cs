using Avalonia.Controls;
using ColorMC.Gui.UI.Controls;

namespace ColorMC.Gui.UI.Windows;

public partial class MultiLinuxWindow : AMultiWindow
{
    public MultiLinuxWindow()
    {
        InitializeComponent();
    }

    public MultiLinuxWindow(BaseUserControl con)
    {
        InitializeComponent();

        SystemDecorations = SystemDecorations.BorderOnly;

        Init(con);
    }

    public override HeadControl Head => HeadControl;

    protected override void SetChild(Control control)
    {
        MainControl.Child = control;
    }
}
