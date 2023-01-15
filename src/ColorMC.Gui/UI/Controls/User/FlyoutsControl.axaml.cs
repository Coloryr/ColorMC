using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.User;

public partial class FlyoutsControl : UserControl
{
    private UserDisplayObj Obj;
    private FlyoutBase FlyoutBase;
    private UserWindow Win;
    public FlyoutsControl()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button3.Click += Button3_Click;
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        UserBinding.Remove(Obj.UUID, Obj.AuthType);
        Win.Load();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        UserBinding.SetLastUser(Obj.UUID, Obj.AuthType);
        Win.Load();
    }

    public void Set(FlyoutBase fb, UserDisplayObj obj, UserWindow win)
    {
        Win = win;
        Obj = obj;
        FlyoutBase = fb;
    }
}

public class UserFlyout : FlyoutBase
{
    private UserDisplayObj Obj;
    private UserWindow Win;
    public UserFlyout(UserWindow win, UserDisplayObj obj)
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
