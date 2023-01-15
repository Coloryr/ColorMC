using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;
using System.Collections.ObjectModel;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils.LaunchSetting;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class Tab3Control : UserControl
{
    private HelloWindow Window;

    private readonly ObservableCollection<UserDisplayObj1> List = new();
    public Tab3Control()
    {
        InitializeComponent();
        List_User.Items = List;

        Button_Add.Click += Button_Add_Click;
        Button_Next.Click += Button_Next_Click;
        Button_Delete.Click += Button_Delete_Click;
        Button_Refash.Click += Button_Refash_Click;

        ComboBox_UserType.SelectionChanged += UserType_SelectionChanged;
        ComboBox_UserType.Items = UserBinding.GetUserTypes();

        Load();
    }

    private void Button_Delete_Click(object? sender, RoutedEventArgs e)
    {
        var item = List_User.SelectedItem as UserDisplayObj1;
        if (item == null)
            return;

        UserBinding.Remove(item.Name, item.Type);
        Load();
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        Window.Next();
    }

    private async void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        Button_Add.IsEnabled = false;
        switch (ComboBox_UserType.SelectedIndex)
        {
            case 0:
                string name = TextBox_Input1.Text;
                if (string.IsNullOrWhiteSpace(name))
                {
                    Window.Info.Show(Localizer.Instance["UserWindow.Error2"]);
                    break;
                }
                var res = await UserBinding.AddUser(0, name, null);
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                }
                Window.Info2.Show(Localizer.Instance["UserWindow.Ok2"]);
                TextBox_Input1.Text = "";
                break;
            case 1:
                CoreMain.LoginOAuthCode = LoginOAuthCode;
                Window.Info1.Show(Localizer.Instance["UserWindow.Info1"]);
                res = await UserBinding.AddUser(1, null);
                Window.Info3.Close();
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                }
                Window.Info2.Show(Localizer.Instance["UserWindow.Ok2"]);
                TextBox_Input1.Text = "";
                break;
            case 2:
                var server = TextBox_Input1.Text;
                if (server.Length != 32)
                {
                    Window.Info.Show(Localizer.Instance["UserWindow.Error3"]);
                    break;
                }
                await Window.Info3.Show(Localizer.Instance["UserWindow.Text1"],
                    Localizer.Instance["UserWindow.Text2"], false);
                Window.Info3.Close();
                if (Window.Info3.Cancel)
                {
                    break;
                }
                var user = Window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Window.Info.Show(Localizer.Instance["UserWindow.Error2"]);
                    break;
                }
                Window.Info1.Show(Localizer.Instance["UserWindow.Info2"]);
                res = await UserBinding.AddUser(2, server, user.Item1, user.Item2);
                Window.Info1.Close();
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                    break;
                }
                Window.Info2.Show(Localizer.Instance["UserWindow.Ok2"]);
                TextBox_Input1.Text = "";
                break;
            case 3:
                server = TextBox_Input1.Text;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Window.Info.Show(Localizer.Instance["UserWindow.Error4"]);
                    break;
                }
                await Window.Info3.Show(Localizer.Instance["UserWindow.Text1"],
                    Localizer.Instance["UserWindow.Text2"], false);
                Window.Info3.Close();
                if (Window.Info3.Cancel)
                {
                    break;
                }
                user = Window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Window.Info.Show(Localizer.Instance["UserWindow.Error2"]);
                    break;
                }
                Window.Info1.Show(Localizer.Instance["UserWindow.Info2"]);
                res = await UserBinding.AddUser(3, server, user.Item1, user.Item2);
                Window.Info1.Close();
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                    break;
                }
                Window.Info2.Show(Localizer.Instance["UserWindow.Ok2"]);
                TextBox_Input1.Text = "";
                break;
            case 4:
                await Window.Info3.Show(Localizer.Instance["UserWindow.Text1"],
                    Localizer.Instance["UserWindow.Text2"], false);
                Window.Info3.Close();
                if (Window.Info3.Cancel)
                {
                    break;
                }
                user = Window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Window.Info.Show(Localizer.Instance["UserWindow.Error2"]);
                    break;
                }
                Window.Info1.Show(Localizer.Instance["UserWindow.Info2"]);
                res = await UserBinding.AddUser(4, user.Item1, user.Item2);
                Window.Info1.Close();
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                    break;
                }
                Window.Info2.Show(Localizer.Instance["UserWindow.Ok2"]);
                break;
            case 5:
                server = TextBox_Input1.Text;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Window.Info.Show(Localizer.Instance["UserWindow.Error4"]);
                    break;
                }
                await Window.Info3.Show(Localizer.Instance["UserWindow.Text1"],
                    Localizer.Instance["UserWindow.Text2"], false);
                Window.Info3.Close();
                if (Window.Info3.Cancel)
                {
                    break;
                }
                user = Window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Window.Info.Show(Localizer.Instance["UserWindow.Error2"]);
                    break;
                }
                Window.Info1.Show(Localizer.Instance["UserWindow.Info2"]);
                res = await UserBinding.AddUser(3, server, user.Item1, user.Item2);
                Window.Info1.Close();
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                    break;
                }
                Window.Info2.Show(Localizer.Instance["UserWindow.Ok2"]);
                TextBox_Input1.Text = "";
                break;
            default:
                Window.Info.Show(Localizer.Instance["UserWindow.Error5"]);
                break;
        }
        Load();
        Button_Add.IsEnabled = true;
    }

    private void LoginOAuthCode(string url, string code)
    {
        Window.Info1.Close();
        Window.Info3.Show(string.Format(Localizer.Instance["UserWindow.Text3"], url),
            string.Format(Localizer.Instance["UserWindow.Text4"], code));
    }

    private void UserType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (ComboBox_UserType.SelectedIndex)
        {
            case 0:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = Localizer.Instance["UserWindow.Text5"];
                TextBox_Input1.Text = "";
                break;
            case 1:
                TextBox_Input1.IsEnabled = false;
                TextBox_Input1.Watermark = "";
                TextBox_Input1.Text = "";
                break;
            case 2:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = Localizer.Instance["UserWindow.Text6"];
                TextBox_Input1.Text = "";
                break;
            case 3:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = Localizer.Instance["UserWindow.Text7"];
                TextBox_Input1.Text = "";
                break;
            case 4:
                TextBox_Input1.IsEnabled = false;
                TextBox_Input1.Watermark = "";
                TextBox_Input1.Text = "";
                break;
            case 5:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = Localizer.Instance["UserWindow.Text8"];
                TextBox_Input1.Text = "";
                break;
        }
    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }

    private void Button_Refash_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    public void Load()
    {
        List.Clear();
        foreach (var item in UserBinding.GetAllUser())
        {
            List.Add(new()
            {
                Name = item.Key.Item1,
                Info = item.Value.UserName + " " + item.Key.Item2.GetName(),
                Type = item.Key.Item2
            });
        }
    }
}
