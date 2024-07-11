using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.User;

public partial class UsersControlModel : TopModel
{
    public ObservableCollection<string> UserTypeList { get; } = [];
    public ObservableCollection<string> DisplayUserTypeList { get; } = [];
    public ObservableCollection<UserDisplayModel> UserList { get; init; } = [];

    public bool LockLogin { get; init; }

    public bool HaveLogin
    {
        get
        {
            if (!LockLogin)
            {
                return true;
            }

            return _locks.Length > 0;
        }
    }

    [ObservableProperty]
    private UserDisplayModel? _item;

    [ObservableProperty]
    private int _type = -1;

    [ObservableProperty]
    private int _displayType;

    [ObservableProperty]
    private bool _enableName;
    [ObservableProperty]
    private bool _enableUser;
    [ObservableProperty]
    private bool _enablePassword;
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

    private readonly LockLoginSetting[] _locks;

    private bool _cancel;
    private bool _isOAuth;

    public UsersControlModel(BaseModel model) : base(model)
    {
        var config = GuiConfigUtils.Config.ServerCustom;
        LockLogin = config.LockLogin;
        if (LockLogin)
        {
            _locks = [.. config.LockLogins];
            DisplayUserTypeList.Add("");
            foreach (var item in _locks)
            {
                if (item.Type == AuthType.OAuth)
                {
                    UserTypeList.Add(AuthType.OAuth.GetName());
                    DisplayUserTypeList.Add(AuthType.OAuth.GetName());
                }
                else
                {
                    UserTypeList.Add(item.Name);
                    DisplayUserTypeList.Add(item.Name);
                }
            }
        }
        else
        {
            UserTypeList.AddRange(UserBinding.GetLoginType());
            DisplayUserTypeList.AddRange(UserBinding.GetDisplayUserTypes());
        }

        Load();
    }

    partial void OnDisplayTypeChanged(int value)
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

    partial void OnTypeChanged(int value)
    {
        AuthType type;
        if (LockLogin)
        {
            type = _locks[value].Type;
        }
        else
        {
            type = (AuthType)value;
        }
        switch (type)
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
                WatermarkName = App.Lang("UserWindow.Info9");
                if (LockLogin)
                {
                    EnableName = false;
                    Name = _locks[Type].Data;
                }
                else
                {
                    EnableName = true;
                    Name = "";
                }
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                CanRegister = true;
                break;
            case AuthType.AuthlibInjector:
                WatermarkName = App.Lang("UserWindow.Info10");
                if (LockLogin)
                {
                    EnableName = false;
                    Name = _locks[Type].Data;
                }
                else
                {
                    EnableName = true;
                    Name = "";
                }
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                CanRegister = false;
                break;
            case AuthType.LittleSkin:
                WatermarkName = "";
                if (LockLogin)
                {
                    EnableName = false;
                    Name = _locks[Type].Data;
                }
                else
                {
                    EnableName = true;
                    Name = "";
                }
                EnableUser = true;
                User = "";
                EnablePassword = true;
                Password = "";
                CanRegister = true;
                break;
            case AuthType.SelfLittleSkin:
                WatermarkName = App.Lang("UserWindow.Info11");
                if (LockLogin)
                {
                    EnableName = false;
                    Name = _locks[Type].Data;
                }
                else
                {
                    EnableName = true;
                    Name = "";
                }
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
        AuthType type;
        if (LockLogin)
        {
            type = _locks[Type].Type;
        }
        else
        {
            type = (AuthType)Type;
        }
        switch (type)
        {
            case AuthType.OAuth:
            case AuthType.Nide8:
            case AuthType.LittleSkin:
                WebBinding.OpenRegister(type, Name);
                break;
        }
    }

    [RelayCommand]
    public void SetAdd()
    {
        if (Type == -1)
        {
            Type = 0;
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
        AuthType type;
        if (LockLogin)
        {
            type = _locks[Type].Type;
        }
        else
        {
            type = (AuthType)Type;
        }
        switch (type)
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

    public void Select(UserDisplayModel item)
    {
        UserBinding.SetLastUser(item.UUID, item.AuthType);

        Model.Notify(App.Lang("UserWindow.Info5"));
        foreach (var item1 in UserList)
        {
            if (item1.AuthType == item.AuthType && item1.UUID == item.UUID)
            {
                item1.IsSelect = true;
            }
            else
            {
                item1.IsSelect = false;
            }
        }
    }

    public void Drop(IDataObject data)
    {
        if (LockLogin)
        {
            return;
        }

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
        if (LockLogin)
        {
            return;
        }

        SetAdd();
        Type = (int)AuthType.AuthlibInjector;
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
            AuthType type;
            int index = 0;
            if (LockLogin)
            {
                index = FindLockLogin(obj.AuthType, obj.Text1);
                if (index == -1)
                {
                    Model.Show(App.Lang("UserWindow.Error9"));
                    return;
                }
                var login = _locks[index];
                type = login.Type;
                Type = index;
            }
            else
            {
                type = (AuthType)Type;
            }

            switch (type)
            {
                case AuthType.Nide8:
                case AuthType.AuthlibInjector:
                case AuthType.SelfLittleSkin:
                    OnTypeChanged(Type);
                    SetAdd();
                    User = obj.Text2;
                    Name = obj.Text1;
                    break;
                case AuthType.LittleSkin:
                    OnTypeChanged(Type);
                    SetAdd();
                    User = obj.Text2;
                    break;
            }
        }
        else
        {
            Model.Notify(App.Lang("UserWindow.Info4"));
        }
    }

    private int FindLockLogin(AuthType type, string url)
    {
        for (int a = 0; a < _locks.Length; a++)
        {
            if (_locks[a].Type == type)
            {
                if (type == AuthType.OAuth)
                {
                    return a;
                }
                else if (_locks[a].Data == url)
                {
                    return a;
                }
            }
        }

        return -1;
    }

    public void ReLogin(UserDisplayModel obj)
    {
        if (LockLogin)
        {
            var index = FindLockLogin(obj.AuthType, obj.Text1);
            if (index == -1)
            {
                Model.Show(App.Lang("UserWindow.Error9"));
                return;
            }
            Type = index;
        }
        else
        {
            Type = (int)obj.AuthType;
        }

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
            if (LockLogin)
            {
                if (DisplayType <= 0)
                {
                    foreach (var item2 in _locks)
                    {
                        if (item2.Type == item.Value.AuthType && (item2.Type == AuthType.OAuth
                            || item2.Data == item.Value.Text1))
                        {
                            UserList.Add(new(this, item.Value)
                            {
                                IsSelect = item1 == item.Value
                            });
                        }
                    }
                }
                else
                {
                    var login = _locks[DisplayType - 1];
                    if (login.Type == item.Value.AuthType && (login.Type == AuthType.OAuth
                            || login.Data == item.Value.Text1))
                    {
                        UserList.Add(new(this, item.Value)
                        {
                            IsSelect = item1 == item.Value
                        });
                    }
                }
            }
            else
            {
                if (DisplayType <= 0)
                {
                    UserList.Add(new(this, item.Value)
                    {
                        IsSelect = item1 == item.Value
                    });
                }
                else if ((DisplayType - 1) == (int)item.Value.AuthType)
                {
                    UserList.Add(new(this, item.Value)
                    {
                        IsSelect = item1 == item.Value
                    });
                }
            }
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

    public override void Close()
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
        Model.PushBack(() =>
        {
            DialogHost.Close("UsersControl");
            Model.PopBack();
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
