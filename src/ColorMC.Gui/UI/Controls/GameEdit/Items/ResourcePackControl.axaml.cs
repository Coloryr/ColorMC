using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class ResourcePackControl : UserControl
{
    public ResourcePackControl()
    {
        InitializeComponent();

        PointerPressed += ResourcePackControl_PointerPressed;
        PointerReleased += ResourcePackControl_PointerReleased;
        PointerEntered += ResourcePackControl_PointerEntered;
        PointerExited += ResourcePackControl_PointerExited;
    }

    private void ResourcePackControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        
    }

    private void ResourcePackControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = false;
    }

    private void ResourcePackControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = true;
    }

    private void ResourcePackControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as ResourcePackModel)!;
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
            var model = (DataContext as ResourcePackModel)!;
            _ = new GameEditFlyout3(this, model);
        });
    }
}
