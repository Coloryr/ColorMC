using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;

namespace ColorMC.Gui.UI.Controls;

public partial class HeadControl : UserControl
{
    public static readonly StyledProperty<object> TitleProperty =
            AvaloniaProperty.Register<ContentControl, object>(nameof(Title), defaultBindingMode: BindingMode.TwoWay);

    private object title;
    private bool min;
    private bool max;
    public object Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
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

    private Window? window;

    public HeadControl()
    {
        InitializeComponent();

        PointerPressed += HeadControl_PointerPressed;

        Initialized += HeadControl_Initialized;

        Button_Min.Click += ButtonMin_Click;
        Button_Max.Click += ButtonMax_Click;
        Button_Close.Click += ButtonClose_Click;
    }

    private void ButtonClose_Click(object? sender, RoutedEventArgs e)
    {
        if (window == null)
            return;

        window.Close();
    }

    private void ButtonMax_Click(object? sender, RoutedEventArgs e)
    {
        if (window == null)
            return;

        if (window.WindowState == WindowState.Maximized)
            window.WindowState = WindowState.Normal;
        else
            window.WindowState = WindowState.Maximized;
    }

    private void ButtonMin_Click(object? sender, RoutedEventArgs e)
    {
        if (window == null)
            return;

        window.WindowState = WindowState.Minimized;
    }

    private void HeadControl_Initialized(object? sender, EventArgs e)
    {
        var control = Parent;
        do
        {
            if (control is Window)
            {
                window = control as Window;
                break;
            }
            else
            {
                control = control?.Parent;
            }
        } while (control != null);
    }

    private void HeadControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (window == null)
            return;
        window.BeginMoveDrag(e);
    }
}
