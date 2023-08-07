using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.Utils;
using System;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class Tab4Control : UserControl
{
    private ServerPackTab4Model _model;
    public Tab4Control()
    {
        InitializeComponent();

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
        DataContextChanged += Tab4Control_DataContextChanged;
    }

    private void Tab4Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ServerPackTab4Model model)
        {
            _model = model;
        }
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this)
            .Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _model.Flyout(this);
            });
        }
    }
}
