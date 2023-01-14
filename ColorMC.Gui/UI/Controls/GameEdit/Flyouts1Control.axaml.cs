using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Flyouts1Control : UserControl
{
    private ModDisplayObj Obj;
    private FlyoutBase FlyoutBase;
    private Tab4Control Con;
    public Flyouts1Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        GameBinding.OpFile(Obj.Local);
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.Delete(Obj);
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.DisE(Obj);
    }

    public void Set(FlyoutBase fb, ModDisplayObj obj, Tab4Control con)
    {
        Obj = obj;
        Con = con;
        FlyoutBase = fb;
    }
}

public class GameEditFlyout1 : FlyoutBase
{
    private ModDisplayObj Obj;
    private Tab4Control Con;
    public GameEditFlyout1(Tab4Control con, ModDisplayObj obj)
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
