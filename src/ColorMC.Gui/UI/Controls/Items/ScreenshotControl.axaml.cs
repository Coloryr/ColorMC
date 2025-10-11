using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// ½ØÍ¼ÏîÄ¿
/// </summary>
public partial class ScreenshotControl : UserControl
{
    public ScreenshotControl()
    {
        InitializeComponent();

        PointerPressed += ScreenshotControl_PointerPressed;
        DoubleTapped += ScreenshotControl_DoubleTapped;
    }

    private void ScreenshotControl_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var model = (DataContext as ScreenshotModel)!;
        PathBinding.OpenPicFile(model.Screenshot);
    }

    private void ScreenshotControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as ScreenshotModel)!;
        model.Select();
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }
    }

    private void Flyout(Control control)
    {
        if (DataContext is not ScreenshotModel model)
        {
            return;
        }
        GameEditFlyout4.Show(control, model);
    }
}
