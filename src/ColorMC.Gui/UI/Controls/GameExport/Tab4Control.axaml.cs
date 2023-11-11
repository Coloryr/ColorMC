using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameExport;
using ColorMC.Gui.Utils;
using System;

namespace ColorMC.Gui.UI.Controls.GameExport;

public partial class Tab4Control : UserControl
{
    public Tab4Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is GameExportModel model && model.NowView == 3)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
