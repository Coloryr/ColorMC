using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class WorldControl : UserControl
{
    public WorldControl()
    {
        InitializeComponent();

        PointerPressed += WorldControl_PointerPressed;

        PointerEntered += WorldControl_PointerEntered;
        PointerExited += WorldControl_PointerExited;
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
        if (DataContext is WorldModel model)
        {
            model.Select();
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                _ = new GameEditFlyout2(this, model);
            }
        }
    }
}
