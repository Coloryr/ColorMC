using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit;

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
        else
        {
            LongPressed.Pressed(() => Flyout((sender as Control)!));
        }
    }

    private void Flyout(Control control)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _ = new GameEditFlyout5(control, (DataContext as GameEditModel)!);
        });
    }
}
