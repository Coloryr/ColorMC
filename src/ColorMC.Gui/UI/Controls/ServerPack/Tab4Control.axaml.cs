using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class Tab4Control : UserControl
{
    public Tab4Control()
    {
        InitializeComponent();

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this)
            .Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                (DataContext as ServerPackTab4Model)?.Flyout(this);
            });
        }
    }
}
