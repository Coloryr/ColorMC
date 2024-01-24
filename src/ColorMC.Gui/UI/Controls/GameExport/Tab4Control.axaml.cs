using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameExport;

namespace ColorMC.Gui.UI.Controls.GameExport;

public partial class Tab4Control : UserControl
{
    public Tab4Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is GameExportModel model && model.NowView == 3)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
