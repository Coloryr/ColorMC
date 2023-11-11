using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameExport;

namespace ColorMC.Gui.UI.Controls.GameExport;

public partial class Tab1Control : UserControl
{
    public Tab1Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is GameExportModel model && model.NowView == 0)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
