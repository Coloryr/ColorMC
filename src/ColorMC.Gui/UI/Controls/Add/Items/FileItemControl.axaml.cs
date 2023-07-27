using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Controls.Add.Items;

public partial class FileItemControl : UserControl
{
    private FileItemModel _model;

    public FileItemControl()
    {
        InitializeComponent();

        PointerPressed += FileItemControl_PointerPressed;
        DoubleTapped += FileItemControl_DoubleTapped;
        PointerEntered += FileItemControl_PointerEntered;
        PointerExited += FileItemControl_PointerExited;

        DataContextChanged += FileItemControl_DataContextChanged;
    }

    private void FileItemControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is FileItemModel model)
        {
            _model = model;
        }
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
        _model.Install();
    }

    private void FileItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _model.SetSelect();

        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsRightButtonPressed)
        {
            var url = _model.Data?.GetUrl();
            if (url == null)
                return;

            _ = new UrlFlyout(this, url);
            e.Handled = true;
        }
        else if (ev.Properties.IsXButton1Pressed)
        {
            _model.Back();
            e.Handled = true;
        }
        else if (ev.Properties.IsXButton2Pressed)
        {
            _model.Next();
            e.Handled = true;
        }
    }
}
