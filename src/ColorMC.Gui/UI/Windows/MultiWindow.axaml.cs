using Avalonia.Controls;
using ColorMC.Gui.UI.Controls;

namespace ColorMC.Gui.UI.Windows;

public partial class MultiWindow : AMultiWindow
{
    public override HeadControl Head => HeadControl;

    public MultiWindow() 
    {
        InitializeComponent();
    }

    public MultiWindow(BaseUserControl con)
    {
        InitializeComponent();

        InitMultiWindow(con);
    }

    protected override void SetChild(Control control)
    {
        MainControl.Child = control;
    }
}
