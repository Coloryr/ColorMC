using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab6Control : UserControl
{
    public Tab6Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        LockLoginList.CellPointerPressed += LockLoginList_CellPointerPressed;
    }

    private void LockLoginList_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }
        else
        {
            LongPressed.Pressed(() => Flyout((sender as Control)!));
        }
    }

    private void Flyout(Control control)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var model = (DataContext as SettingModel)!;
            if (model.LockSelect != null)
            {
                _ = new LockLoginFlyout(control, model.LockSelect);
            }
        });
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
        if (DataContext is SettingModel model && model.NowView == 4)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
