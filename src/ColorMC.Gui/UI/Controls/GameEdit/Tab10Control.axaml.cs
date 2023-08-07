using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab10Control : UserControl
{
    public Tab10Control()
    {
        InitializeComponent();

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _ = new GameEditFlyout5(this, (DataContext as GameEditTab10Model)!);
            });
        }
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }
}
