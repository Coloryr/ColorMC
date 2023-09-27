using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class WorldControl : UserControl
{
    public WorldControl()
    {
        InitializeComponent();

        PointerPressed += WorldControl_PointerPressed;
        PointerReleased += WorldControl_PointerReleased;
        PointerEntered += WorldControl_PointerEntered;
        PointerExited += WorldControl_PointerExited;
    }

    private void WorldControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void WorldControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = false;
    }

    private void WorldControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = true;
    }

    private void WorldControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as WorldModel)!;
        model.Select();
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }

        LongPressed.Pressed(() => Flyout((sender as Control)!));
    }

    private void Flyout(Control control)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var model = (DataContext as WorldModel)!;
            _ = new GameEditFlyout2(control, model);
        });
    }
}
