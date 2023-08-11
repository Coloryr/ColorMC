using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Web;

namespace ColorMC.Gui.UI.Model.User;

public partial class UsersModel : BaseModel
{
    public List<string> UserTypeList { get; init; } = UserBinding.GetUserTypes();
    public ObservableCollection<UserDisplayObj> UserList { get; init; } = new();

    [ObservableProperty]
    private UserDisplayObj? _item;

    [ObservableProperty]
    private int _type;

    [ObservableProperty]
    private bool _enableName;
    [ObservableProperty]
    private bool _enableUser;
    [ObservableProperty]
    private bool _enablePassword;
    [ObservableProperty]
    private bool _enableType;
    [ObservableProperty]
    private bool _isAdding;
    [ObservableProperty]
    private bool _displayAdd;

    [ObservableProperty]
    private string _watermarkName;
    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private string _user;
    [ObservableProperty]
    private string _password;

    private bool _cancel;

    public UsersModel(IUserControl con) : base(con)
    {
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

        if (ConfigBinding.IsLockLogin())
        {
            ConfigBinding.GetLockLogin(out var type, out var url);
            Type = type + 1;
            Name = url;
            EnableType = false;
            EnableName = false;
        }
        else
        {
            Name = "";
            Type = -1;
            EnableType = true;
            EnableName = true;
        }

        User = "";
        Password = "";

        DisplayAdd = true;
    }

    [RelayCommand]
    public async Task Add()
    {
        bool ok = false;
        IsAdding = true;
        switch (Type)
        {
            case 0:
                var name = User;
                if (string.IsNullOrWhiteSpace(name))
                {
                    Show(App.GetLanguage("Gui.Error8"));
                    break;
                }
                var res = await UserBinding.AddUser(AuthType.Offline, name, null);
                if (!res.Item1)
                {
                    Show(res.Item2!);
                    break;
                }
                Notify(App.GetLanguage("Gui.Info4"));
                Name = "";
                ok = true;
                break;
            case 1:
                _cancel = false;
                ColorMCCore.LoginOAuthCode = LoginOAuthCode;
                Progress(App.GetLanguage("UserWindow.Info1"));
                res = await UserBinding.AddUser(AuthType.OAuth, null);
                ProgressClose();
                InputClose();
                if (_cancel)
                    break;
                if (!res.Item1)
                {
                    Show(res.Item2!);
                    break;
                }
                Notify(App.GetLanguage("Gui.Info4"));
                Name = "";
                ok = true;
                break;
            case 2:
                var server = Name;
                if (server?.Length != 32)
                {
                    Show(App.GetLanguage("UserWindow.Error3"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(User) ||
                    string.IsNullOrWhiteSpace(Password))
                {
                    Show(App.GetLanguage("Gui.Error8"));
                    break;
                }
                Progress(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.Nide8, server,
                    User, Password);
                ProgressClose();
                if (!res.Item1)
                {
                    Show(res.Item2!);
                    break;
                }
                Notify(App.GetLanguage("Gui.Info4"));
                Name = "";
                ok = true;
                break;
            case 3:
                server = Name;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Show(App.GetLanguage("UserWindow.Error4"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(User) ||
                   string.IsNullOrWhiteSpace(Password))
                {
                    Show(App.GetLanguage("Gui.Error8"));
                    break;
                }
                Progress(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.AuthlibInjector, server,
                    User, Password);
                ProgressClose();
                if (!res.Item1)
                {
                    Show(res.Item2!);
                    break;
                }
                Notify(App.GetLanguage("Gui.Info4"));
                Name = "";
                ok = true;
                break;
            case 4:
                if (string.IsNullOrWhiteSpace(User) ||
                   string.IsNullOrWhiteSpace(Password))
                {
                    Show(App.GetLanguage("Gui.Error8"));
                    break;
                }
                Progress(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.LittleSkin,
                    User, Password);
                ProgressClose();
                if (!res.Item1)
                {
                    Show(res.Item2!);
                    break;
                }
                Notify(App.GetLanguage("Gui.Info4"));
                ok = true;
                break;
            case 5:
                server = Name;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Show(App.GetLanguage("UserWindow.Error4"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(User) ||
                   string.IsNullOrWhiteSpace(Password))
                {
                    Show(App.GetLanguage("Gui.Error8"));
                    break;
                }
                Progress(App.GetLanguage("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.SelfLittleSkin,
                    User, Password, server);
                ProgressClose();
                if (!res.Item1)
                {
                    Show(res.Item2!);
                    break;
                }
                Notify(App.GetLanguage("Gui.Info4"));
                Name = "";
                ok = true;
                break;
            default:
                Show(App.GetLanguage("UserWindow.Error5"));
                break;
        }
        if (ok)
        {
            UserBinding.UserLastUser();
            DisplayAdd = false;
        }
        Load();
        IsAdding = false;
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
        if (Item == null)
        {
            Show(App.GetLanguage("UserWindow.Error1"));
            return;
        }

        Select(Item);
    }

    public void Select(UserDisplayObj item)
    {
        UserBinding.SetLastUser(item.UUID, item.AuthType);

        Notify(App.GetLanguage("UserWindow.Info5"));
        Load();
    }

    public void Drop(IDataObject data)
    {
        if (ConfigBinding.IsLockLogin())
            return;

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
        if (ConfigBinding.IsLockLogin())
            return;

        SetAdd();
        Type = 3;
        Name = HttpUtility.UrlDecode(url.Replace("authlib-injector:yggdrasil-server:", ""));
    }

    public async void Refresh(UserDisplayObj obj)
    {
        Progress(App.GetLanguage("UserWindow.Info3"));
        var res = await UserBinding.ReLogin(obj.UUID, obj.AuthType);
        ProgressClose();
        if (!res)
        {
            Show(App.GetLanguage("UserWindow.Error6"));
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
            Notify(App.GetLanguage("UserWindow.Info4"));
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
        IsAdding = false;
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
        ProgressClose();
        ShowInput(string.Format(App.GetLanguage("UserWindow.Info6"), url),
            string.Format(App.GetLanguage("UserWindow.Info7"), code), () =>
            {
                _cancel = true;
                UserBinding.OAuthCancel();
            });
        BaseBinding.OpUrl(url);
        await BaseBinding.CopyTextClipboard(TopLevel.GetTopLevel(Control.Con), code);
    }

    public override void Close()
    {
        UserList.Clear();
    }
}
