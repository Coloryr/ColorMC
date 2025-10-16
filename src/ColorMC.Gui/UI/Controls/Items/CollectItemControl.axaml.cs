using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// 资源收藏子项目
/// </summary>
public partial class CollectItemControl : UserControl
{
    public CollectItemControl()
    {
        InitializeComponent();

        PointerEntered += CollectItemControl_PointerEntered;
        PointerExited += CollectItemControl_PointerExited;
        PointerPressed += CollectItemControl_PointerPressed;
    }

    private void CollectItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not CollectItemModel model)
        {
            return;
        }

        model.SetSelect();

        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsRightButtonPressed)
        {
            model.IsCheck = true;
            CollectFlyout.Show((sender as Control)!, model);
            e.Handled = true;
        }
    }

    private void CollectItemControl_PointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is CollectItemModel model)
        {
            model.Top = false;
        }
    }

    private void CollectItemControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is CollectItemModel model)
        {
            model.Top = true;
        }
    }
}