using Avalonia.Controls;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class Tab3Control : UserControl
{
    public Tab3Control()
    {
        InitializeComponent();

        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }

    private void DataGrid1_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        (DataContext as ServerPackTab3Model)?.ItemEdit();
    }
}
