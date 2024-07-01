using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

public partial class WorldControl : UserControl
{
    public WorldControl()
    {
        InitializeComponent();

        PointerPressed += WorldControl_PointerPressed;
        PointerReleased += WorldControl_PointerReleased;
        PointerMoved += WorldControl_PointerMoved;

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
            else
            {
                LongPressed.Pressed(() => Flyout1((sender as Control)!));
            }
        });
    }

    private void DataGridDataPack_DoubleTapped(object? sender, TappedEventArgs e)
    {
        (DataContext as WorldModel)!.DisE();
    }

    private void WorldControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        LongPressed.Cancel();
    }

    private void WorldControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void WorldControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as WorldModel)!;
        model.Select();
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
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
            var model = (DataContext as WorldModel)!;
            _ = new GameEditFlyout2(control, model);
        });
    }

    private void Flyout1(Control control)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var model = (DataContext as WorldModel)!;
            _ = new GameEditFlyout8(control, DataGridDataPack.SelectedItems, model);
        });
    }
}
