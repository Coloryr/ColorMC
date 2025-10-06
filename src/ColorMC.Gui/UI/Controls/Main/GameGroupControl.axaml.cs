using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Controls.Main;

/// <summary>
/// 游戏分组
/// </summary>
public partial class GameGroupControl : UserControl
{
    public GameGroupControl()
    {
        InitializeComponent();

        Expander_Head.ContentTransition = ThemeManager.CrossFade;

        PointerEntered += GameGroupControl_PointerEntered;
        PointerExited += GameGroupControl_PointerExited;
        PointerPressed += GameGroupControl_PointerPressed;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void GameGroupControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var temp = e.GetCurrentPoint(this).Properties;
        if (temp.IsLeftButtonPressed)
        {
            if (DataContext is GameGroupModel model)
            {
                model.Top.EndMut();
                model.Top.SearchClose();
                model.Top.Select(null);
            }
        }
        else if (temp.IsRightButtonPressed)
        {
            if (DataContext is GameGroupModel model)
            {
                if (model.Top.IsMut)
                {
                    Flyout1(this, model);
                }
                else
                {
                    Flyout(this, model);
                }
            }
            e.Handled = true;
        }
    }

    private static void Flyout(Control con, GameGroupModel model)
    {
        _ = new MainFlyout1(con, model);
    }

    private static void Flyout1(Control con, GameGroupModel model)
    {
        _ = new MainFlyout2(con, model, model.Top);
    }

    private void GameGroupControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Button1.IsVisible = false;
    }

    private void GameGroupControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Button1.IsVisible = true;
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Source is Control && DataContext is GameGroupModel model)
        {
            Grid1.IsVisible = model.DropIn(e.DataTransfer);
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid1.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        Grid1.IsVisible = false;
        if (e.Source is Control && DataContext is GameGroupModel model)
        {
            model.Drop(e.DataTransfer);
        }
    }
}
