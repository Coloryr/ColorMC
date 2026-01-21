using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class Tab4Control : UserControl
{
    public Tab4Control()
    {
        InitializeComponent();

        TreeView1.DoubleTapped += DataGrid1_DoubleTapped;
        TreeView1.PointerPressed += DataGrid1_CellPointerPressed;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            Grid2.IsVisible = true;
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
        (DataContext as GameEditModel)!.DropMod(e.DataTransfer);
    }

    private void DataGrid1_CellPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }
    }

    private void DataGrid1_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is not DataGrid data || data.CurrentColumn == null)
        {
            return;
        }
        if (data.CurrentColumn.DisplayIndex == 1)
        {
            return;
        }
        (DataContext as GameEditModel)!.DisEMod();
    }

    private void Flyout(Control control)
    {
        if (DataContext is not GameEditModel model)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var items = TreeView1.RowSelection?.SelectedItems;
            if (items == null)
            {
                return;
            }
            GameEditFlyout1.Show(control, items, model);
        });
    }
}
