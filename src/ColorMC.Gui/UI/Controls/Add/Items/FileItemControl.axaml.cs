using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Add.Items;

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
    }

    private void FileItemControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void FileItemControl_PointerExited(object? sender, PointerEventArgs e)
    {
        (DataContext as FileItemModel)!.Top = false;
    }

    private void FileItemControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        (DataContext as FileItemModel)!.Top = true;
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
        model.SetSelect();

        void OpenFlyout()
        {
            var url = model.Data?.GetUrl();
            var url1 = model.Data?.GetMcMod();
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

        LongPressed.Pressed(() =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                OpenFlyout();
            });
        });
    }
}
