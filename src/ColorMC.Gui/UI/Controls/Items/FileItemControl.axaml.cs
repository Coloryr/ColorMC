using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Items;

public partial class FileItemControl : UserControl
{
    public FileItemControl()
    {
        InitializeComponent();

        PointerPressed += FileItemControl_PointerPressed;
        PointerReleased += FileItemControl_PointerReleased;
        DoubleTapped += FileItemControl_DoubleTapped;
        PointerEntered += FileItemControl_PointerEntered;
        PointerExited += FileItemControl_PointerExited;
        PointerMoved += FileItemControl_PointerMoved;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (GuiConfigUtils.Config.Style.EnableAm)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ItemAnimation.Make().RunAsync(this);
            });
        }
    }

    private void FileItemControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        LongPressed.Cancel();
    }

    private void FileItemControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void FileItemControl_PointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is FileItemModel model)
        {
            model.Top = false;
        }
    }

    private void FileItemControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is FileItemModel model)
        {
            model.Top = true;
        }
    }

    private void FileItemControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        (DataContext as FileItemModel)?.Install();
    }

    private void FileItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not FileItemModel model)
        {
            return;
        }
        Focus();
        model.SetSelect();

        void OpenFlyout()
        {
            var url = model.Url;
            var url1 = model.McMod?.GetUrl();
            if (url == null && url1 == null)
            {
                return;
            }

            _ = new UrlFlyout((sender as Control)!, url, url1);
            e.Handled = true;
        }

        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsRightButtonPressed)
        {
            OpenFlyout();
        }
        else if (ev.Properties.IsXButton1Pressed)
        {
            model.Back();
            e.Handled = true;
        }
        else if (ev.Properties.IsXButton2Pressed)
        {
            model.Next();
            e.Handled = true;
        }
        else
        {
            LongPressed.Pressed(OpenFlyout);
        }
    }
}
