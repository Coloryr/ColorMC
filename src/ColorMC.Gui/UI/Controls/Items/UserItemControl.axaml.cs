using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

public partial class UserItemControl : UserControl
{
    public UserItemControl()
    {
        InitializeComponent();

        DoubleTapped += UserItemControl_DoubleTapped;
        PointerPressed += UserItemControl_PointerPressed;
        PointerReleased += UserItemControl_PointerReleased;
    }

    private void UserItemControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void UserItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as UserDisplayModel)!;


        var pro = e.GetCurrentPoint(this);

        if (pro.Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }
        else
        {
            LongPressed.Pressed(() => Flyout((sender as Control)!));
        }
    }

    private void UserItemControl_DoubleTapped(object? sender, TappedEventArgs e)
    {
        (DataContext as UserDisplayModel)!.Select();
    }

    private void Flyout(Control control)
    {
        var model = (DataContext as UserDisplayModel)!;
        _ = new UserFlyout(control, model);
    }
}
