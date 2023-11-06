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
    private readonly Button _buttonBack;
    private readonly Button _buttonDown;
    private readonly Button _buttonChoise;

    public HeadControl()
    {
        InitializeComponent();

        Border1.PointerPressed += HeadControl_PointerPressed;
        TitleShow.PointerPressed += HeadControl_PointerPressed;
        TitleShow1.PointerPressed += HeadControl_PointerPressed;

        DataContextChanged += HeadControl_DataContextChanged;

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
        _buttonChoise = new Button()
        {
            Width = 80,
            Height = 35,
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(0),
            IsVisible = false
        };
        _buttonDown = new Button()
        {
            Width = 80,
            Height = 35,
            Content = App.Lang("Gui.Info33"),
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(0),
            IsVisible = false
        };
        _buttonBack = new Button()
        {
            Width = 80,
            Height = 35,
            Content = App.Lang("Gui.Info31"),
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(0),
            IsVisible = false
        };

        if (SystemInfo.Os == OsType.MacOS)
        {
            StackPanel1.SetValue(DockPanel.DockProperty, Dock.Left);
            Icons.Margin = new Thickness(10, 0, 0, 0);
            StackPanel1.Children.Add(_buttonClose);
            StackPanel1.Children.Add(_buttonMin);
            StackPanel1.Children.Add(_buttonMax);
            StackPanel1.Children.Add(_buttonDown);
            StackPanel1.Children.Add(_buttonBack);
            StackPanel1.Children.Add(_buttonChoise);
        }
        else
        {
            Icons.Margin = new Thickness(5, 0, 0, 0);
            StackPanel1.SetValue(DockPanel.DockProperty, Dock.Right);
            StackPanel1.Children.Add(_buttonDown);
            StackPanel1.Children.Add(_buttonBack);
            StackPanel1.Children.Add(_buttonChoise);
            StackPanel1.Children.Add(_buttonMin);
            StackPanel1.Children.Add(_buttonMax);
            StackPanel1.Children.Add(_buttonClose);
        }

        _buttonMin.Click += ButtonMin_Click;
        _buttonMax.Click += ButtonMax_Click;
        _buttonClose.Click += ButtonClose_Click;
        _buttonBack.Click += ButtonBack_Click;
        _buttonDown.Click += ButtonDown_Click;
        _buttonChoise.Click += ButtonChoise_Click;
    }

    private void ButtonChoise_Click(object? sender, RoutedEventArgs e)
    {
        (DataContext as BaseModel)?.ChoiseClick?.Invoke();
    }

    private void ButtonDown_Click(object? sender, RoutedEventArgs e)
    {
        (DataContext as BaseModel)?.DownClick?.Invoke();
    }

    private void ButtonBack_Click(object? sender, RoutedEventArgs e)
    {
        (DataContext as BaseModel)?.BackClick?.Invoke();
    }

    private void HeadControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            _buttonMin.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonMax.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonClose.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonBack.Bind(IsVisibleProperty, model.HeadBackObservale.ToBinding());
            _buttonDown.Bind(IsVisibleProperty, model.HeadDownObservale.ToBinding());
            _buttonChoise.Bind(IsVisibleProperty, model.HeadChoiseObservale.ToBinding());
            _buttonChoise.Bind(ContentProperty, model.HeadChoiseContentObservale.ToBinding());
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
