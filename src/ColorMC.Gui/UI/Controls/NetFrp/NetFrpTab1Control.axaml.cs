using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.NetFrp;

namespace ColorMC.Gui.UI.Controls.NetFrp;

public partial class NetFrpTab1Control : UserControl
{
    public NetFrpTab1Control()
    {
        InitializeComponent();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is NetFrpModel model && model.NowView == 0)
        {
            if (e.Delta.Y < 0)
            {
                model.NowView++;
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ScrollViewer1.PointerWheelChanged -= ScrollViewer1_PointerWheelChanged;
    }
}
