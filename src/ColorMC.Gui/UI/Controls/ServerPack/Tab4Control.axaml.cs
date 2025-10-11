using Avalonia.Controls;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.ServerPack;

namespace ColorMC.Gui.UI.Controls.ServerPack;

/// <summary>
/// 服务器实例生成窗口
/// </summary>
public partial class Tab4Control : UserControl
{
    public Tab4Control()
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
        if (DataContext is not ServerPackModel model)
        {
            return;
        }
        ServerPackFlyout1.Show(control, model, model.FileItem);
    }
}
