using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using ColorMC.Core.Objs;
using ColorMC.Core;
using System.Threading.Channels;
using System.Threading;
using System.Web;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ColorMC.Gui.UI.Model.User;

public partial class UsersModel : ObservableObject
{
    private IUserControl Con;

    private bool cancel;

    public List<string> UserTypeList { get; } = UserBinding.GetUserTypes();
    public ObservableCollection<UserDisplayObj> UserList { get; init; } = new();

    [ObservableProperty]
    private UserDisplayObj? item;

    [ObservableProperty]
    private int type;

    [ObservableProperty]
    private bool enableName;
    [ObservableProperty]
    private bool enableUser;
    [ObservableProperty]
    private bool enablePassword;
    [ObservableProperty]
    private bool enableType;
    [ObservableProperty]
    private bool isAdding;
    [ObservableProperty]
    private bool displayAdd;

    [ObservableProperty]
    private string watermarkName;
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string user;
    [ObservableProperty]
    private string password;

    public UsersModel(IUserControl con)
    {
        Con = con;

        Load();
    }

    partial void OnTypeChanged(int value)
    {
        switch (value)
        {
            case 0:
                EnableName = false;
                WatermarkName = "";
                Name = "";
                EnableUser = true;
                User = "";
                EnablePassword = false;
                Password = "";
                break;
            case 1:
                EnableName = false;
                WatermarkName = "";
                Name = "";
                EnableUser = false;
                User = "";
                EnablePassword = false;
                Password = "";
                break;
            case 2:
                EnableName = true;
                WatermarkName = App.GetLanguage("UserWindow.Info9");
                Name = "";
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                break;
            case 3:
                EnableName = true;
                WatermarkName = App.GetLanguage("UserWindow.Info10");
                Name = "";
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                break;
            case 4:
                EnableName = false;
                WatermarkName = "";
                Name = "";
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                break;
            case 5:
                EnableName = true;
                WatermarkName = App.GetLanguage("UserWindow.Info11");
                Name = "";
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                break;
            default:
                EnableName = false;
                WatermarkName = "";
                Name = "";
                EnableUser = false;
                User = "";
                EnablePassword = false;
                Password = "";
                break;
        }
    }

    [RelayCommand]
    public void SetAdd()
    {
        EnableType = true;

        Name = "";
        User = "";
        Password = "";

        Type = -1;

        DisplayAdd = true;
    }

    [RelayCommand]
    public async void Add()
    {
        var window = Con.Window;
        bool ok = false;
        IsAdding = false;
        switch (Type)
        {
            case 0:
                var name = User;
                if (string.IsNullOrWhiteSpace(name))
                {
                    window.OkInfo.Show(App.GetLanguage("Gui.Error8"));
                    break;
                }
                var res = await UserBinding.AddUser(AuthType.Offline, name, null);
                if (!res.Item1)
                {
                    window.OkInfo.Show(res.Item2!);
                    break;
                }
                window.NotifyInfo.Show(App.GetLanguage("Gui.Info4"));
                Name = "";
                ok = true;
                break;
            case 1:
                cancel = false;
                ColorMCCore.LoginOAuthCode = LoginOAuthCode;
                window.ProgressInfo.Show(App.GetLanguage("UserWindow.Info1"));
                res = await UserBinding.AddUser(AuthType.OAuth, null);
                window.ProgressInfo.Close();
                window.InputInfo.Close();
                if (cancel)
                    break;
                if (!res.Item1)
                {
                    window.OkInfo.Show(res.Item2!);
                    break;
                }
                window.NotifyInfo.Show(App.GetLanguage("Gui.Info4"));
                Name = "";
                ok = true;
                break;
            case 2:
                var server = Name;
                if (server?.Length != 32)
                {
                    window.OkInfo.Show(App.GetLanguage("UserWindow.Error3"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(User) ||
                    string.IsNullOrWhiteSpace(Password))
                {
                    window.OkInfo.Show(App.GetLanguage("Gui.Error8"));
                    break;
                }
                window.ProgressInfo.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.Nide8, server,
                    User, Password);
                window.ProgressInfo.Close();
                if (!res.Item1)
                {
                    window.OkInfo.Show(res.Item2!);
                    break;
                }
                window.NotifyInfo.Show(App.GetLanguage("Gui.Info4"));
                Name = "";
                ok = true;
                break;
            case 3:
                server = Name;
                if (string.IsNullOrWhiteSpace(server))
                {
                    window.OkInfo.Show(App.GetLanguage("UserWindow.Error4"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(User) ||
                   string.IsNullOrWhiteSpace(Password))
                {
                    window.OkInfo.Show(App.GetLanguage("Gui.Error8"));
                    break;
                }
                window.ProgressInfo.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.AuthlibInjector, server,
                    User, Password);
                window.ProgressInfo.Close();
                if (!res.Item1)
                {
                    window.OkInfo.Show(res.Item2!);
                    break;
                }
                window.NotifyInfo.Show(App.GetLanguage("Gui.Info4"));
                Name = "";
                ok = true;
                break;
            case 4:
                if (string.IsNullOrWhiteSpace(User) ||
                   string.IsNullOrWhiteSpace(Password))
                {
                    window.OkInfo.Show(App.GetLanguage("Gui.Error8"));
                    break;
                }
                window.ProgressInfo.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.LittleSkin,
                    User, Password);
                window.ProgressInfo.Close();
                if (!res.Item1)
                {
                    window.OkInfo.Show(res.Item2!);
                    break;
                }
                window.NotifyInfo.Show(App.GetLanguage("Gui.Info4"));
                ok = true;
                break;
            case 5:
                server = Name;
                if (string.IsNullOrWhiteSpace(server))
                {
                    window.OkInfo.Show(App.GetLanguage("UserWindow.Error4"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(User) ||
                   string.IsNullOrWhiteSpace(Password))
                {
                    window.OkInfo.Show(App.GetLanguage("Gui.Error8"));
                    break;
                }
                window.ProgressInfo.Show(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.SelfLittleSkin,
                    User, Password, server);
                window.ProgressInfo.Close();
                if (!res.Item1)
                {
                    window.OkInfo.Show(res.Item2!);
                    break;
                }
                window.NotifyInfo.Show(App.GetLanguage("Gui.Info4"));
                Name = "";
                ok = true;
                break;
            default:
                window.OkInfo.Show(App.GetLanguage("UserWindow.Error5"));
                break;
        }
        if (ok)
        {
            UserBinding.UserLastUser();
            DisplayAdd = false;
        }
        Load();
        IsAdding = true;
    }

    [RelayCommand]
    public void Cancel()
    {
        DisplayAdd = false;
    }

    public void Remove(UserDisplayObj item)
    {
        UserBinding.Remove(item.UUID, item.AuthType);
        Load();
    }

    public void Select()
    {
        var window = Con.Window;
        if (Item == null)
        {
            window.OkInfo.Show(App.GetLanguage("UserWindow.Error1"));
            return;
        }

        Select(Item);
    }

    public void Select(UserDisplayObj item)
    {
        var window = Con.Window;
        UserBinding.SetLastUser(item.UUID, item.AuthType);

        window.NotifyInfo.Show(App.GetLanguage("UserWindow.Info5"));
        Load();
    }

    public void Drop(IDataObject data)
    {
        if (data.Contains(DataFormats.Text))
        {
            var str = data.GetText();
            if (str?.StartsWith("authlib-injector:yggdrasil-server:") == true)
            {
                AddUrl(str);
            }
        }
    }

    public void AddUrl(string url)
    {
        SetAdd();
        Type = 3;
        Name = HttpUtility.UrlDecode(url.Replace("authlib-injector:yggdrasil-server:", ""));
    }

    public async void Refresh(UserDisplayObj obj)
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("UserWindow.Info3"));
        var res = await UserBinding.ReLogin(obj.UUID, obj.AuthType);
        window.ProgressInfo.Close();
        if (!res)
        {
            window.OkInfo.Show(App.GetLanguage("UserWindow.Error6"));
            var user = UserBinding.GetUser(obj.UUID, obj.AuthType);
            if (user == null)
                return;

            switch (Type)
            {
                case 2:
                case 3:
                case 5:
                    OnTypeChanged(Type);
                    SetAdd();
                    User = user.Text2;
                    Name = user.Text1;
                    break;
                case 4:
                    OnTypeChanged(Type);
                    SetAdd();
                    User = user.Text2;
                    break;
            }
        }
        else
        {
            window.NotifyInfo.Show(App.GetLanguage("UserWindow.Info4"));
        }
    }

    public void ReLogin(UserDisplayObj obj)
    {
        Type = obj.AuthType.ToInt();

        EnableType = false;
        EnableName = false;
        EnableUser = false;

        Name = obj.Text1;
        User = obj.Text2;

        DisplayAdd = true;
    }

    public void Load()
    {
        var item1 = UserBinding.GetLastUser();
        UserList.Clear();
        foreach (var item in UserBinding.GetAllUser())
        {
            UserList.Add(new()
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

    private async void LoginOAuthCode(string url, string code)
    {
        var window = Con.Window;
        window.ProgressInfo.Close();
        window.InputInfo.Show(string.Format(App.GetLanguage("UserWindow.Info6"), url),
            string.Format(App.GetLanguage("UserWindow.Info7"), code), () =>
            {
                cancel = true;
                UserBinding.OAuthCancel();
            });
        BaseBinding.OpUrl(url);
        await BaseBinding.CopyTextClipboard(TopLevel.GetTopLevel(Con.Window.Con), code);
    }
}
