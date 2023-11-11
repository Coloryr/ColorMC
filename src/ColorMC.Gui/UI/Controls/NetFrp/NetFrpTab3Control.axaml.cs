using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.NetFrp;

namespace ColorMC.Gui.UI.Controls.NetFrp;

public partial class NetFrpTab3Control : UserControl
{
    public NetFrpTab3Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is NetFrpModel model && model.NowView == 2)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
