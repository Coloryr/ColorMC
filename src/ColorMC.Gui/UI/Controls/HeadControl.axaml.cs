using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System;

namespace ColorMC.Gui.UI.Controls;

public partial class HeadControl : UserControl
{
    private Button _buttonClose;
    private Button _buttonMax;
    private Button _buttonMin;

    public HeadControl()
    {
        InitializeComponent();

        DataContext = this;

        Border1.PointerPressed += HeadControl_PointerPressed;
        TitleShow.PointerPressed += HeadControl_PointerPressed;
        TitleShow1.PointerPressed += HeadControl_PointerPressed;

        _buttonMin = new Button()
        {
            Width = 24,
            Height = 24,
            Margin = new Thickness(3, 0, 3, 0),
            Content = "-",
        };

        _buttonMax = new Button()
        {
            Width = 24,
            Height = 24,
            Margin = new Thickness(3, 0, 3, 0),
            Content = "▢",
        };

        _buttonClose = new Button()
        {
            Width = 24,
            Height = 24,
            Margin = new Thickness(3, 0, 3, 0),
            Content = "X",
        };

        if (SystemInfo.Os == OsType.MacOS)
        {
            StackPanel1.SetValue(DockPanel.DockProperty, Dock.Left);
            Icons.Margin = new Thickness(10, 5, 0, 5);
            StackPanel1.Children.Add(_buttonClose);
            StackPanel1.Children.Add(_buttonMin);
            StackPanel1.Children.Add(_buttonMax);
        }
        else
        {
            Icons.Margin = new Thickness(0, 5, 0, 5);
            StackPanel1.SetValue(DockPanel.DockProperty, Dock.Right);
            StackPanel1.Children.Add(_buttonMin);
            StackPanel1.Children.Add(_buttonMax);
            StackPanel1.Children.Add(_buttonClose);
        }

        _buttonMin.Click += ButtonMin_Click;
        _buttonMax.Click += ButtonMax_Click;
        _buttonClose.Click += ButtonClose_Click;
    }

    public void Display(bool value)
    {
        _buttonMin.IsVisible = value;
        _buttonMax.IsVisible = value;
        _buttonClose.IsVisible = value;
    }

    private void ButtonClose_Click(object? sender, RoutedEventArgs e)
    {
        (VisualRoot as Window)?.Close();
    }

    private void ButtonMax_Click(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
        {
            return;
        }

        if (window.WindowState == WindowState.Maximized)
            window.WindowState = WindowState.Normal;
        else
            window.WindowState = WindowState.Maximized;
    }

    private void ButtonMin_Click(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
        {
            return;
        }

        window.WindowState = WindowState.Minimized;
    }

    private void HeadControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (VisualRoot as Window)?.BeginMoveDrag(e);
    }
}
