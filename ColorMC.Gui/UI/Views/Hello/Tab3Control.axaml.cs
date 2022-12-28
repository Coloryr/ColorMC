using Avalonia.Controls;
using ColorMC.Core.Game.Auth;
using System.Collections.Generic;
using System;
using ColorMC.Core.Utils;
using Avalonia.Interactivity;
using ColorMC.Gui.UIBinding;
using ColorMC.Core;
using System.Collections.ObjectModel;
using DynamicData;

namespace ColorMC.Gui.UI.Views.Hello;

public partial class Tab3Control : UserControl
{
    private HelloWindow Window;

    private ObservableCollection<UserObj> List = new();
    public Tab3Control()
    {
        InitializeComponent();
        List_User.Items = List;

        Button_Add.Click += Button_Add_Click;
        Button_Next.Click += Button_Next_Click;
        UserType.SelectionChanged += UserType_SelectionChanged;
        Button_Delete.Click += Button_Delete_Click;
        Button_Refash.Click += Button_Refash_Click;

        UserType.Items = UserBinding.GetUserTypes();

        Load();
    }

    private void Button_Delete_Click(object? sender, RoutedEventArgs e)
    {
        var item = List_User.SelectedItem as UserObj;
        if (item == null)
            return;

        UserBinding.Remove(item);
        Load();
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        Window.Next();
    }

    private async void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        Button_Add.IsEnabled = false;
        switch (UserType.SelectedIndex)
        {
            case 0:
                string name = Input1.Text;
                if (string.IsNullOrWhiteSpace(name))
                {
                    Window.Info.Show("请输入必要信息");
                    break;
                }
                var res = await UserBinding.AddUser(0, name, null);
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                }
                Window.Info2.Show("添加成功");
                Input1.Text = "";
                break;
            case 1:
                CoreMain.LoginOAuthCode = LoginOAuthCode;
                Window.Info1.Show("正在加载");
                res = await UserBinding.AddUser(1, null);
                Window.Info3.Close();
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                }
                Window.Info2.Show("添加成功");
                Input1.Text = "";
                break;
            case 2:
                var server = Input1.Text;
                if (server.Length != 32)
                {
                    Window.Info.Show("服务器UUID错误");
                    break;
                }
                await Window.Info3.Show("账户", "密码", false);
                Window.Info3.Close();
                var user = Window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Window.Info.Show("请输入必要信息");
                    break;
                }
                Window.Info1.Show("正在登录");
                res = await UserBinding.AddUser(2, server, user.Item1, user.Item2);
                Window.Info1.Close();
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                    break;
                }
                Window.Info2.Show("添加成功");
                Input1.Text = "";
                break;
            case 3:
                server = Input1.Text;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Window.Info.Show("服务器UUID错误");
                    break;
                }
                await Window.Info3.Show("账户", "密码", false);
                Window.Info3.Close();
                user = Window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Window.Info.Show("请输入必要信息");
                    break;
                }
                Window.Info1.Show("正在登录");
                res = await UserBinding.AddUser(3, server, user.Item1, user.Item2);
                Window.Info1.Close();
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                    break;
                }
                Window.Info2.Show("添加成功");
                Input1.Text = "";
                break;
            case 4:
                await Window.Info3.Show("账户", "密码", false);
                Window.Info3.Close();
                user = Window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Window.Info.Show("请输入必要信息");
                    break;
                }
                Window.Info1.Show("正在登录");
                res = await UserBinding.AddUser(4, user.Item1, user.Item2);
                Window.Info1.Close();
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                    break;
                }
                Window.Info2.Show("添加成功");
                break;
            case 5:
                server = Input1.Text;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Window.Info.Show("服务器UUID错误");
                    break;
                }
                await Window.Info3.Show("账户", "密码", false);
                Window.Info3.Close();
                user = Window.Info3.Read();
                if (string.IsNullOrWhiteSpace(user.Item1) || string.IsNullOrWhiteSpace(user.Item2))
                {
                    Window.Info.Show("请输入必要信息");
                    break;
                }
                Window.Info1.Show("正在登录");
                res = await UserBinding.AddUser(3, server, user.Item1, user.Item2);
                Window.Info1.Close();
                if (!res.Item1)
                {
                    Window.Info.Show(res.Item2!);
                    break;
                }
                Window.Info2.Show("添加成功");
                Input1.Text = "";
                break;
            default:
                Window.Info.Show("请选择类型");
                break;
        }
        Load();
        Button_Add.IsEnabled = true;
    }

    private void LoginOAuthCode(string url, string code)
    {
        Window.Info1.Close();
        Window.Info3.Show("打开网址" + url, "输入代码:" + code);
    }

    private void UserType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (UserType.SelectedIndex != -1)
        {
            switch (UserType.SelectedIndex)
            {
                case 0:
                    Input1.IsEnabled = true;
                    Input1.Watermark = "用户名";
                    Input1.Text = "";
                    break;
                case 1:
                    Input1.IsEnabled = false;
                    Input1.Watermark = "";
                    Input1.Text = "";
                    break;
                case 2:
                    Input1.IsEnabled = true;
                    Input1.Watermark = "服务器UUID";
                    Input1.Text = "";
                    break;
                case 3:
                    Input1.IsEnabled = true;
                    Input1.Watermark = "服务器地址";
                    Input1.Text = "";
                    break;
                case 4:
                    Input1.IsEnabled = false;
                    Input1.Watermark = "";
                    Input1.Text = "";
                    break;
                case 5:
                    Input1.IsEnabled = true;
                    Input1.Watermark = "皮肤站地址";
                    Input1.Text = "";
                    break;
            }
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
        List.AddRange(UserBinding.GetAllUser());
    }
}
