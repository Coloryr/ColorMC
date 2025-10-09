using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.BuildPack;

namespace ColorMC.Gui.UI.Controls.BuildPack;

public partial class Tab3Control : UserControl
{
    public Tab3Control()
    {
        InitializeComponent();

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout1((sender as Control)!);
        }
        else
        {
            LongPressed.Pressed(() => Flyout1((sender as Control)!));
        }
    }

    private void Flyout1(Control control)
    {
        if (DataContext is not BuildPackModel model)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            BuildPackFlyout1.Show(control, model);
        });
    }
}