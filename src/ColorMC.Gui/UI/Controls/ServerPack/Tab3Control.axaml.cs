using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class Tab3Control : UserControl
{
    public Tab3Control()
    {
        InitializeComponent();

        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;
        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }

    private void DataGrid1_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        (DataContext as ServerPackModel)?.ConfigItemEdit();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is ServerPackModel model && model.NowView == 2)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
