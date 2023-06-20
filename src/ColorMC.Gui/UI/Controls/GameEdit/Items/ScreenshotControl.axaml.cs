using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class ScreenshotControl : UserControl
{
    public ScreenshotControl()
    {
        InitializeComponent();

        PointerPressed += ScreenshotControl_PointerPressed;

        PointerEntered += ScreenshotControl_PointerEntered;
        PointerExited += ScreenshotControl_PointerExited;
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
        if (DataContext is ScreenshotModel model)
        {
            model.Select();
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                model.Flyout(this);
            }
        }
    }
}
