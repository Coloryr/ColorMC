using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Flyouts3Control : UserControl
{
    private ResourcepackDisplayObj Obj;
    private FlyoutBase FlyoutBase;
    private Tab8Control Con;
    public Flyouts3Control()
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

    public void Set(FlyoutBase fb, ResourcepackDisplayObj obj, Tab8Control con)
    {
        Obj = obj;
        Con = con;
        FlyoutBase = fb;
    }
}

public class GameEditFlyout3 : PopupFlyoutBase
{
    private readonly ResourcepackDisplayObj Obj;
    private readonly Tab8Control Con;
    public GameEditFlyout3(Tab8Control con, ResourcepackDisplayObj obj)
    {
        Con = con;
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new Flyouts3Control();
        control.Set(this, Obj, Con);
        return control;
    }
}
