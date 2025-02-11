using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.Setting;

/// <summary>
/// Æô¶¯Æ÷ÉèÖÃ´°¿Ú
/// </summary>
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
            Flyout((sender as Control)!);
        }
        else
        {
            LongPressed.Pressed(() => Flyout((sender as Control)!));
        }
    }

    private void Flyout(Control control)
    {
        if (DataContext is not SettingModel model)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            _ = new SettingFlyout3(control, model, model.DnsItem);
        });
    }
}
