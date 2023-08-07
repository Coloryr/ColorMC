using Avalonia.Controls;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.Utils;
using System;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class Tab3Control : UserControl
{
    private ServerPackTab3Model _model;
    public Tab3Control()
    {
        InitializeComponent();

        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;
        DataContextChanged += Tab3Control_DataContextChanged;
    }

    private void Tab3Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ServerPackTab3Model model)
        {
            _model = model;
        }
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }

    private void DataGrid1_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        _model.ItemEdit();
    }
}
