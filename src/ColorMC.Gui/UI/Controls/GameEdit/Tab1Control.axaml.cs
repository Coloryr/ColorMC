using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab1Control : UserControl
{
    public Tab1Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        ComboBox1.PointerPressed += ComboBox1_PointerPressed;
    }

    private async void ComboBox1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is GameEditModel model)
        {
            await model.LangLoad();
        }
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
        if (DataContext is GameEditModel model && model.NowView == 0)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
