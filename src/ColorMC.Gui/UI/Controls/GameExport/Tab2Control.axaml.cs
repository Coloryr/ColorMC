using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameExport;
using ColorMC.Gui.UI.Model.NetFrp;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.GameExport;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is GameExportModel model && model.NowView == 1)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}