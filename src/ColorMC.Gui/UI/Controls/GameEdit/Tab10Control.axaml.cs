using Avalonia.Controls;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit;

/// <summary>
/// ÓÎÏ·ÊµÀý±à¼­
/// </summary>
public partial class Tab10Control : UserControl
{
    public Tab10Control()
    {
        InitializeComponent();

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }
    }

    private void Flyout(Control control)
    {
        if (DataContext is not GameEditModel model)
        {
            return;
        }
        GameEditFlyout5.Show(control, model);
    }
}
