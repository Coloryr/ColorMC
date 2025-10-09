using Avalonia.Controls;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Dialog;

namespace ColorMC.Gui.UI.Controls.Dialog;

/// <summary>
/// NBT±Í«©…Ë÷√
/// </summary>
public partial class NbtDialogEditControl : UserControl
{
    public NbtDialogEditControl()
    {
        InitializeComponent();

        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
    }

    private void DataGrid1_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            (DataContext as NbtDialogEditModel)!.DataEdit();
        }
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout2((sender as Control)!);
        }
        else
        {
            LongPressed.Pressed(() => Flyout2((sender as Control)!));
        }
    }

    private void Flyout2(Control control)
    {
        if (DataContext is not NbtDialogEditModel model)
        {
            return;
        }
        if (model.DataItem != null)
        {
            ConfigFlyout2.Show(control, model, model.DataItem);
        }
    }
}
