using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class ScreenshotControl : UserControl
{
    public static readonly StyledProperty<ScreenshotModel> ScreenshotModelProperty =
        AvaloniaProperty.Register<ScreenshotControl, ScreenshotModel>(nameof(ScreenshotModel));

    public ScreenshotModel ScreenshotModel
    {
        get => GetValue(ScreenshotModelProperty);
        set => SetValue(ScreenshotModelProperty, value);
    }
    public ScreenshotControl()
    {
        InitializeComponent();

        PointerPressed += ScreenshotControl_PointerPressed;

        PointerEntered += ScreenshotControl_PointerEntered;
        PointerExited += ScreenshotControl_PointerExited;

        PropertyChanged += ScreenshotControl_PropertyChanged;
    }

    private void ScreenshotControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = false;
    }

    private void ScreenshotControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = true;
    }

    private void ScreenshotControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ScreenshotModelProperty)
        {
            if (ScreenshotModel == null)
                return;

            DataContext = ScreenshotModel;
        }
    }

    private void ScreenshotControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ScreenshotModel.Select();
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            ScreenshotModel.Flyout(this);
        }
    }
}
