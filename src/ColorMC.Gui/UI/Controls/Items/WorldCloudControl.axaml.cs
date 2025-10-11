using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// 存档云同步项目
/// </summary>
public partial class WorldCloudControl : UserControl
{
    public WorldCloudControl()
    {
        InitializeComponent();

        PointerPressed += WorldCloudControl_PointerPressed;
    }

    private void WorldCloudControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as WorldCloudModel)!;
        model.Select();
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }
    }

    private void Flyout(Control control)
    {
        if (DataContext is not WorldCloudModel model)
        {
            return;
        }
        GameCloudFlyout1.Show(control, model);
    }
}
