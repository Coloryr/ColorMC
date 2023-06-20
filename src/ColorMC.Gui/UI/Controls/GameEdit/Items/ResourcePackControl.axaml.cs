using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class ResourcePackControl : UserControl
{
    public ResourcePackControl()
    {
        InitializeComponent();

        PointerPressed += ResourcePackControl_PointerPressed;

        PointerEntered += ResourcePackControl_PointerEntered;
        PointerExited += ResourcePackControl_PointerExited;
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
        if (DataContext is ResourcePackModel model)
        {
            model.Select();
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                model.Flyout(this);
            }
        }
    }
}
