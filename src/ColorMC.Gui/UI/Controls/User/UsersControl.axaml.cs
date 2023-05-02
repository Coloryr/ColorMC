using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System.Collections.ObjectModel;
using System.Threading;
using System.Web;

namespace ColorMC.Gui.UI.Controls.User;

public partial class UsersControl : UserControl, IUserControl
{
    private readonly ObservableCollection<UserDisplayObj> List = new();
    private bool Cancel;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UsersControl()
    {
        InitializeComponent();

        DataGrid_User.Items = List;
        DataGrid_User.DoubleTapped += DataGrid_User_DoubleTapped;
        DataGrid_User.CellPointerPressed += DataGrid_User_PointerPressed;

        Button_A1.PointerExited += Button_A1_PointerLeave;
        Button_A.PointerEntered += Button_A_PointerEnter;

        Button_A.Click += Button_A1_Click;
        Button_A1.Click += Button_A1_Click;

        Button_Cancel.Click += Button_Cancel_Click;
        Button_Add.Click += Button_Add_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox1.ItemsSource = UserBinding.GetUserTypes();

        TextBox_Input1.KeyDown += TextBox_Input1_KeyDown;
        TextBox_Input3.KeyDown += TextBox_Input3_KeyDown;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void TextBox_Input3_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Button_Add_Click(null, null);
        }
    }

    private void TextBox_Input1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (ComboBox1.SelectedIndex == 0)
            {
                Button_Add_Click(null, null);
            }
        }
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("UserWindow.Title"));

        Load();

        Dispatcher.UIThread.Post(DataGrid_User.MakeTran);
    }

    public void Closed()
    {
        ColorMCCore.LoginOAuthCode = null;

        App.UserWindow = null;
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Source is Control)
        {
            return;
        }
        if (e.Data.Contains(DataFormats.Text))
        {
            Grid2.IsVisible = true;
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (e.Source is Control)
        {
            return;
        }
        Grid2.IsVisible = false;
        if (e.Data.Contains(DataFormats.Text))
        {
            var str = e.Data.GetText();
            if (str?.StartsWith("authlib-injector:yggdrasil-server:") == true)
            {
                AddUrl(str);
            }
        }
    }

    private void DataGrid_User_PointerPressed(object? sender,
        DataGridCellPointerPressedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var user = DataGrid_User.SelectedItem as UserDisplayObj;
            if (user == null)
                return;

            var pro = e.PointerPressedEventArgs.GetCurrentPoint(this);

            if (pro.Properties.IsRightButtonPressed)
            {
                _ = new UserFlyout(this, user);
            }
            else if (e.Column.DisplayIndex == 0 && pro.Properties.IsLeftButtonPressed)
            {
                Select(user);
            }
        });
    }

    private void DataGrid_User_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var item = DataGrid_User.SelectedItem as UserDisplayObj;
        if (item == null)
        {
            window.Info.Show(App.GetLanguage("UserWindow.Error1"));
            return;
        }

        Select(item);
    }

    private void Select(UserDisplayObj item)
    {
        var window = App.FindRoot(VisualRoot);
        UserBinding.SetLastUser(item.UUID, item.AuthType);

        window.Info2.Show(App.GetLanguage("UserWindow.Info5"));
        Load();
    }

    private async void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        bool ok = false;
        Button_Add.IsEnabled = false;
        switch (ComboBox1.SelectedIndex)
        {
            case 0:
                var name = TextBox_Input2.Text;
                if (string.IsNullOrWhiteSpace(name))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error2"));
                    break;
                }
                var res = await UserBinding.AddUser(AuthType.Offline, name, null);
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                TextBox_Input1.Text = "";
                ok = true;
                break;
            case 1:
                Cancel = false;
                ColorMCCore.LoginOAuthCode = LoginOAuthCode;
                window.Info1.Show(App.GetLanguage("UserWindow.Info1"));
                res = await UserBinding.AddUser(AuthType.OAuth, null);
                window.Info1.Close();
                window.Info3.Close();
                if (Cancel)
                    break;
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                TextBox_Input1.Text = "";
                ok = true;
                break;
            case 2:
                var server = TextBox_Input1.Text;
                if (server?.Length != 32)
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error3"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(TextBox_Input2.Text) ||
                    string.IsNullOrWhiteSpace(TextBox_Input3.Text))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error2"));
                    break;
                }
                window.Info1.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.Nide8, server,
                    TextBox_Input2.Text, TextBox_Input3.Text);
                window.Info1.Close();
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                TextBox_Input1.Text = "";
                ok = true;
                break;
            case 3:
                server = TextBox_Input1.Text;
                if (string.IsNullOrWhiteSpace(server))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error4"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(TextBox_Input2.Text) ||
                   string.IsNullOrWhiteSpace(TextBox_Input3.Text))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error2"));
                    break;
                }
                window.Info1.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.AuthlibInjector, server,
                    TextBox_Input2.Text, TextBox_Input3.Text);
                window.Info1.Close();
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                TextBox_Input1.Text = "";
                ok = true;
                break;
            case 4:
                if (string.IsNullOrWhiteSpace(TextBox_Input2.Text) ||
                   string.IsNullOrWhiteSpace(TextBox_Input3.Text))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error2"));
                    break;
                }
                window.Info1.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.LittleSkin,
                    TextBox_Input2.Text, TextBox_Input3.Text);
                window.Info1.Close();
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                ok = true;
                break;
            case 5:
                server = TextBox_Input1.Text;
                if (string.IsNullOrWhiteSpace(server))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error4"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(TextBox_Input2.Text) ||
                   string.IsNullOrWhiteSpace(TextBox_Input3.Text))
                {
                    window.Info.Show(App.GetLanguage("UserWindow.Error2"));
                    break;
                }
                window.Info1.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.SelfLittleSkin,
                    TextBox_Input2.Text, TextBox_Input3.Text, server);
                window.Info1.Close();
                if (!res.Item1)
                {
                    window.Info.Show(res.Item2!);
                    break;
                }
                window.Info2.Show(App.GetLanguage("Info4"));
                TextBox_Input1.Text = "";
                ok = true;
                break;
            default:
                window.Info.Show(App.GetLanguage("UserWindow.Error5"));
                break;
        }
        if (ok)
        {
            UserBinding.UserLastUser();
            await App.CrossFade300.Start(Grid1, null, CancellationToken.None);
        }
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
                TextBox_Input1.IsEnabled = false;
                TextBox_Input1.Watermark = "";
                TextBox_Input1.Text = "";
                TextBox_Input2.IsEnabled = true;
                TextBox_Input2.Text = "";
                TextBox_Input3.IsEnabled = false;
                TextBox_Input3.Text = "";
                break;
            case 1:
                TextBox_Input1.IsEnabled = false;
                TextBox_Input1.Watermark = "";
                TextBox_Input1.Text = "";
                TextBox_Input2.IsEnabled = false;
                TextBox_Input2.Text = "";
                TextBox_Input3.IsEnabled = false;
                TextBox_Input3.Text = "";
                break;
            case 2:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = App.GetLanguage("UserWindow.Info9");
                TextBox_Input1.Text = "";
                TextBox_Input2.IsEnabled = true;
                TextBox_Input2.Text = "";
                TextBox_Input3.IsEnabled = true;
                TextBox_Input3.Text = "";
                break;
            case 3:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = App.GetLanguage("UserWindow.Info10");
                TextBox_Input1.Text = "";
                TextBox_Input2.IsEnabled = true;
                TextBox_Input2.Text = "";
                TextBox_Input3.IsEnabled = true;
                TextBox_Input3.Text = "";
                break;
            case 4:
                TextBox_Input1.IsEnabled = false;
                TextBox_Input1.Watermark = "";
                TextBox_Input1.Text = "";
                TextBox_Input2.IsEnabled = true;
                TextBox_Input2.Text = "";
                TextBox_Input3.IsEnabled = true;
                TextBox_Input3.Text = "";
                break;
            case 5:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = App.GetLanguage("UserWindow.Info11");
                TextBox_Input1.Text = "";
                TextBox_Input2.IsEnabled = true;
                TextBox_Input2.Text = "";
                TextBox_Input3.IsEnabled = true;
                TextBox_Input3.Text = "";
                break;
            default:
                TextBox_Input1.IsEnabled = false;
                TextBox_Input1.Watermark = "";
                TextBox_Input1.Text = "";
                TextBox_Input2.IsEnabled = false;
                TextBox_Input2.Text = "";
                TextBox_Input3.IsEnabled = false;
                TextBox_Input3.Text = "";
                break;
        }
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        App.CrossFade300.Start(Grid1, null, CancellationToken.None);
    }

    private void Button_D1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var item = DataGrid_User.SelectedItem as UserDisplayObj;
        if (item == null)
        {
            window.Info.Show(App.GetLanguage("UserWindow.Error1"));
            return;
        }

        UserBinding.Remove(item.UUID, item.AuthType);

        Load();
    }

    private void Button_S1_Click(object? sender, RoutedEventArgs e)
    {
        DataGrid_User_DoubleTapped(sender, e);
    }

    private void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        SetAdd();
    }

    public void SetAdd()
    {
        ComboBox1.IsEnabled = true;

        TextBox_Input1.Text = "";
        TextBox_Input2.Text = "";
        TextBox_Input3.Text = "";

        ComboBox1.SelectedIndex = -1;

        App.CrossFade300.Start(null, Grid1, CancellationToken.None);
    }

    public async void Refresh(UserDisplayObj obj)
    {
        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("UserWindow.Info3"));
        var res = await UserBinding.ReLogin(obj.UUID, obj.AuthType);
        window.Info1.Close();
        if (!res)
        {
            window.Info.Show(App.GetLanguage("UserWindow.Error6"));
            var user = UserBinding.GetUser(obj.UUID, obj.AuthType);
            if (user == null)
                return;

            switch (ComboBox1.SelectedIndex)
            {
                case 2:
                case 3:
                case 5:
                    ComboBox1_SelectionChanged(null, null);
                    SetAdd();
                    TextBox_Input2.Text = user.Text2;
                    TextBox_Input1.Text = user.Text1;
                    break;
                case 4:
                    ComboBox1_SelectionChanged(null, null);
                    SetAdd();
                    TextBox_Input2.Text = user.Text2;
                    break;
            }
        }
        else
        {
            window.Info2.Show(App.GetLanguage("UserWindow.Info4"));
        }
    }


    public void Load()
    {
        var item1 = UserBinding.GetLastUser();
        List.Clear();
        foreach (var item in UserBinding.GetAllUser())
        {
            List.Add(new()
            {
                Name = item.Value.UserName,
                UUID = item.Key.Item1,
                AuthType = item.Key.Item2,
                Type = item.Key.Item2.GetName(),
                Text1 = item.Value.Text1,
                Text2 = item.Value.Text2,
                Use = item1 == item.Value
            });
        }
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_A1, null, CancellationToken.None);
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_A1, CancellationToken.None);
    }

    public void AddUrl(string url)
    {
        SetAdd();
        ComboBox1.SelectedIndex = 3;
        TextBox_Input1.Text = HttpUtility.UrlDecode(url.Replace("authlib-injector:yggdrasil-server:", ""));
    }

    public void ReLogin(UserDisplayObj obj)
    {
        ComboBox1.SelectedIndex = obj.AuthType.ToInt();

        ComboBox1.IsEnabled = false;
        TextBox_Input1.IsEnabled = false;
        TextBox_Input2.IsEnabled = false;

        TextBox_Input1.Text = obj.Text1;
        TextBox_Input2.Text = obj.Text2;

        App.CrossFade300.Start(null, Grid1, CancellationToken.None);
    }
}
