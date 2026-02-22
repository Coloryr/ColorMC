using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using ColorMC.Gui.UI.Controls.Items;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class MainGamesControl : UserControl
{
    public MainGamesControl()
    {
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);

        PointerPressed += MainGamesControl_PointerPressed;
    }

    private void MainGamesControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var temp = e.GetCurrentPoint(this).Properties;
        if (temp.IsLeftButtonPressed)
        {
            if (DataContext is MainModel model)
            {
                model.OneGroup.Top.EndMut();
                model.OneGroup.Top.SearchClose();
                model.OneGroup.Top.Select(null);
            }
        }
        else if (temp.IsRightButtonPressed)
        {
            if (DataContext is MainModel model)
            {
                if (model.OneGroup.Top.IsMut)
                {
                    Flyout1(this, model.OneGroup);
                }
                else
                {
                    Flyout(this, model.OneGroup);
                }
            }
            e.Handled = true;
        }
    }

    public void OnDragOver(object? sender, DragEventArgs e)
    {
        var draggedItem = e.DataTransfer.TryGetValue(BaseBinding.DrapType);

        if (draggedItem == null)
        {
            return;
        }

        var vm = DataContext as MainModel;

        var visualTarget = e.Source as Visual;
        var targetBorder = visualTarget?.FindAncestorOfType<GameControl>();

        if (targetBorder != null && targetBorder?.DataContext is GameItemModel targetItem)
        {
            var pos = e.GetPosition(targetBorder);

            if (pos.X > targetBorder.Bounds.Width / 2)
            {
                vm?.OneGroup.PutPla(targetItem, false);
            }
            else
            {
                vm?.OneGroup.PutPla(targetItem, true);
            }
        }
        else
        {
            vm?.OneGroup.PutPla();
        }
    }

    public void OnDrop(object? sender, DragEventArgs e)
    {
        var draggedItem = e.DataTransfer.TryGetValue(BaseBinding.DrapType);

        if (draggedItem == null)
        {
            return;
        }

        var vm = DataContext as MainModel;

        var visualTarget = e.Source as Visual;
        var targetBorder = visualTarget?.FindAncestorOfType<GameControl>();

        if (targetBorder != null && targetBorder?.DataContext is GameItemModel targetItem)
        {
            var pos = e.GetPosition(targetBorder);

            if (pos.X > targetBorder.Bounds.Width / 2)
            {
                vm?.OneGroup.PutDrap(targetItem, false);
            }
            else
            {
                vm?.OneGroup.PutDrap(targetItem, true);
            }
        }
        else
        {
            vm?.OneGroup.PutDrap();
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
}