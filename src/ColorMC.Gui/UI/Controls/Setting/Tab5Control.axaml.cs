using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils;
using System;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab5Control : UserControl
{
    private SettingTab5Model _model;

    public Tab5Control()
    {
        InitializeComponent();

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
        DataContextChanged += Tab5Control_DataContextChanged;
    }

    private void Tab5Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is SettingTab5Model model)
        {
            _model = model;
        }
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }

    private void DataGrid1_CellPointerPressed(object? sender,
        DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _model.Flyout(this, DataGrid1.SelectedItems);
            });
        }
    }
}
