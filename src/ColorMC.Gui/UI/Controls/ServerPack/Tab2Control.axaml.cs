using Avalonia.Controls;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.Utils;
using System;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class Tab2Control : UserControl
{
    private ServerPackTab2Model _model;
    public Tab2Control()
    {
        InitializeComponent();

        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;
        DataContextChanged += Tab2Control_DataContextChanged;
    }

    private void Tab2Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ServerPackTab2Model model)
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
