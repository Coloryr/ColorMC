using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class Tab1Control : UserControl
{
    public Tab1Control()
    {
        InitializeComponent();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is ServerPackModel model && model.NowView == 1)
        {
            if (e.Delta.Y < 0 && ScrollViewer1.ScrollBarMaximum == ScrollViewer1.Offset)
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
