using Avalonia;
using Avalonia.Controls;
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
    private readonly Button _buttonChoise;
    private readonly Button _buttonChoise1;

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
        _buttonChoise1 = new Button()
        {
            Width = 80,
            Height = 35,
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
            StackPanel1.Children.Add(_buttonChoise);
            StackPanel1.Children.Add(_buttonChoise1);
            StackPanel1.Children.Add(_buttonBack);
        }
        else
        {
            Icons.Margin = new Thickness(5, 0, 0, 0);
            StackPanel1.SetValue(DockPanel.DockProperty, Dock.Right);
            StackPanel1.Children.Add(_buttonChoise);
            StackPanel1.Children.Add(_buttonChoise1);
            StackPanel1.Children.Add(_buttonBack);
            StackPanel1.Children.Add(_buttonMin);
            StackPanel1.Children.Add(_buttonMax);
            StackPanel1.Children.Add(_buttonClose);
        }

        _buttonMin.Click += ButtonMin_Click;
        _buttonMax.Click += ButtonMax_Click;
        _buttonClose.Click += ButtonClose_Click;
        _buttonBack.Click += ButtonBack_Click;
        _buttonChoise.Click += ButtonChoise_Click;
        _buttonChoise1.Click += _buttonChoise1_Click;
    }

    private void HeadControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            _buttonMin.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonMax.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonClose.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonBack.Bind(IsVisibleProperty, model.HeadBackObservale.ToBinding());
            _buttonChoise.Bind(IsVisibleProperty, model.HeadChoiseObservale.ToBinding());
            _buttonChoise.Bind(ContentProperty, model.HeadChoiseContentObservale.ToBinding());
            _buttonClose.Bind(IsEnabledProperty, model.HeadCloseObservale.ToBinding());
            _buttonChoise1.Bind(IsVisibleProperty, model.HeadChoise1Observale.ToBinding());
            _buttonChoise1.Bind(ContentProperty, model.HeadChoise1ContentObservale.ToBinding());
        }
    }

    private void _buttonChoise1_Click(object? sender, RoutedEventArgs e)
    {
        (DataContext as BaseModel)?.Choise1Click();
    }

    private void ButtonChoise_Click(object? sender, RoutedEventArgs e)
    {
        (DataContext as BaseModel)?.ChoiseClick();
    }

    private void ButtonBack_Click(object? sender, RoutedEventArgs e)
    {
        (DataContext as BaseModel)?.BackClick();
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
        {
            window.WindowState = WindowState.Normal;
        }
        else
        {
            window.WindowState = WindowState.Maximized;
        }
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
