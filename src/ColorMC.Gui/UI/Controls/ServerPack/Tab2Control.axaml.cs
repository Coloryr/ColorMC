using Avalonia.Controls;
using ColorMC.Gui.UI.Model.ServerPack;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;
    }

    private void DataGrid1_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        (DataContext as ServerPackTab2Model)?.ItemEdit();
    }
}
