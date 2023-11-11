using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class Tab1Control : UserControl
{
    public Tab1Control()
    {
        InitializeComponent();
        
        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is ServerPackModel model && model.NowView == 1)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
