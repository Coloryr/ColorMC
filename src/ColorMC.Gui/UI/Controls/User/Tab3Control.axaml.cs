using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab3Control : UserControl
{
    public Tab3Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    public void Reset()
    {
        ScrollViewer1.ScrollToHome();
    }

    public void End()
    {
        ScrollViewer1.ScrollToEnd();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is SettingModel model && model.NowView == 1)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
