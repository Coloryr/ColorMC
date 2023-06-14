using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class Live2dControl : UserControl
{
    public Live2dControl()
    {
        InitializeComponent();

        Live2dTop.PointerPressed += Live2dTop_PointerPressed;
        Live2dTop.PointerReleased += Live2dTop_PointerReleased;
        Live2dTop.PointerMoved += Live2dTop_PointerMoved;
    }

    private void Live2dTop_PointerMoved(object? sender, PointerEventArgs e)
    {
        var pro = e.GetCurrentPoint(this);
        if (pro.Properties.IsLeftButtonPressed)
            Live2d.Moved((float)pro.Position.X, (float)pro.Position.Y);
    }

    private void Live2dTop_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Live2d.Release();
    }

    private void Live2dTop_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pro = e.GetCurrentPoint(this);
        if (pro.Properties.IsLeftButtonPressed)
        {
            Live2d.Pressed();
            Live2d.Moved((float)pro.Position.X, (float)pro.Position.Y);
        }
    }
}
