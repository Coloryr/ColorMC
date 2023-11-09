using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab9Control : UserControl
{
    public Tab9Control()
    {
        InitializeComponent();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is GameEditModel model && model.NowView == 5)
        {
            if (e.Delta.Y < 0)
            {
                model.NowView++;
            }
            else if (e.Delta.Y > 0)
            {
                model.NowView--;
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
