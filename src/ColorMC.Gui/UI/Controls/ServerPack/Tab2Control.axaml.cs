using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UIBinding;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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
