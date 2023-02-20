using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ColorMC.Gui.UI.Controls;

public partial class InfoFlyoutsControl : UserControl
{
    public InfoFlyoutsControl()
    {
        InitializeComponent();
    }

    public void Set(string text)
    {
        Label1.Content = text;
    }
}

public class InfoFlyout : FlyoutBase
{
    private string Text;
    public InfoFlyout(string text)
    {
        Text = text;
    }
    protected override Control CreatePresenter()
    {
        var control = new InfoFlyoutsControl();
        control.Set(Text);
        return control;
    }
}
