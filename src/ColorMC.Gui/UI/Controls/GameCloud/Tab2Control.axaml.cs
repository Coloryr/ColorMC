using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameCloud;

namespace ColorMC.Gui.UI.Controls.GameCloud;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is GameCloudModel model && model.NowView == 1)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
