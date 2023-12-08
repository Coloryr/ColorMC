using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    public void End()
    {
        ScrollViewer1.ScrollToEnd();
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
