using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }

    private void DataGrid1_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        (DataContext as ServerPackModel)?.ModItemEdit();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is SettingModel model && model.NowView == 1)
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
