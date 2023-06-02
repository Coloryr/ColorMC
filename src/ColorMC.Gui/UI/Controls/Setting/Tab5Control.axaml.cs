using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab5Control : UserControl
{

    public Tab5Control()
    {
        InitializeComponent();

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_D1.PointerExited += Button_D1_PointerLeave;
        Button_D.PointerEntered += Button_D_PointerEnter;

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        LayoutUpdated += Tab5Control_LayoutUpdated;
    }


    private void DataGrid1_CellPointerPressed(object? sender,
        DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                (DataContext as SettingTab5Model)?.Flyout(this, DataGrid1.SelectedItems);
            });
        }
    }

    private void Tab5Control_LayoutUpdated(object? sender, EventArgs e)
    {
        DataGrid1.MakeTran();
    }

    private void Button_D1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_D1, null, CancellationToken.None);
        Button_D.IsVisible = true;
    }

    private void Button_D_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_D1, CancellationToken.None);
        Button_D.IsVisible = false;
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
}
