using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.UI.Controls;

public partial class HeadControl : UserControl
{
    public static readonly StyledProperty<object> TitleProperty =
            AvaloniaProperty.Register<Label, object>(nameof(Title), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> Title1Property =
            AvaloniaProperty.Register<Label, object?>(nameof(Title1), defaultBindingMode: BindingMode.TwoWay);

    private bool _min;
    private bool _clo;
    private bool _max;
    public object Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public object? Title1
    {
        get { return GetValue(Title1Property); }
        set { SetValue(Title1Property, value); }
    }

    public bool Max
    {
        get { return _max; }
        set
        {
            _max = value;
            _buttonMax.IsVisible = _max;
        }
    }

    public bool Min
    {
        get { return _min; }
        set
        {
            _min = value;
            _buttonMin.IsVisible = _min;
        }
    }

    public bool Clo
    {
        get { return _clo; }
        set
        {
            _clo = value;
            _buttonClose.IsVisible = _clo;
        }
    }

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
            StackPanel2.HorizontalAlignment = HorizontalAlignment.Center;
            StackPanel1.HorizontalAlignment = HorizontalAlignment.Left;
            StackPanel1.Children.Add(_buttonClose);
            StackPanel1.Children.Add(_buttonMin);
            StackPanel1.Children.Add(_buttonMax);
        }
        else
        {
            StackPanel1.Children.Add(_buttonMin);
            StackPanel1.Children.Add(_buttonMax);
            StackPanel1.Children.Add(_buttonClose);
        }

        _buttonMin.Click += ButtonMin_Click;
        _buttonMax.Click += ButtonMax_Click;
        _buttonClose.Click += ButtonClose_Click;
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
