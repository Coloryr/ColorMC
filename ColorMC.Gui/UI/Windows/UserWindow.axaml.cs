using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class UserWindow : Window
{
    private readonly ObservableCollection<UserDisplayObj> List = new();

    private readonly CrossFade CrossFade = new(TimeSpan.FromMilliseconds(300));
    private readonly CrossFade CrossFade1 = new(TimeSpan.FromMilliseconds(100));
    public UserWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;

        if (App.BackBitmap != null)
        {
            Image_Back.Source = App.BackBitmap;
        }

        DataGrid_User.Items = List;
        DataGrid_User.DoubleTapped += DataGrid_User_DoubleTapped;

        Button_A1.PointerLeave += Button_A1_PointerLeave;
        Button_A.PointerEnter += Button_A_PointerEnter;

        Button_D1.PointerLeave += Button_D1_PointerLeave;
        Button_D.PointerEnter += Button_D_PointerEnter;

        Button_S1.PointerLeave += Button_S1_PointerLeave;
        Button_S.PointerEnter += Button_S_PointerEnter;

        Button_A1.Click += Button_A1_Click;
        Button_D1.Click += Button_D1_Click;
        Button_S1.Click += Button_S1_Click;

        Button_Cancel.Click += Button_Cancel_Click;
        Button_Add.Click += Button_Add_Click;

        Expander_A.ContentTransition = CrossFade1;
        Expander_D.ContentTransition = CrossFade1;
        Expander_S.ContentTransition = CrossFade1;

        Closed += UserWindow_Closed;
        Opened += UserWindow_Opened;

        ComboBox_UserType.SelectionChanged += UserType_SelectionChanged;
        ComboBox_UserType.Items = UserBinding.GetUserTypes();

        Load();
    }

    private void DataGrid_User_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var item = DataGrid_User.SelectedItem as UserDisplayObj;
        if (item == null)
        {
            Info.Show("请先选择账户");
            return;
        }

        UserBinding.SetLastUser(item.UUID, item.AuthType);
        MainWindow.OnUserEdit();

        Info2.Show("切换成功");
        Load();
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
                    Info.Show("请输入必要信息");
                    break;
                }
                var res = await UserBinding.AddUser(0, name, null);
                if (!res.Item1)
                {
                    Info.Show(res.Item2!);
                }
                Info2.Show("添加成功");
                TextBox_Input1.Text = "";
                break;
            case 1:
                CoreMain.LoginOAuthCode = LoginOAuthCode;
                Info1.Show("正在加载");
                res = await UserBinding.AddUser(1, null);
                Info3.Close();
                if (!res.Item1)
                {
                    Info.Show(res.Item2!);
                }
                Info2.Show("添加成功");
                TextBox_Input1.Text = "";
                break;
            case 2:
                var server = TextBox_Input1.Text;
                if (server.Length != 32)
                {
                    Info.Show("服务器UUID错误");
                    break;
                }
                await Info3.Show("账户", "密码", false);
                Info3.Close();
                var user = Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Info.Show("请输入必要信息");
                    break;
                }
                Info1.Show("正在登录");
                res = await UserBinding.AddUser(2, server, user.Item1, user.Item2);
                Info1.Close();
                if (!res.Item1)
                {
                    Info.Show(res.Item2!);
                    break;
                }
                Info2.Show("添加成功");
                TextBox_Input1.Text = "";
                break;
            case 3:
                server = TextBox_Input1.Text;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Info.Show("服务器UUID错误");
                    break;
                }
                await Info3.Show("账户", "密码", false);
                Info3.Close();
                user = Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Info.Show("请输入必要信息");
                    break;
                }
                Info1.Show("正在登录");
                res = await UserBinding.AddUser(3, server, user.Item1, user.Item2);
                Info1.Close();
                if (!res.Item1)
                {
                    Info.Show(res.Item2!);
                    break;
                }
                Info2.Show("添加成功");
                TextBox_Input1.Text = "";
                break;
            case 4:
                await Info3.Show("账户", "密码", false);
                Info3.Close();
                user = Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Info.Show("请输入必要信息");
                    break;
                }
                Info1.Show("正在登录");
                res = await UserBinding.AddUser(4, user.Item1, user.Item2);
                Info1.Close();
                if (!res.Item1)
                {
                    Info.Show(res.Item2!);
                    break;
                }
                Info2.Show("添加成功");
                break;
            case 5:
                server = TextBox_Input1.Text;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Info.Show("服务器UUID错误");
                    break;
                }
                await Info3.Show("账户", "密码", false);
                Info3.Close();
                user = Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Info.Show("请输入必要信息");
                    break;
                }
                Info1.Show("正在登录");
                res = await UserBinding.AddUser(5, server, user.Item1, user.Item2);
                Info1.Close();
                if (!res.Item1)
                {
                    Info.Show(res.Item2!);
                    break;
                }
                Info2.Show("添加成功");
                TextBox_Input1.Text = "";
                break;
            default:
                Info.Show("请选择类型");
                break;
        }
        Load();
        await CrossFade.Start(Grid_Add, null, CancellationToken.None);
        Button_Add.IsEnabled = true;
    }

    private void LoginOAuthCode(string url, string code)
    {
        Info1.Close();
        Info3.Show("打开网址" + url, "输入代码:" + code);
    }

    private void UserType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (ComboBox_UserType.SelectedIndex)
        {
            case 0:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = "用户名";
                TextBox_Input1.Text = "";
                TextBox_Input2.IsEnabled = false;
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
                TextBox_Input1.Watermark = "服务器UUID";
                TextBox_Input1.Text = "";
                TextBox_Input2.IsEnabled = true;
                TextBox_Input2.Text = "";
                TextBox_Input3.IsEnabled = true;
                TextBox_Input3.Text = "";
                break;
            case 3:
                TextBox_Input1.IsEnabled = true;
                TextBox_Input1.Watermark = "服务器地址";
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
                TextBox_Input1.Watermark = "皮肤站地址";
                TextBox_Input1.Text = "";
                TextBox_Input2.IsEnabled = true;
                TextBox_Input2.Text = "";
                TextBox_Input3.IsEnabled = true;
                TextBox_Input3.Text = "";
                break;
        }
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        CrossFade.Start(Grid_Add, null, CancellationToken.None);
    }

    private void Button_D1_Click(object? sender, RoutedEventArgs e)
    {
        var item = DataGrid_User.SelectedItem as UserDisplayObj;
        if (item == null)
        {
            Info.Show("请先选择账户");
            return;
        }

        UserBinding.Remove(item.UUID, item.AuthType);
        MainWindow.OnUserEdit();
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

    private void Button_S1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_S.IsExpanded = false;
    }

    private void Button_S_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_S.IsExpanded = true;
    }

    private void Button_D1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_D.IsExpanded = false;
    }

    private void Button_D_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_D.IsExpanded = true;
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = false;
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = true;
    }

    private void UserWindow_Opened(object? sender, EventArgs e)
    {
        DataGrid_User.MakeTran();
        Expander_A.MakePadingNull();
        Expander_D.MakePadingNull();
        Expander_S.MakePadingNull();
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        App.UserWindow = null;
        CoreMain.LoginOAuthCode = null;
    }

    public void SetAdd()
    {
        CrossFade.Start(null, Grid_Add, CancellationToken.None);
    }

    private void Load()
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
}
