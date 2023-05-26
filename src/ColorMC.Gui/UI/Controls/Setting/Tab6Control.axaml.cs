using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab6Control : UserControl
{
    public Tab6Control()
    {
        InitializeComponent();
        TextBox4.LostFocus += TextBox4_LostFocus;
    }

    private void TextBox4_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TextBox4.Text))
            return;

        if (TextBox4.Text.EndsWith("/"))
            return;

        TextBox4.Text += "/";
    }
}
