using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// 存档子项目
/// </summary>
public partial class WorldControl : UserControl
{
    public WorldControl()
    {
        InitializeComponent();

        PointerPressed += WorldControl_PointerPressed;

        DataGridDataPack.DoubleTapped += DataGridDataPack_DoubleTapped;
        DataGridDataPack.CellPointerPressed += DataGridDataPack_CellPointerPressed;
    }

    private void DataGridDataPack_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var model = (DataContext as WorldModel)!;

            var pro = e.PointerPressedEventArgs.GetCurrentPoint(this);

            if (pro.Properties.IsRightButtonPressed)
            {
                Flyout1((sender as Control)!);
            }
        });
    }

    private void DataGridDataPack_DoubleTapped(object? sender, TappedEventArgs e)
    {
        (DataContext as WorldModel)!.DisE();
    }

    private void WorldControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as WorldModel)!;
        model.Select();
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }
    }

    private void Flyout(Control control)
    {
        if (DataContext is not WorldModel model)
        {
            return;
        }
        GameEditFlyout2.Show(control, model);
    }

    private void Flyout1(Control control)
    {
        if (DataContext is not WorldModel model)
        {
            return;
        }
        GameEditFlyout8.Show(control, DataGridDataPack.SelectedItems, model);
    }
}
