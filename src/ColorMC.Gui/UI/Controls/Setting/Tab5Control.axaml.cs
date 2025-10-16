using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.Setting;

/// <summary>
/// 启动器设置窗口
/// </summary>
public partial class Tab5Control : UserControl
{
    public Tab5Control()
    {
        InitializeComponent();

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
    }

    private void DataGrid1_CellPointerPressed(object? sender,
        DataGridCellPointerPressedEventArgs e)
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
            SettingFlyout1.Show(control, model, DataGrid1.SelectedItems);
        });
    }
}
