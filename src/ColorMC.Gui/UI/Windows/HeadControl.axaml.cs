using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model;
using System;

namespace ColorMC.Gui.UI.Windows;

public partial class HeadControl : UserControl
{
    private readonly Button _buttonClose;
    private readonly Button _buttonMax;
    private readonly Button _buttonMin;

    public HeadControl()
    {
        InitializeComponent();

        Border1.PointerPressed += HeadControl_PointerPressed;
        TitleShow.PointerPressed += HeadControl_PointerPressed;
        TitleShow1.PointerPressed += HeadControl_PointerPressed;

        _buttonMin = new Button()
        {
            Width = 40,
            Height = 35,
            Content = new HeadImg() { Path = "/Resource/Icon/Head/min.svg" },
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(0)
        };
        _buttonMax = new Button()
        {
            Width = 40,
            Height = 35,
            Content = new HeadImg() { Path = "/Resource/Icon/Head/max.svg" },
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(0)
        };
        _buttonClose = new Button()
        {
            Width = 40,
            Height = 35,
            Content = new HeadImg() { Path = "/Resource/Icon/Head/close.svg" },
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(0)
        };

        if (SystemInfo.Os == OsType.MacOS)
        {
            StackPanel1.SetValue(DockPanel.DockProperty, Dock.Left);
            Icons.Margin = new Thickness(10, 0, 0, 0);
            StackPanel1.Children.Add(_buttonClose);
            StackPanel1.Children.Add(_buttonMin);
            StackPanel1.Children.Add(_buttonMax);
        }
        else
        {
            Icons.Margin = new Thickness(5, 0, 0, 0);
            StackPanel1.SetValue(DockPanel.DockProperty, Dock.Right);
            StackPanel1.Children.Add(_buttonMin);
            StackPanel1.Children.Add(_buttonMax);
            StackPanel1.Children.Add(_buttonClose);
        }

        _buttonMin.Click += ButtonMin_Click;
        _buttonMax.Click += ButtonMax_Click;
        _buttonClose.Click += ButtonClose_Click;

        DataContextChanged += HeadControl_DataContextChanged;
    }

    private void HeadControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            _buttonMin.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonMax.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonClose.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
        }
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
