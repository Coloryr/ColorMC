using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.UI.Controls.Server;

public partial class Flyouts1Control : UserControl
{
    private ServerPackConfigDisplayObj Obj;
    private FlyoutBase FlyoutBase;
    private Tab4Control Con;
    public Flyouts1Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.Delete(Obj);
    }

    public void Set(FlyoutBase fb, ServerPackConfigDisplayObj obj, Tab4Control con)
    {
        Obj = obj;
        Con = con;
        FlyoutBase = fb;
    }
}

public class ServerPackFlyout1 : PopupFlyoutBase
{
    private readonly ServerPackConfigDisplayObj Obj;
    private readonly Tab4Control Con;
    public ServerPackFlyout1(Tab4Control con, ServerPackConfigDisplayObj obj)
    {
        Con = con;
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new Flyouts1Control();
        control.Set(this, Obj, Con);
        return control;
    }
}
