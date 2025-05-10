using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Controls.User;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.User;

/// <summary>
/// 账户页面
/// </summary>
public partial class UsersModel : TopModel
{
    /// <summary>
    /// 当前账户类型列表
    /// </summary>
    public ObservableCollection<string> DisplayUserTypeList { get; init; } = [];
    /// <summary>
    /// 账户列表
    /// </summary>
    public ObservableCollection<UserDisplayModel> UserList { get; init; } = [];

    /// <summary>
    /// 是否锁定登录
    /// </summary>
    public bool LockLogin { get; private set; }

    /// <summary>
    /// 是否有登录账户
    /// </summary>
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

    /// <summary>
    /// 选中的账户
    /// </summary>
    [ObservableProperty]
    private UserDisplayModel? _item;

    /// <summary>
    /// 账户类型
    /// </summary>
    [ObservableProperty]
    private int _displayType;

    /// <summary>
    /// 账户类型列表
    /// </summary>
    private readonly List<string> _userTypeList = [];

    /// <summary>
    /// 锁定账户类型
    /// </summary>
    private LockLoginSetting[] _locks;

    /// <summary>
    /// 是否取消登录
    /// </summary>
    private bool _cancel;
    /// <summary>
    /// 是否为OAuth
    /// </summary>
    private bool _isOAuth;

    public UsersModel(BaseModel model) : base(model)
    {
        LoadUsers();
    }

    partial void OnDisplayTypeChanged(int value)
    {
        Load();
    }

    /// <summary>
    /// 开始添加账户
    /// </summary>
    [RelayCommand]
    public void SetAdd()
    {
        ShowAdd(new());
    }

    /// <summary>
    /// 添加账户
    /// </summary>
    /// <param name="model"></param>
    private async void Add(AddUserModel model)
    {
        AuthType type;
        if (LockLogin)
        {
            type = _locks[model.Type].Type;
        }
        else
        {
            type = (AuthType)model.Type;
        }
        switch (type)
        {
            case AuthType.Offline:
                var name = model.User;
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(name))
                {
                    Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
                    break;
                }
                var res = await UserBinding.AddUser(AuthType.Offline, LoginOAuthCode, LoginSelect, name);
                if (!res.State)
                {
                    Model.Show(res.Message!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                break;
            case AuthType.OAuth:
                _cancel = false;
                _isOAuth = true;
                Model.Progress(App.Lang("UserWindow.Info1"));
                res = await UserBinding.AddUser(AuthType.OAuth, LoginOAuthCode, LoginSelect);
                Model.ProgressClose();
                if (_cancel)
                {
                    break;
                }
                if (!res.State)
                {
                    Model.Show(res.Message!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                _isOAuth = false;
                break;
            case AuthType.Nide8:
                var server = model.Name;
                _isOAuth = false;
                if (server?.Length != 32)
                {
                    Model.Show(App.Lang("UserWindow.Error3"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(model.User) ||
                    string.IsNullOrWhiteSpace(model.Password))
                {
                    Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
                    break;
                }
                Model.Progress(App.Lang("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.Nide8, LoginOAuthCode, LoginSelect,
                    server, model.User, model.Password);
                Model.ProgressClose();
                if (!res.State)
                {
                    Model.Show(res.Message!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                break;
            case AuthType.AuthlibInjector:
                server = model.Name;
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Model.Show(App.Lang("UserWindow.Error4"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(model.User) ||
                   string.IsNullOrWhiteSpace(model.Password))
                {
                    Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
                    break;
                }
                Model.Progress(App.Lang("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.AuthlibInjector, LoginOAuthCode,
                    LoginSelect, server, model.User, model.Password);
                Model.ProgressClose();
                if (!res.State)
                {
                    Model.Show(res.Message!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                break;
            case AuthType.LittleSkin:
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(model.User) ||
                   string.IsNullOrWhiteSpace(model.Password))
                {
                    Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
                    break;
                }
                Model.Progress(App.Lang("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.LittleSkin, LoginOAuthCode,
                    LoginSelect, model.User, model.Password);
                Model.ProgressClose();
                if (!res.State)
                {
                    Model.Show(res.Message!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                break;
            case AuthType.SelfLittleSkin:
                server = model.Name;
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Model.Show(App.Lang("UserWindow.Error4"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(model.User) ||
                   string.IsNullOrWhiteSpace(model.Password))
                {
                    Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
                    break;
                }
                Model.Progress(App.Lang("UserWindow.Info2"));
                res = await UserBinding.AddUser(AuthType.SelfLittleSkin, LoginOAuthCode,
                    LoginSelect, model.User, model.Password, server);
                Model.ProgressClose();
                if (!res.State)
                {
                    Model.Show(res.Message!);
                    break;
                }
                Model.Notify(App.Lang("UserWindow.Info12"));
                break;
            default:
                Model.Show(App.Lang("UserWindow.Error5"));
                break;
        }
        Load();
    }

    /// <summary>
    /// 登录账户选择
    /// </summary>
    /// <param name="title"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    private async Task<int> LoginSelect(string title, List<string> items)
    {
        var res = await Model.Combo(title, items);
        if (res.Cancel)
        {
            return -1;
        }
        return res.Index;
    }

    /// <summary>
    /// 删除账户
    /// </summary>
    /// <param name="item"></param>
    public void Remove(UserDisplayModel item)
    {
        UserBinding.Remove(item.UUID, item.AuthType);
        Load();
    }

    /// <summary>
    /// 选择账户
    /// </summary>
    /// <param name="item"></param>
    public void Select(UserDisplayModel item)
    {
        UserBinding.SetSelectUser(item.UUID, item.AuthType);

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

    /// <summary>
    /// 拖拽添加账户
    /// </summary>
    /// <param name="data"></param>
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

    /// <summary>
    /// 通过网址添加账户
    /// </summary>
    /// <param name="url"></param>
    public void AddUrl(string url)
    {
        if (LockLogin)
        {
            return;
        }

        ShowAdd(new()
        {
            Type = (int)AuthType.AuthlibInjector,
            Name = HttpUtility.UrlDecode(url.Replace("authlib-injector:yggdrasil-server:", ""))
        });
    }

    /// <summary>
    /// 刷新账户token
    /// </summary>
    /// <param name="obj"></param>
    public async void Refresh(UserDisplayModel obj)
    {
        Model.Progress(App.Lang("UserWindow.Info3"));
        var res = await UserBinding.ReLogin(obj.UUID, obj.AuthType);
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("UserWindow.Error6"));
            AuthType type;
            if (LockLogin)
            {
                var index = FindLockLogin(obj.AuthType, obj.Text1);
                if (index == -1)
                {
                    Model.Show(App.Lang("UserWindow.Error9"));
                    return;
                }
                var login = _locks[index];
                type = login.Type;
            }
            else
            {
                type = obj.AuthType;
            }

            switch (type)
            {
                case AuthType.Nide8:
                case AuthType.AuthlibInjector:
                case AuthType.SelfLittleSkin:
                    ShowAdd(new()
                    {
                        Type = (int)type,
                        User = obj.Text2,
                        Name = obj.Text1
                    });
                    break;
                case AuthType.LittleSkin:
                    ShowAdd(new()
                    {
                        Type = (int)type,
                        User = obj.Text2
                    });
                    break;
            }
        }
        else
        {
            Model.Notify(App.Lang("UserWindow.Info4"));
        }
    }

    /// <summary>
    /// 检索锁定的账户
    /// </summary>
    /// <param name="type"></param>
    /// <param name="url"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 重新登录账户
    /// </summary>
    /// <param name="obj"></param>
    public void Relogin(UserDisplayModel obj)
    {
        int type;
        if (LockLogin)
        {
            var index = FindLockLogin(obj.AuthType, obj.Text1);
            if (index == -1)
            {
                Model.Show(App.Lang("UserWindow.Error9"));
                return;
            }
            type = index;
        }
        else
        {
            type = (int)obj.AuthType;
        }

        ShowAdd(new()
        {
            Type = type,
            EnableName = false,
            EnableUser = false,
            Name = obj.Text1,
            User = obj.Text2,
        });
    }

    /// <summary>
    /// 加载账户列表
    /// </summary>
    public void LoadUsers()
    {
        var config = GuiConfigUtils.Config.ServerCustom;
        LockLogin = config.LockLogin;
        _userTypeList.Clear();
        DisplayUserTypeList.Clear();
        UserList.Clear();
        if (LockLogin)
        {
            _locks = [.. config.LockLogins];
            DisplayUserTypeList.Add("");
            foreach (var item in _locks)
            {
                if (item.Type == AuthType.OAuth)
                {
                    _userTypeList.Add(AuthType.OAuth.GetName());
                    DisplayUserTypeList.Add(AuthType.OAuth.GetName());
                }
                else
                {
                    _userTypeList.Add(item.Name);
                    DisplayUserTypeList.Add(item.Name);
                }
            }
        }
        else
        {
            _userTypeList.AddRange(LanguageBinding.GetLoginUserType());
            DisplayUserTypeList.AddRange(LanguageBinding.GetDisplayUserTypes());
        }

        Load();
        OnPropertyChanged(nameof(HaveLogin));
    }

    /// <summary>
    /// 加载账户
    /// </summary>
    public void Load()
    {
        var item1 = UserBinding.GetLastUser();
        foreach (var item in UserList)
        {
            item.Close();
        }
        UserList.Clear();
        foreach (var item in UserBinding.GetAllUser())
        {
            //是否为锁定账户模式
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

    /// <summary>
    /// 展示OAuth信息
    /// </summary>
    /// <param name="url"></param>
    /// <param name="code"></param>
    private async void LoginOAuthCode(string url, bool iscode, string code)
    {
        if (iscode)
        {
            Model.ProgressClose();
            Model.InputWithReadInfo(string.Format(App.Lang("UserWindow.Info6"), url),
                string.Format(App.Lang("UserWindow.Info7"), code), true, false, true, () =>
                {
                    _cancel = true;
                    UserBinding.OAuthCancel();
                });
            BaseBinding.OpenUrl($"{url}?otc={code}");
            var top = Model.GetTopLevel();
            if (top == null)
            {
                return;
            }
            await BaseBinding.CopyTextClipboard(top, code);
        }
        else
        {
            Model.Progress(string.Format(App.Lang("UserWindow.Info14"), code));
        }
    }

    public override void Close()
    {
        foreach (var item in UserList)
        {
            item.Close();
        }
        UserList.Clear();
        if (_isOAuth)
        {
            UserBinding.OAuthCancel();
        }
    }

    /// <summary>
    /// 重新登录
    /// </summary>
    public void ReLogin()
    {
        var user = UserBinding.GetLastUser();
        if (user == null)
        {
            return;
        }
        var obj = UserList.Where(item => item.AuthType == user.AuthType && item.UUID == user.UUID)
            .FirstOrDefault();
        if (obj == null)
        {
            return;
        }
        Relogin(obj);
    }

    /// <summary>
    /// 编辑账户
    /// </summary>
    /// <param name="obj"></param>
    public async void Edit(UserDisplayModel obj)
    {
        if (obj.AuthType != AuthType.Offline)
        {
            return;
        }

        var res = await Model.InputWithEdit(obj.Name, obj.UUID,
            App.Lang("UserWindow.Text10"), App.Lang("UserWindow.Info13"));
        if (res.Cancel)
        {
            return;
        }
        else if (string.IsNullOrWhiteSpace(res.Text1) || string.IsNullOrWhiteSpace(res.Text2))
        {
            Model.Show(App.Lang("UserWindow.Error7"));
            return;
        }
        else if (res.Text2.Length != 32 || !CheckHelpers.CheckIs(res.Text2))
        {
            Model.Show(App.Lang("UserWindow.Error8"));
            return;
        }

        UserBinding.EditUser(obj.Obj, res.Text1, res.Text2);
    }

    /// <summary>
    /// 显示添加账户
    /// </summary>
    /// <param name="model"></param>
    private async void ShowAdd(AddUserModel model)
    {
        model.UserTypeList.AddRange(_userTypeList);
        model.LockLogin = LockLogin;
        if (_locks != null)
        {
            model.Locks = [.. _locks];
        }

        if (DisplayType != 0)
        {
            model.Type = DisplayType - 1;
        }

        if (DialogHost.IsDialogOpen(UsersControl.DialogName))
        {
            DialogHost.Close(UsersControl.DialogName, false);
        }

        var res = await DialogHost.Show(model, UsersControl.DialogName);
        if (res is not true)
        {
            return;
        }
        Add(model);
    }

    protected override void MinModeChange()
    {
        foreach (var item in UserList)
        {
            item.SetMin(MinMode);
        }
    }

    public void ReloadHead()
    {
        foreach (var item in UserList)
        {
            item.ReloadHead();
        }
    }
}
