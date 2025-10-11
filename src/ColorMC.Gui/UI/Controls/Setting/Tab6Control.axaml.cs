using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.Setting;

/// <summary>
/// ���������ô���
/// </summary>
public partial class Tab6Control : UserControl
{
    public Tab6Control()
    {
        InitializeComponent();

        LockLoginList.CellPointerPressed += LockLoginList_CellPointerPressed;
    }

    private void LockLoginList_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
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
            LockLoginFlyout.Show(control, model.LockSelect);
        });
    }
}
