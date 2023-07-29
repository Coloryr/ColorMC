using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab12Control : UserControl
{
    public Tab12Control()
    {
        InitializeComponent();

        Button_A1.PointerExited += Button_A1_PointerLeave;
        Button_A.PointerEntered += Button_A_PointerEnter;

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_O1.PointerExited += Button_O1_PointerLeave;
        Button_O.PointerEntered += Button_O_PointerEnter;

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
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
        (DataContext as GameEditTab12Model)?.Drop(e.Data);
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _ = new GameEditFlyout7(this, (DataContext as GameEditTab12Model)!);
            });
        }
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_A1, null);
        Button_A.IsVisible = true;
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        Button_A.IsVisible = false;
        App.CrossFade100.Start(null, Button_A1);
    }
    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_R1, null);
        Button_R.IsVisible = true;
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        Button_R.IsVisible = false;
        App.CrossFade100.Start(null, Button_R1);
    }

    private void Button_O1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_O1, null);
        Button_O.IsVisible = true;
    }

    private void Button_O_PointerEnter(object? sender, PointerEventArgs e)
    {
        Button_O.IsVisible = false;
        App.CrossFade100.Start(null, Button_O1);
    }
}

