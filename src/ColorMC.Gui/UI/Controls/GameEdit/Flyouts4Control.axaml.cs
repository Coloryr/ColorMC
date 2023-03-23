using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Flyouts4Control : UserControl
{
    private ScreenshotDisplayObj Obj;
    private FlyoutBase FlyoutBase;
    private Tab9Control Con;
    public Flyouts4Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.Delete(Obj);
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        BaseBinding.OpFile(Obj.Local);
    }

    public void Set(FlyoutBase fb, ScreenshotDisplayObj obj, Tab9Control con)
    {
        Obj = obj;
        Con = con;
        FlyoutBase = fb;
    }
}

public class GameEditFlyout4 : PopupFlyoutBase
{
    private readonly ScreenshotDisplayObj Obj;
    private readonly Tab9Control Con;
    public GameEditFlyout4(Tab9Control con, ScreenshotDisplayObj obj)
    {
        Con = con;
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new Flyouts4Control();
        control.Set(this, Obj, Con);
        return control;
    }
}
