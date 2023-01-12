using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using Avalonia.Interactivity;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.User;

public partial class FlyoutsControl : UserControl
{
    private UserDisplayObj Obj;
    private FlyoutBase FlyoutBase;
    public FlyoutsControl()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        UserBinding.Remove(Obj.UUID, Obj.AuthType);
        MainWindow.OnUserEdit();
        App.UserWindow?.Load();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        App.UserWindow?.SetAdd();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        UserBinding.SetLastUser(Obj.UUID, Obj.AuthType);
        MainWindow.OnUserEdit();

        App.UserWindow?.Load();
    }

    public void Set(FlyoutBase fb, UserDisplayObj obj)
    {
        Obj = obj;
        FlyoutBase = fb;
    }
}

public class UserFlyout : FlyoutBase
{
    private UserDisplayObj Obj;
    public UserFlyout(UserDisplayObj obj)
    {
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new FlyoutsControl();
        control.Set(this, Obj);
        return control;
    }
}
