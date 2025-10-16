using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

public partial class BlockItemControl : UserControl
{
    public BlockItemControl()
    {
        InitializeComponent();

        PointerPressed += BlockItemControl_PointerPressed;
    }

    private void BlockItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not BlockItemModel model
            || model.Top == null)
        {
            return;
        }

        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsRightButtonPressed)
        {
            BlockFlyout.Show((sender as Control)!, model);
            e.Handled = true;
        }
    }
}