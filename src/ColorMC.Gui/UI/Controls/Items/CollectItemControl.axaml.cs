using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

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
        if (DataContext is CollectItemModel model)
        {
            model.SetSelect();
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