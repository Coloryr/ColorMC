using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    public void Reset()
    {
        ScrollViewer1.ScrollToHome();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is GameEditModel model && model.NowView == 1)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
