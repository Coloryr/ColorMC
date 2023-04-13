using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab7Control : UserControl
{
    public Tab7Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;

        Label1.Content = ColorMCCore.Version;

        Image1.Source = App.GameIcon;
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpUrl("https://coloryr.github.io/sponsor.html");
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpUrl("https://www.minecraft.net/");
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpUrl("https://www.github.com/Coloryr/ColorMC");
    }
}
