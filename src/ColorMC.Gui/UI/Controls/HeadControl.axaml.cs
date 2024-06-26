using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;

namespace ColorMC.Gui.UI.Controls;

public partial class HeadControl : UserControl
{
    private readonly Button _buttonClose;
    private readonly Button _buttonMax;
    private readonly Button _buttonMin;

    private readonly SelfPublisher<string> MaxObservale = new();

    private readonly string[] MaxIcon =
    [
        "/Resource/Icon/Head/max1.svg",
        "/Resource/Icon/Head/max.svg"
    ];

    public HeadControl()
    {
        InitializeComponent();

        Border1.PointerPressed += HeadControl_PointerPressed;
        TitleShow.PointerPressed += HeadControl_PointerPressed;
        TitleShow.PointerPressed += HeadControl_PointerPressed;

        Border1.DoubleTapped += Border1_DoubleTapped;
        TitleShow.DoubleTapped += Border1_DoubleTapped;
        TitleShow.DoubleTapped += Border1_DoubleTapped;

        DataContextChanged += HeadControl_DataContextChanged;

        _buttonMin = new Button()
        {
            Width = 40,
            Height = 35,
            Content = new HeadImg() { Path = "/Resource/Icon/Head/min.svg" },
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(0),
            Foreground = Brushes.White
        };
        _buttonMin.Bind(BackgroundProperty, new ColorsExtension("Main"));
        _buttonMax = new Button()
        {
            Width = 40,
            Height = 35,
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(0),
            Foreground = Brushes.White
        };
        _buttonMax.Bind(BackgroundProperty, new ColorsExtension("Main"));
        var max = new HeadImg();
        max.Bind(HeadImg.PathProperty, MaxObservale.ToBinding());
        MaxObservale.Notify(MaxIcon[0]);
        _buttonMax.Content = max;
        _buttonClose = new Button()
        {
            Width = 40,
            Height = 35,
            Content = new HeadImg() { Path = "/Resource/Icon/Head/close.svg" },
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(0),
            Foreground = Brushes.White
        };
        _buttonClose.Bind(BackgroundProperty, new ColorsExtension("Main"));

        StackPanel1.Children.Add(_buttonMin);
        StackPanel1.Children.Add(_buttonMax);
        StackPanel1.Children.Add(_buttonClose);

        _buttonMin.Click += ButtonMin_Click;
        _buttonMax.Click += ButtonMax_Click;
        _buttonClose.Click += ButtonClose_Click;
    }

    private void Border1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        WindowMax();
    }

    private void HeadControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            _buttonMin.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonMax.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonClose.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonClose.Bind(IsEnabledProperty, model.HeadCloseObservale.ToBinding());
        }
    }

    private void ButtonClose_Click(object? sender, RoutedEventArgs e)
    {
        (VisualRoot as Window)?.Close();
    }

    private void ButtonMax_Click(object? sender, RoutedEventArgs e)
    {
        WindowMax();
    }

    private void WindowMax()
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

    public void WindowStateChange(WindowState state)
    {
        if (state == WindowState.Maximized)
        {
            MaxObservale.Notify(MaxIcon[1]);
        }
        else
        {
            MaxObservale.Notify(MaxIcon[0]);
        }
    }
}
