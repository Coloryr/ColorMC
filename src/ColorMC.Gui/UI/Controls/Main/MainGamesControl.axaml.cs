using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using ColorMC.Gui.UI.Controls.Items;
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
}