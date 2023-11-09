using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameExport;
using ColorMC.Gui.UI.Model.NetFrp;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.GameExport;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is GameExportModel model && model.NowView == 1)
        {
            if (e.Delta.Y < 0)
            {
                model.NowView++;
            }
            else if (e.Delta.Y > 0)
            {
                model.NowView--;
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ScrollViewer1.PointerWheelChanged -= ScrollViewer1_PointerWheelChanged;
    }
}