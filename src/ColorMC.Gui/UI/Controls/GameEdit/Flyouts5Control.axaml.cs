using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Flyouts5Control : UserControl
{
    private ServerInfoObj Obj;
    private FlyoutBase FlyoutBase;
    private Tab10Control Con;
    public Flyouts5Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        GameBinding.CopyServer(Obj);
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.Delete(Obj);
    }

    public void Set(FlyoutBase fb, ServerInfoObj obj, Tab10Control con)
    {
        Obj = obj;
        Con = con;
        FlyoutBase = fb;
    }
}

public class GameEditFlyout5 : PopupFlyoutBase
{
    private readonly ServerInfoObj Obj;
    private readonly Tab10Control Con;
    public GameEditFlyout5(Tab10Control con, ServerInfoObj obj)
    {
        Con = con;
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new Flyouts5Control();
        control.Set(this, Obj, Con);
        return control;
    }
}
