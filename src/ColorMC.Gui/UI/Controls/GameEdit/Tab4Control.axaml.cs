using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab4Control : UserControl
{
    public Tab4Control()
    {
        InitializeComponent();

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
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
        (DataContext as GameEditModel)!.DropMod(e.Data);
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }
        else
        {
            LongPressed.Pressed(() => Flyout((sender as Control)!));
        }
    }

    private void DataGrid1_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var data = (sender as DataGrid)!;
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

        var items = DataGrid1.SelectedItems;
        _ = new GameEditFlyout1(control, items, model);
    }
}
