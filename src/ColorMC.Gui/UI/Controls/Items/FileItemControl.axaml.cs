using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// 项目列表子项目
/// </summary>
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
        (DataContext as FileItemModel)?.Show();
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

            UrlFlyout.Show((sender as Control)!, url, url1);
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
    }
}
