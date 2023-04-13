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

    public static readonly StyledProperty<object> Title1Property =
            AvaloniaProperty.Register<Label, object>(nameof(Title1), defaultBindingMode: BindingMode.TwoWay);

    private bool min;
    private bool clo;
    private bool max;
    public object Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public object Title1
    {
        get { return GetValue(Title1Property); }
        set { SetValue(Title1Property, value); }
    }

    public bool Max
    {
        get { return max; }
        set
        {
            max = value;
            Button_Max.IsVisible = max;
        }
    }

    public bool Min
    {
        get { return min; }
        set
        {
            min = value;
            Button_Min.IsVisible = min;
        }
    }

    public bool Clo
    {
        get { return clo; }
        set
        {
            clo = value;
            Button_Close.IsVisible = clo;
        }
    }

    private Button Button_Close;
    private Button Button_Max;
    private Button Button_Min;

    public HeadControl()
    {
        InitializeComponent();

        DataContext = this;

        Border1.PointerPressed += HeadControl_PointerPressed;
        TitleShow.PointerPressed += HeadControl_PointerPressed;
        TitleShow1.PointerPressed += HeadControl_PointerPressed;

        Button_Min = new Button()
        {
            Width = 24,
            Height = 24,
            Margin = new Thickness(3, 0, 3, 0),
            Content = "-",
        };

        Button_Max = new Button()
        {
            Width = 24,
            Height = 24,
            Margin = new Thickness(3, 0, 3, 0),
            Content = "▢",
        };

        Button_Close = new Button()
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
            StackPanel1.Children.Add(Button_Close);
            StackPanel1.Children.Add(Button_Min);
            StackPanel1.Children.Add(Button_Max);
        }
        else
        {
            StackPanel1.Children.Add(Button_Min);
            StackPanel1.Children.Add(Button_Max);
            StackPanel1.Children.Add(Button_Close);
        }

        Button_Min.Click += ButtonMin_Click;
        Button_Max.Click += ButtonMax_Click;
        Button_Close.Click += ButtonClose_Click;
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
