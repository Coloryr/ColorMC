using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils;
using System;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab5Control : UserControl
{

    public Tab5Control()
    {
        InitializeComponent();

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        LayoutUpdated += Tab5Control_LayoutUpdated;
    }


    private void DataGrid1_CellPointerPressed(object? sender,
        DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                (DataContext as SettingTab5Model)?.Flyout(this, DataGrid1.SelectedItems);
            });
        }
    }

    private void Tab5Control_LayoutUpdated(object? sender, EventArgs e)
    {
        DataGrid1.SetFontColor();
    }
}
