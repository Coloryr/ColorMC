using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class FlyoutsControl : UserControl
{
    private JavaDisplayObj Obj;
    private FlyoutBase FlyoutBase;
    private Tab5Control Win;
    public FlyoutsControl()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();

        JavaBinding.RemoveJava(Obj.Name);
        Win.Load();
    }

    public void Set(FlyoutBase fb, JavaDisplayObj obj, Tab5Control win)
    {
        Win = win;
        Obj = obj;
        FlyoutBase = fb;
    }
}

public class SettingFlyout : PopupFlyoutBase
{
    private JavaDisplayObj Obj;
    private Tab5Control Win;
    public SettingFlyout(Tab5Control win, JavaDisplayObj obj)
    {
        Win = win;
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new FlyoutsControl();
        control.Set(this, Obj, Win);
        return control;
    }
}
