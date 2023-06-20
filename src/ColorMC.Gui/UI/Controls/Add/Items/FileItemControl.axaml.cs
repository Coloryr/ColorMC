using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Controls.Add.Items;

public partial class FileItemControl : UserControl
{
    public FileItemControl()
    {
        InitializeComponent();

        PointerPressed += FileItemControl_PointerPressed;
        DoubleTapped += FileItemControl_DoubleTapped;
        PointerEntered += FileItemControl_PointerEntered;
        PointerExited += FileItemControl_PointerExited;
    }

    private void FileItemControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = false;
    }

    private void FileItemControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = true;
    }


    private void FileItemControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is FileItemModel item)
        {
            var window = App.FindRoot(VisualRoot);
            (window.Con as IAddWindow)?.Install(item);
        }
    }

    private void FileItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is FileItemModel item)
        {
            var window = App.FindRoot(VisualRoot);
            (window.Con as IAddWindow)?.SetSelect(item);

            var ev = e.GetCurrentPoint(this);
            if (ev.Properties.IsRightButtonPressed)
            {
                var url = item.Data?.GetUrl();
                if (url == null)
                    return;

                _ = new UrlFlyout(this, url);
                e.Handled = true;
            }
            else if (ev.Properties.IsXButton1Pressed)
            {
                (window.Con as IAddWindow)?.Back();
                e.Handled = true;
            }
            else if (ev.Properties.IsXButton2Pressed)
            {
                (window.Con as IAddWindow)?.Next();
                e.Handled = true;
            }
        }
    }
}
