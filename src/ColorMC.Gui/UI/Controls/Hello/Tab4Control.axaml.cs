using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class Tab4Control : UserControl
{
    private bool Cancel;

    private readonly ObservableCollection<UserDisplayObj1> List = new();
    public Tab4Control()
    {
        InitializeComponent();
        List_User.ItemsSource = List;

        Button_Add.Click += Button_Add_Click;
        Button_Next.Click += Button_Next_Click;
        Button_Delete.Click += Button_Delete_Click;
        Button_Refash.Click += Button_Refash_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox1.ItemsSource = UserBinding.GetUserTypes();

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
        var window = App.FindRoot(VisualRoot);
        (window.Con as HelloControl)?.Next();
    }

    private async void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        Button_Add.IsEnabled = false;
        switch (ComboBox1.SelectedIndex)
        {
            case 0:
                var name = TextBox_Input1.Text;
                if (string.IsNullOrWhiteSpace(name))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error2"));
                    break;
                }
                var res = await UserBinding.AddUser(AuthType.Offline, name, null);
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                TextBox_Input1.Text = "";
                break;
            case 1:
                Cancel = false;
                ColorMCCore.LoginOAuthCode = LoginOAuthCode;
                window.Info1.Show(App.GetLanguage("UserWindow.Info1"));
                res = await UserBinding.AddUser(AuthType.OAuth, null);
                window.Info3.Close();
                window.Info1.Close();
                if (Cancel)
                    break;
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                break;
            case 2:
                var server = TextBox_Input1.Text;
                if (server?.Length != 32)
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error3"));
                    break;
                }
                await window.Info3.ShowInput(App.GetLanguage("UserWindow.Text1"),
                    App.GetLanguage("UserWindow.Text2"), true);
                if (window.Info3.Cancel)
                {
                    break;
                }
                var user = window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error2"));
                    break;
                }
                window.Info1.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.Nide8, server, user.Item1, user.Item2);
                window.Info1.Close();
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                TextBox_Input1.Text = "";
                break;
            case 3:
                server = TextBox_Input1.Text;
                if (string.IsNullOrWhiteSpace(server))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error4"));
                    break;
                }
                await window.Info3.ShowInput(App.GetLanguage("UserWindow.Text1"),
                    App.GetLanguage("UserWindow.Text2"), true);
                if (window.Info3.Cancel)
                {
                    break;
                }
                user = window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error2"));
                    break;
                }
                window.Info1.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.AuthlibInjector, server, user.Item1, user.Item2);
                window.Info1.Close();
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                TextBox_Input1.Text = "";
                break;
            case 4:
                await window.Info3.ShowInput(App.GetLanguage("UserWindow.Text1"),
                    App.GetLanguage("UserWindow.Text2"), true);
                if (window.Info3.Cancel)
                {
                    break;
                }
                user = window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error2"));
                    break;
                }
                window.Info1.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.LittleSkin, user.Item1, user.Item2);
                window.Info1.Close();
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                break;
            case 5:
                server = TextBox_Input1.Text;
                if (string.IsNullOrWhiteSpace(server))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error4"));
                    break;
                }
                await window.Info3.ShowInput(App.GetLanguage("UserWindow.Text1"),
                    App.GetLanguage("UserWindow.Text2"), true);
                if (window.Info3.Cancel)
                {
                    break;
                }
                user = window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error2"));
                    break;
                }
                window.Info1.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.SelfLittleSkin, user.Item1, user.Item2, server);
                window.Info1.Close();
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                TextBox_Input1.Text = "";
                break;
            default:
                window.Info.Show(App.GetLanguage("UserWindow.Error5"));
                break;
        }
        UserBinding.UserLastUser();

        Load();
        Button_Add.IsEnabled = true;
    }

    private async void LoginOAuthCode(string url, string code)
    {
        var window = App.FindRoot(VisualRoot);
        window.Info1.Close();
        window.Info3.Show(string.Format(App.GetLanguage("UserWindow.Info6"), url),
            string.Format(App.GetLanguage("UserWindow.Info7"), code), () =>
            {
                Cancel = true;
                UserBinding.OAuthCancel();
            });
        BaseBinding.OpUrl(url);
        await BaseBinding.CopyTextClipboard(code);
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (ComboBox1.SelectedIndex)
        {
            case 0:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = App.GetLanguage("UserWindow.Info8");
                TextBox_Input1.Text = "";
                break;
            case 1:
                TextBox_Input1.IsEnabled = false;
                TextBox_Input1.Watermark = "";
                TextBox_Input1.Text = "";
                break;
            case 2:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = App.GetLanguage("UserWindow.Info9");
                TextBox_Input1.Text = "";
                break;
            case 3:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = App.GetLanguage("UserWindow.Info10");
                TextBox_Input1.Text = "";
                break;
            case 4:
                TextBox_Input1.IsEnabled = false;
                TextBox_Input1.Watermark = "";
                TextBox_Input1.Text = "";
                break;
            case 5:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = App.GetLanguage("UserWindow.Info11");
                TextBox_Input1.Text = "";
                break;
        }
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
