using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.GameExport;
using System;

namespace ColorMC.Gui.UI.Controls.GameExport;

public partial class Tab4Control : UserControl
{
    private GameExportModel _model;

    public Tab4Control()
    {
        InitializeComponent();

        DataGrid1.CellPointerPressed += DataGrid1_Pressed;

        DataContextChanged += Tab4Control_DataContextChanged;
    }

    private void Tab4Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is GameExportModel model)
        {
            _model = model;
        }
    }

    private void DataGrid1_Pressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        var point = e.PointerPressedEventArgs.GetCurrentPoint(this);
        if (point.Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _model.CellPressd();
            });
        }
    }
}
