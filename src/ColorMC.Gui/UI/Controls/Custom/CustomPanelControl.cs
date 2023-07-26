using Avalonia;
using Avalonia.Controls;

namespace ColorMC.Gui.UI.Controls.Custom;

public class CustomPanelControl : UserControl
{
    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<CustomPanelControl, string>(nameof(Title));

    public string Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }
}
