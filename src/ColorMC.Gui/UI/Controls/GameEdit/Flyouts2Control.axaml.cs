using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Flyouts2Control : UserControl
{
    private WorldDisplayObj Obj;
    private FlyoutBase FlyoutBase;
    private Tab5Control Con;
    public Flyouts2Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.Backup(Obj);
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.Delete(Obj);
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.Export(Obj);
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        BaseBinding.OpPath(Obj.Local);
    }

    public void Set(FlyoutBase fb, WorldDisplayObj obj, Tab5Control con)
    {
        Obj = obj;
        Con = con;
        FlyoutBase = fb;
    }
}

public class GameEditFlyout2 : PopupFlyoutBase
{
    private readonly WorldDisplayObj Obj;
    private readonly Tab5Control Con;
    public GameEditFlyout2(Tab5Control con, WorldDisplayObj obj)
    {
        Con = con;
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new Flyouts2Control();
        control.Set(this, Obj, Con);
        return control;
    }
}
