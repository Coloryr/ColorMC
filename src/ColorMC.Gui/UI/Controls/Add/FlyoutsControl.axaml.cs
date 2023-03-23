using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class FlyoutsControl : UserControl
{
    private string Url;
    private FlyoutBase FlyoutBase;
    public FlyoutsControl()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        BaseBinding.OpUrl(Url);
    }

    public void Set(FlyoutBase fb, string url)
    {
        Url = url;
        FlyoutBase = fb;
    }
}

public class UrlFlyout : PopupFlyoutBase
{
    private string Url;
    public UrlFlyout(string url)
    {
        Url = url;
    }
    protected override Control CreatePresenter()
    {
        var control = new FlyoutsControl();
        control.Set(this, Url);
        return control;
    }
}
