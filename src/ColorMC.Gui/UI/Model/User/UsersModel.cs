using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.User;

public partial class UsersControlModel : TopModel
{
    public List<string> UserTypeList { get; init; } = UserBinding.GetUserTypes();
    public ObservableCollection<UserDisplayModel> UserList { get; init; } = [];

    [ObservableProperty]
    private UserDisplayModel? _item;

    [ObservableProperty]
    private AuthType _type;

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
    private bool _canRegister;

    [ObservableProperty]
    private string _watermarkName;
    [ObservableProperty]
    private string? _name;
    [ObservableProperty]
    private string? _user;
    [ObservableProperty]
    private string? _password;

    private bool _cancel;
    private bool _isOAuth;

    public UsersControlModel(BaseModel model) : base(model)
    {
        Load();
    }

    partial void OnPasswordChanging(string? value)
    {
        if (value?.EndsWith(Environment.NewLine) == true)
        {
            Password = value.TrimEnd();

            _ = Add();
        }
    }

    partial void OnUserChanging(string? value)
    {
        if (value?.EndsWith(Environment.NewLine) == true)
        {
            User = value.TrimEnd();

            _ = Add();
        }
    }

    partial void OnNameChanging(string? value)
    {
        if (value?.EndsWith(Environment.NewLine) == true)
        {
            Name = value.TrimEnd();

            _ = Add();
        }
    }

    partial void OnTypeChanged(AuthType value)
    {
        switch (value)
        {
            case AuthType.Offline:
                EnableName = false;
                WatermarkName = "";
                Name = "";
                EnableUser = true;
                User = "";
                EnablePassword = false;
                Password = "";
                CanRegister = false;
                break;
            case AuthType.OAuth:
                EnableName = false;
                WatermarkName = "";
                Name = "";
                EnableUser = false;
                User = "";
                EnablePassword = false;
                Password = "";
                CanRegister = true;
                break;
            case AuthType.Nide8:
                EnableName = true;
                WatermarkName = App.Lang("UserWindow.Info9");
                Name = "";
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                CanRegister = true;
                break;
            case AuthType.AuthlibInjector:
                EnableName = true;
                WatermarkName = App.Lang("UserWindow.Info10");
                Name = "";
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                CanRegister = false;
                break;
            case AuthType.LittleSkin:
                EnableName = false;
                WatermarkName = "";
                Name = "";
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                CanRegister = true;
                break;
            case AuthType.SelfLittleSkin:
                EnableName = true;
                WatermarkName = App.Lang("UserWindow.Info11");
                Name = "";
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                CanRegister = false;
                break;
            default:
                EnableName = false;
                WatermarkName = "";
                Name = "";
                EnableUser = false;
                User = "";
                EnablePassword = false;
                Password = "";
                CanRegister = false;
                break;
        }
    }

    [RelayCommand]
    public void Register()
    {
        switch (Type)
        {
            case AuthType.OAuth:
            case AuthType.Nide8:
            case AuthType.LittleSkin:
                WebBinding.OpenRegister(Type, Name);
                break;
        }
    }

    [RelayCommand]
    public void SetAdd()
    {
        EnableType = true;
        var server = ConfigBinding.GetAllConfig().Item2.ServerCustom;
        if (server.LockLogin)
        {
            Type = (AuthType)(server.LoginType + 1);
            Name = server.LoginUrl;
            EnableType = false;
        }
        else
        {
            Name = "";
            Type = (AuthType)(-1);
            EnableType = true;
        }

        User = "";
        Password = "";

        Show();
    }

    [RelayCommand]
    public async Task Add()
    {
        bool ok = false;
        IsAdding = true;
        switch (Type)
        {
            case AuthType.Offline:
                var name = User;
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(name))
                {
                    Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
                    break;
                }
                var res = await UserBinding.AddUser(AuthType.Offline, LoginOAuthCode, name, null);
                if (!res.Item1)
                {
                    Model.Show(res.Item2!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                Name = "";
                ok = true;
                break;
            case AuthType.OAuth:
                _cancel = false;
                _isOAuth = true;
                Model.Progress(App.Lang("UserWindow.Info1"));
                res = await UserBinding.AddUser(AuthType.OAuth, LoginOAuthCode, null);
                Model.ProgressClose();
                if (_cancel)
                {
                    break;
                }
                if (!res.Item1)
                {
                    Model.Show(res.Item2!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                Name = "";
                _isOAuth = false;
                ok = true;
                break;
            case AuthType.Nide8:
                var server = Name;
                _isOAuth = false;
                if (server?.Length != 32)
                {
                    Model.Show(App.Lang("UserWindow.Error3"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(User) ||
                    string.IsNullOrWhiteSpace(Password))
                {
                    Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
                    break;
                }
                Model.Progress(App.Lang("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.Nide8, LoginOAuthCode, server,
                    User, Password);
                Model.ProgressClose();
                if (!res.Item1)
                {
                    Model.Show(res.Item2!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                Name = "";
                ok = true;
                break;
            case AuthType.AuthlibInjector:
                server = Name;
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Model.Show(App.Lang("UserWindow.Error4"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(User) ||
                   string.IsNullOrWhiteSpace(Password))
                {
                    Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
                    break;
                }
                Model.Progress(App.Lang("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.AuthlibInjector, LoginOAuthCode, server,
                    User, Password);
                Model.ProgressClose();
                if (!res.Item1)
                {
                    Model.Show(res.Item2!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                Name = "";
                ok = true;
                break;
            case AuthType.LittleSkin:
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(User) ||
                   string.IsNullOrWhiteSpace(Password))
                {
                    Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
                    break;
                }
                Model.Progress(App.Lang("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.LittleSkin, LoginOAuthCode,
                    User, Password);
                Model.ProgressClose();
                if (!res.Item1)
                {
                    Model.Show(res.Item2!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                ok = true;
                break;
            case AuthType.SelfLittleSkin:
                server = Name;
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Model.Show(App.Lang("UserWindow.Error4"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(User) ||
                   string.IsNullOrWhiteSpace(Password))
                {
                    Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
                    break;
                }
                Model.Progress(App.Lang("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.SelfLittleSkin, LoginOAuthCode,
                    User, Password, server);
                Model.ProgressClose();
                if (!res.Item1)
                {
                    Model.Show(res.Item2!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                Name = "";
                ok = true;
                break;
            default:
                Model.Show(App.Lang("UserWindow.Error5"));
                break;
        }
        if (ok)
        {
            UserBinding.UserLastUser();
            CloseShow();
        }
        Load();
        IsAdding = false;
    }

    [RelayCommand]
    public void Cancel()
    {
        CloseShow();
    }

    public void Remove(UserDisplayModel item)
    {
        UserBinding.Remove(item.UUID, item.AuthType);
        Load();
    }

    public void Select()
    {
        if (Item == null)
        {
            Model.Show(App.Lang("UserWindow.Error1"));
            return;
        }

        Select(Item);
    }

    public void Select(UserDisplayModel item)
    {
        UserBinding.SetLastUser(item.UUID, item.AuthType);

        Model.Notify(App.Lang("UserWindow.Info5"));
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
        Type = AuthType.AuthlibInjector;
        Name = HttpUtility.UrlDecode(url.Replace("authlib-injector:yggdrasil-server:", ""));
    }

    public async void Refresh(UserDisplayModel obj)
    {
        Model.Progress(App.Lang("UserWindow.Info3"));
        var res = await UserBinding.ReLogin(obj.UUID, obj.AuthType);
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("UserWindow.Error6"));
            var user = UserBinding.GetUser(obj.UUID, obj.AuthType);
            if (user == null)
                return;

            switch (Type)
            {
                case AuthType.Nide8:
                case AuthType.AuthlibInjector:
                case AuthType.SelfLittleSkin:
                    OnTypeChanged(Type);
                    SetAdd();
                    User = user.Text2;
                    Name = user.Text1;
                    break;
                case AuthType.LittleSkin:
                    OnTypeChanged(Type);
                    SetAdd();
                    User = user.Text2;
                    break;
            }
        }
        else
        {
            Model.Notify(App.Lang("UserWindow.Info4"));
        }
    }

    public void ReLogin(UserDisplayModel obj)
    {
        Type = obj.AuthType;

        EnableType = false;
        EnableName = false;
        EnableUser = false;

        Name = obj.Text1;
        User = obj.Text2;

        Show();
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
        Model.ProgressClose();
        Model.ShowReadInfo(string.Format(App.Lang("UserWindow.Info6"), url),
            string.Format(App.Lang("UserWindow.Info7"), code), () =>
            {
                _cancel = true;
                UserBinding.OAuthCancel();
            });
        BaseBinding.OpUrl($"{url}?otc={code}");
        await BaseBinding.CopyTextClipboard(code);
    }

    protected override void Close()
    {
        UserList.Clear();
        if (_isOAuth)
        {
            UserBinding.OAuthCancel();
        }
    }

    public async void Edit(UserDisplayModel obj)
    {
        if (obj.AuthType != AuthType.Offline)
        {
            return;
        }

        var res = await Model.ShowEditInput(obj.Name, obj.UUID,
            App.Lang("Text.UserName"), App.Lang("UserWindow.DataGrid.Text4"));
        if (res.Cancel)
        {
            return;
        }
        else if (string.IsNullOrWhiteSpace(res.Text1) || string.IsNullOrWhiteSpace(res.Text2))
        {
            Model.Show(App.Lang("UserWindow.Error7"));
            return;
        }
        else if (res.Text2.Length != 32 || !FuntionUtils.CheckIs(res.Text2))
        {
            Model.Show(App.Lang("UserWindow.Error8"));
            return;
        }

        UserBinding.EditUser(obj.Name, obj.UUID, res.Text1, res.Text2);
    }

    private void Show()
    {
        Model.AddBackCall(() =>
        {
            DialogHost.Close("UsersControl");
            Model.RemoveBack();
        });

        Dispatcher.UIThread.Post(() =>
        {
            DialogHost.Show(this, "UsersControl");
        });
    }

    private void CloseShow()
    {
        Model.BackClick();
    }
}
