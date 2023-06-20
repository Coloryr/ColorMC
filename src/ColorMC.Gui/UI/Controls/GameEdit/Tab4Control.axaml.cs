using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.Utils;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab4Control : UserControl
{
    public Tab4Control()
    {
        InitializeComponent();

        Button_A1.PointerExited += Button_A1_PointerLeave;
        Button_A.PointerEntered += Button_A_PointerEnter;

        Button_I1.PointerExited += Button_I1_PointerLeave;
        Button_I.PointerEntered += Button_I_PointerEnter;

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_C1.PointerExited += Button_C1_PointerLeave;
        Button_C.PointerEntered += Button_C_PointerEnter;

        Button_B1.PointerExited += Button_B1_PointerLeave;
        Button_B.PointerEntered += Button_B_PointerEnter;

        Button_D1.PointerExited += Button_D1_PointerLeave;
        Button_D.PointerEntered += Button_D_PointerEnter;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        LayoutUpdated += Tab5Control_LayoutUpdated;

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
        (DataContext as GameEditTab4Model)?.Drop(e.Data);
    }

    private void DataGrid1_CellPointerPressed(object? sender,
        DataGridCellPointerPressedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var items = DataGrid1.SelectedItems;

            if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                _ = new GameEditFlyout1(this, items, (DataContext as GameEditTab4Model)!);
            }
        });
    }

    private void DataGrid1_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        (DataContext as GameEditTab4Model)?.DisE();
    }

    private void Button_B1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_B1, null, CancellationToken.None);
        Button_B.IsVisible = true;
    }

    private void Button_B_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_B1, CancellationToken.None);
        Button_B.IsVisible = false;
    }

    private void Button_C1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_C1, null, CancellationToken.None);
        Button_C.IsVisible = true;
    }

    private void Button_C_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_C1, CancellationToken.None);
        Button_C.IsVisible = false;
    }

    private void Button_I1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_I1, null, CancellationToken.None);
        Button_I.IsVisible = true;
    }

    private void Button_I_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_I1, CancellationToken.None);
        Button_I.IsVisible = false;
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_A1, null, CancellationToken.None);
        Button_A.IsVisible = true;
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_A1, CancellationToken.None);
        Button_A.IsVisible = false;
    }
    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_R1, null, CancellationToken.None);
        Button_R.IsVisible = true;
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_R1, CancellationToken.None);
        Button_R.IsVisible = false;
    }

    private void Button_D1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_D1, null, CancellationToken.None);
        Button_D.IsVisible = true;
    }

    private void Button_D_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_D1, CancellationToken.None);
        Button_D.IsVisible = true;
    }

    private void Tab5Control_LayoutUpdated(object? sender, EventArgs e)
    {
        DataGrid1.SetFontColor();
    }
}
