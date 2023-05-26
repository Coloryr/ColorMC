using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class ResourcePackControl : UserControl
{
    public static readonly StyledProperty<ResourcePackModel> PackModelProperty =
        AvaloniaProperty.Register<ResourcePackControl, ResourcePackModel>(nameof(PackModel));

    public ResourcePackModel PackModel
    {
        get => GetValue(PackModelProperty);
        set => SetValue(PackModelProperty, value);
    }
    public ResourcePackControl()
    {
        InitializeComponent();

        PointerPressed += ResourcePackControl_PointerPressed;

        PointerEntered += ResourcePackControl_PointerEntered;
        PointerExited += ResourcePackControl_PointerExited;

        PropertyChanged += ResourcePackControl_PropertyChanged;
    }

    private void ResourcePackControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == PackModelProperty)
        {
            if (PackModel == null)
                return;

            DataContext = PackModel;
        }
    }

    private void ResourcePackControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = false;
    }

    private void ResourcePackControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = true;
    }

    private void ResourcePackControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PackModel.Select();
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            PackModel.Flyout(this);
        }
    }
}
