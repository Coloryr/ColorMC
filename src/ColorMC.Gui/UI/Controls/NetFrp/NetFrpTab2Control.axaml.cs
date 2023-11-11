using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.NetFrp;

namespace ColorMC.Gui.UI.Controls.NetFrp;

public partial class NetFrpTab2Control : UserControl
{
    public NetFrpTab2Control()
    {
        InitializeComponent();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is NetFrpModel model && model.NowView == 1)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
