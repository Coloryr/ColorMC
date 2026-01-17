using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls.Items;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;

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
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    public void OnDragOver(object? sender, DragEventArgs e)
    {
        var draggedItem = e.DataTransfer.TryGetValue(BaseBinding.DrapType);

        if (draggedItem == null)
        {
            return;
        }

        var vm = DataContext as GameGroupModel;

        var visualTarget = e.Source as Visual;
        var targetBorder = visualTarget?.FindAncestorOfType<GameControl>();

        if (targetBorder != null && targetBorder?.DataContext is GameItemModel targetItem)
        {
            var pos = e.GetPosition(targetBorder);

            if (pos.X > targetBorder.Bounds.Width / 2)
            {
                vm?.PutPla(targetItem, false);
            }
            else
            {
                vm?.PutPla(targetItem, true);
            }
        }
        else
        {
            vm?.PutPla();
        }
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
        MainFlyout1.Show(con, model);
    }

    private static void Flyout1(Control con, GameGroupModel model)
    {
        MainFlyout2.Show(con, model, model.Top);
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
            var icon = e.DataTransfer.TryGetBitmap();
            if (icon != null)
            {
                var customCursor = new Cursor(icon, new PixelPoint(0, 0));
                var top = TopLevel.GetTopLevel(this);
                top?.Cursor = customCursor;
            }

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

        var draggedItem = e.DataTransfer.TryGetValue(BaseBinding.DrapType);

        if (draggedItem == null)
        {
            return;
        }

        var vm = DataContext as GameGroupModel;

        var visualTarget = e.Source as Visual;
        var targetBorder = visualTarget?.FindAncestorOfType<GameControl>();

        if (targetBorder != null && targetBorder?.DataContext is GameItemModel targetItem)
        {
            var pos = e.GetPosition(targetBorder);

            if (pos.X > targetBorder.Bounds.Width / 2)
            {
                vm?.PutDrap(targetItem, false);
            }
            else
            {
                vm?.PutDrap(targetItem, true);
            }
        }
        else
        {
            vm?.PutDrap();
        }
    }
}
