using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class ScreenshotControl : UserControl
{
    public ScreenshotControl()
    {
        InitializeComponent();

        PointerPressed += ScreenshotControl_PointerPressed;
        PointerReleased += ScreenshotControl_PointerReleased;

        PointerEntered += ScreenshotControl_PointerEntered;
        PointerExited += ScreenshotControl_PointerExited;

        DoubleTapped += ScreenshotControl_DoubleTapped;
    }

    private void ScreenshotControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void ScreenshotControl_DoubleTapped(object? sender, TappedEventArgs e)
    {
        LongPressed.Released();
        var model = (DataContext as ScreenshotModel)!;
        PathBinding.OpenPicFile(model.Screenshot);
    }

    private void ScreenshotControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = false;
    }

    private void ScreenshotControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = true;
    }

    private void ScreenshotControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as ScreenshotModel)!;
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
            var model = (DataContext as ScreenshotModel)!;
            _ = new GameEditFlyout4(control, model);
        });
    }
}
