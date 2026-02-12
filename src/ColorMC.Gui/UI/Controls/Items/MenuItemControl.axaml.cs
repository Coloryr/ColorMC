using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

public partial class MenuItemControl : UserControl
{
    public MenuItemControl()
    {
        InitializeComponent();

        PointerPressed += MenuItemControl_PointerPressed;
    }

    private void MenuItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsLeftButtonPressed)
        {
            e.Handled = true;
            if (DataContext is MenuItemModel model)
            {
                model.IsCheck = true;
            }
        }
    }
}