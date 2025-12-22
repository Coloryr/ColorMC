using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// 文件版本子项目
/// </summary>
public partial class FileVersionItemControl : UserControl
{
    public FileVersionItemControl()
    {
        InitializeComponent();

        PointerPressed += FileItemControl_PointerPressed;
        DoubleTapped += FileItemControl_DoubleTapped;
        PointerEntered += FileItemControl_PointerEntered;
        PointerExited += FileItemControl_PointerExited;
    }

    private void FileItemControl_PointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is FileVersionItemModel model)
        {
            model.Top = false;
        }
    }

    private void FileItemControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is FileVersionItemModel model)
        {
            model.Top = true;
        }
    }

    private void FileItemControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        (DataContext as FileVersionItemModel)?.Install();
    }

    private void FileItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not FileVersionItemModel model)
        {
            return;
        }
        model.SetSelect();

        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
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