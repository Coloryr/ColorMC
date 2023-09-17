using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.GameCloud.Items;

public partial class WorldCloudControl : UserControl
{
    public WorldCloudControl()
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
        if (DataContext is WorldCloudModel model)
        {
            model.Select();
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                _ = new GameCloudFlyout1(this, model);
            }
        }
    }
}
