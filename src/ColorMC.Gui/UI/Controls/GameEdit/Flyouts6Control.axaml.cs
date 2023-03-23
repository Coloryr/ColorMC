using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Flyouts6Control : UserControl
{
    private ShaderpackDisplayObj Obj;
    private FlyoutBase FlyoutBase;
    private Tab11Control Con;
    public Flyouts6Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.Delete(Obj);
    }

    public void Set(FlyoutBase fb, ShaderpackDisplayObj obj, Tab11Control con)
    {
        Obj = obj;
        Con = con;
        FlyoutBase = fb;
    }
}

public class GameEditFlyout6 : PopupFlyoutBase
{
    private readonly ShaderpackDisplayObj Obj;
    private readonly Tab11Control Con;
    public GameEditFlyout6(Tab11Control con, ShaderpackDisplayObj obj)
    {
        Con = con;
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new Flyouts6Control();
        control.Set(this, Obj, Con);
        return control;
    }
}
