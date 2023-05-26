using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class WorldControl : UserControl
{
    public static readonly StyledProperty<WorldModel> WorldModelProperty =
        AvaloniaProperty.Register<WorldControl, WorldModel>(nameof(WorldModel));

    public WorldModel WorldModel
    {
        get => GetValue(WorldModelProperty);
        set => SetValue(WorldModelProperty, value);
    }

    public WorldControl()
    {
        InitializeComponent();

        PointerPressed += WorldControl_PointerPressed;

        PointerEntered += WorldControl_PointerEntered;
        PointerExited += WorldControl_PointerExited;

        PropertyChanged += WorldControl_PropertyChanged;
    }

    private void WorldControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WorldModelProperty)
        {
            if (WorldModel == null)
                return;

            DataContext = WorldModel;
        }
    }

    private void WorldControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = false;
    }

    private void WorldControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = true;
    }

    private void WorldControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        WorldModel.Select();
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            WorldModel.Flyout(this);
        }
    }
}
