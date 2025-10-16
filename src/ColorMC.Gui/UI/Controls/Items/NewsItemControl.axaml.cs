using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// Minecraft News子项目
/// </summary>
public partial class NewsItemControl : UserControl
{
    public NewsItemControl()
    {
        InitializeComponent();

        PointerEntered += Border1_PointerEntered;
        PointerExited += Border1_PointerExited;

        PointerPressed += NewsItemControl_PointerPressed;
    }

    private void NewsItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (DataContext is NewsItemModel model)
            {
                model.OpenUrl();
            }
        }
    }

    private void Border1_PointerExited(object? sender, PointerEventArgs e)
    {
        SubTitle.IsVisible = false;
    }

    private void Border1_PointerEntered(object? sender, PointerEventArgs e)
    {
        SubTitle.IsVisible = true;
    }
}
