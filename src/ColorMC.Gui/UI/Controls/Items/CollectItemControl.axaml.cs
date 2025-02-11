using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// ��Դ�ղ�����Ŀ
/// </summary>
public partial class CollectItemControl : UserControl
{
    public CollectItemControl()
    {
        InitializeComponent();

        PointerEntered += CollectItemControl_PointerEntered;
        PointerExited += CollectItemControl_PointerExited;
        PointerPressed += CollectItemControl_PointerPressed;
        PointerReleased += CollectItemControl_PointerReleased;
    }

    private void CollectItemControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Cancel();
    }

    private void CollectItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not CollectItemModel model)
        {
            return;
        }

        model.SetSelect();

        void OpenFlyout()
        {
            _ = new CollectFlyout((sender as Control)!, model);
            e.Handled = true;
        }

        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsRightButtonPressed)
        {
            model.IsCheck = true;
            OpenFlyout();
        }
        else
        {
            LongPressed.Pressed(OpenFlyout);
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