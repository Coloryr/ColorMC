using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace ColorMC.Gui.UI.Views;

public partial class ButtonControl : UserControl
{
    public static readonly StyledProperty<IBrush> ColorProperty =
        AvaloniaProperty.Register<ButtonControl, IBrush>(nameof(Color), Brush.Parse("#C1EAE5"));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<ButtonControl, string?>(nameof(Text));

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<ButtonControl, ICommand?>(nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
       AvaloniaProperty.Register<ButtonControl, object?>(nameof(CommandParameter));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// 背景色
    /// </summary>
    public IBrush Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    /// 显示文字
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ButtonControl()
    {
        InitializeComponent();

        InitColor();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        InitSize();

        return base.ArrangeOverride(finalSize);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ColorProperty)
        {
            InitColor();
        }
        else if (change.Property == TextProperty)
        {
            TextShow.Text = Text;
        }
        else if (change.Property == CommandProperty)
        {
            Button1.Command = Command;
        }
        else if (change.Property == CommandParameterProperty)
        {
            Button1.CommandParameter = CommandParameter;
        }
    }

    private void InitColor()
    {
        Color getColor = Colors.Transparent;
        if (Color is ISolidColorBrush color)
        {
            getColor = color.Color;

        }
        else if (Color is ImmutableSolidColorBrush color1)
        {
            getColor = color1.Color;
        }

        Button1.Background = Color;

        var hsv = getColor.ToHsv();
        var hsv1 = new HsvColor(hsv.A, hsv.H, hsv.S * 0.9, hsv.V * 0.66);
        var hsv2 = new HsvColor(hsv1.A, hsv1.H, hsv1.S * 0.9, hsv1.V * 0.9);
        var hsv3 = new HsvColor(hsv2.A, hsv2.H * 0.9, hsv2.S * 0.9, hsv2.V * 0.9);

        Left.Background = new SolidColorBrush(hsv1.ToRgb());
        Right.Background = new SolidColorBrush(hsv1.ToRgb());
        Top.Background = new SolidColorBrush(hsv2.ToRgb());
        Bottom.Background = new SolidColorBrush(hsv3.ToRgb());
        TextShow.Foreground = new SolidColorBrush(hsv3.ToRgb());
    }

    private void InitSize()
    {
        var h = Height * 0.12;

        Left.Width = h;
        Right.Width = h;
        Top.Height = h;
        Top.Margin = new Thickness(h, 0, 0, 0);
        Bottom.Height = h;
    }
}