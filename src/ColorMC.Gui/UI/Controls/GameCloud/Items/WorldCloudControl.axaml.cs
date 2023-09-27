using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.GameCloud.Items;

public partial class WorldCloudControl : UserControl
{
    public WorldCloudControl()
    {
        InitializeComponent();

        PointerPressed += WorldCloudControl_PointerPressed;
        PointerReleased += WorldCloudControl_PointerReleased;
        PointerEntered += WorldCloudControl_PointerEntered;
        PointerExited += WorldCloudControl_PointerExited;
    }

    private void WorldCloudControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void WorldCloudControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = false;
    }

    private void WorldCloudControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = true;
    }

    private void WorldCloudControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as WorldCloudModel)!;
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
            var model = (DataContext as WorldCloudModel)!;
            _ = new GameCloudFlyout1(control, model);
        });
    }
}
