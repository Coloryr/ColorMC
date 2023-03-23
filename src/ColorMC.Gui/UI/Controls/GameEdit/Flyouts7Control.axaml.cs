using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Flyouts7Control : UserControl
{
    private SchematicDisplayObj Obj;
    private FlyoutBase FlyoutBase;
    private Tab12Control Con;
    public Flyouts7Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.Delete(Obj);
    }

    public void Set(FlyoutBase fb, SchematicDisplayObj obj, Tab12Control con)
    {
        Obj = obj;
        Con = con;
        FlyoutBase = fb;
    }
}

public class GameEditFlyout7 : PopupFlyoutBase
{
    private readonly SchematicDisplayObj Obj;
    private readonly Tab12Control Con;
    public GameEditFlyout7(Tab12Control con, SchematicDisplayObj obj)
    {
        Con = con;
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new Flyouts7Control();
        control.Set(this, Obj, Con);
        return control;
    }
}
