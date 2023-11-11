using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameCloud;

namespace ColorMC.Gui.UI.Controls.GameCloud;

public partial class Tab3Control : UserControl
{
    public Tab3Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is GameCloudModel model && model.NowView == 2)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
