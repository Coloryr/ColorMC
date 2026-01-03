using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Web;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.User;

/// <summary>
/// 账户页面
/// </summary>
public partial class UsersModel : ControlModel
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

    public CancellationToken Token => _tokenSource.Token;

    /// <summary>
    /// 选中的账户
    /// </summary>
    [ObservableProperty]
    private UserDisplayModel? _item;
    /// <summary>
    /// 用户展示类型
    /// </summary>
    [ObservableProperty]
    private ItemsGridType _gridType = ItemsGridType.Grid;

    /// <summary>
    /// 账户类型
    /// </summary>
    [ObservableProperty]
    private int _displayType;
    /// <summary>
    /// 所有账户数量
    /// </summary>
    [ObservableProperty]
    private int _userCount;
    /// <summary>
    /// 当前分类账户数量
    /// </summary>
    [ObservableProperty]
    private int _listUserCount;

    [ObservableProperty]
    private int _gridCount;

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

    private CancellationTokenSource _tokenSource;

    public UsersModel(WindowModel model) : base(model)
    {
        LoadUsers();

        EventManager.LockUserChange += EventManager_LockUserChange;
        EventManager.SkinChange += EventManager_SkinChange;
    }

    private void EventManager_SkinChange()
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var item in UserList)
            {
                item.ReloadHead();
            }
        });
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
        ShowAdd(new AddUserModel(Window.WindowId));
    }

    private void EventManager_LockUserChange()
    {
        LoadUsers();
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
        
        var gui = new LoginGui(Window);

        ReadyCancel();

        switch (type)
        {
            case AuthType.Offline:
                var name = model.User;
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(name))
                {
                    Window.Show(LangUtils.Get("SettingWindow.Tab5.Text21"));
                    break;
                }
                var res = await UserBinding.AddUserAsync(AuthType.Offline, new LoginOAuthGui(this, null), gui, Token, name);
                if (!res.State)
                {
                    Window.Show(res.Data!);
                    break;
                }
                Window.Notify(LangUtils.Get("UserWindow.Text20"));
                break;
            case AuthType.OAuth:
                _cancel = false;
                _isOAuth = true;
                var dialog = Window.ShowProgress(LangUtils.Get("Text.Loading"));
                res = await UserBinding.AddUserAsync(AuthType.OAuth, new LoginOAuthGui(this, dialog), gui, Token);
                Window.CloseDialog(dialog);
                if (_cancel)
                {
                    break;
                }
                if (!res.State)
                {
                    Window.Show(res.Data!);
                    break;
                }
                Window.Notify(LangUtils.Get("UserWindow.Text20"));
                _isOAuth = false;
                break;
            case AuthType.Nide8:
                var server = model.Name;
                _isOAuth = false;
                if (server?.Length != 32)
                {
                    Window.Show(LangUtils.Get("UserWindow.Text23"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(model.User) ||
                    string.IsNullOrWhiteSpace(model.Password))
                {
                    Window.Show(LangUtils.Get("SettingWindow.Tab5.Text21"));
                    break;
                }
                dialog = Window.ShowProgress(LangUtils.Get("UserWindow.Text11"));
                res = await UserBinding.AddUserAsync(AuthType.Nide8, new LoginOAuthGui(this, dialog), gui, Token,
                     model.User, model.Password, server);
                Window.CloseDialog(dialog);
                if (!res.State)
                {
                    Window.Show(res.Data!);
                    break;
                }
                Window.Notify(LangUtils.Get("UserWindow.Text20"));
                break;
            case AuthType.AuthlibInjector:
                server = model.Name;
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Window.Show(LangUtils.Get("UserWindow.Text24"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(model.User) ||
                   string.IsNullOrWhiteSpace(model.Password))
                {
                    Window.Show(LangUtils.Get("SettingWindow.Tab5.Text21"));
                    break;
                }
                dialog = Window.ShowProgress(LangUtils.Get("UserWindow.Text11"));
                res = await UserBinding.AddUserAsync(AuthType.AuthlibInjector, new LoginOAuthGui(this, dialog),
                    gui, Token, model.User, model.Password, server);
                Window.CloseDialog(dialog);
                if (!res.State)
                {
                    Window.Show(res.Data!);
                    break;
                }
                Window.Notify(LangUtils.Get("UserWindow.Text20"));
                break;
            case AuthType.LittleSkin:
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(model.User) ||
                   string.IsNullOrWhiteSpace(model.Password))
                {
                    Window.Show(LangUtils.Get("SettingWindow.Tab5.Text21"));
                    break;
                }
                dialog = Window.ShowProgress(LangUtils.Get("UserWindow.Text11"));
                res = await UserBinding.AddUserAsync(AuthType.LittleSkin, new LoginOAuthGui(this, dialog),
                    gui, Token, model.User, model.Password);
                Window.CloseDialog(dialog);
                if (!res.State)
                {
                    Window.Show(res.Data!);
                    break;
                }
                Window.Notify(LangUtils.Get("UserWindow.Text20"));
                break;
            case AuthType.SelfLittleSkin:
                server = model.Name;
                _isOAuth = false;
                if (string.IsNullOrWhiteSpace(server))
                {
                    Window.Show(LangUtils.Get("UserWindow.Text24"));
                    break;
                }
                if (string.IsNullOrWhiteSpace(model.User) ||
                   string.IsNullOrWhiteSpace(model.Password))
                {
                    Window.Show(LangUtils.Get("SettingWindow.Tab5.Text21"));
                    break;
                }
                dialog = Window.ShowProgress(LangUtils.Get("UserWindow.Text11"));
                res = await UserBinding.AddUserAsync(AuthType.SelfLittleSkin, new LoginOAuthGui(this, dialog),
                    gui, Token, model.User, model.Password, server);
                Window.CloseDialog(dialog);
                if (!res.State)
                {
                    Window.Show(res.Data!);
                    break;
                }
                Window.Notify(LangUtils.Get("UserWindow.Text20"));
                break;
            default:
                Window.Show(LangUtils.Get("UserWindow.Text25"));
                break;
        }
        Load();
    }

    /// <summary>
    /// 删除账户
    /// </summary>
    /// <param name="item"></param>
    public void Remove(UserDisplayModel item)
    {
        UserManager.Remove(item.UUID, item.AuthType);
        Load();
    }

    /// <summary>
    /// 选择账户
    /// </summary>
    /// <param name="item"></param>
    public void Select(UserDisplayModel item)
    {
        UserManager.SetSelect(item.Obj);

        Window.Notify(LangUtils.Get("UserWindow.Text14"));
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
    public void Drop(IDataTransfer data)
    {
        if (LockLogin || !data.Contains(DataFormat.Text))
        {
            return;
        }

        var str = data.TryGetText();
        if (str?.StartsWith(GuiNames.NameAuthlibKey) == true)
        {
            AddUrl(str);
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

        ShowAdd(new AddUserModel(Window.WindowId)
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
        var dialog = Window.ShowProgress(LangUtils.Get("UserWindow.Text12"));
        ReadyCancel();
        var res = await UserBinding.ReLoginAsync(obj.UUID, obj.AuthType, Token);
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            Window.Show(string.Format(LangUtils.Get("UserWindow.Text26"), res.Data));
            AuthType type;
            if (LockLogin)
            {
                var index = FindLockLogin(obj.AuthType, obj.Text1);
                if (index == -1)
                {
                    Window.Show(LangUtils.Get("UserWindow.Text29"));
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
                    ShowAdd(new AddUserModel(Window.WindowId)
                    {
                        Type = (int)type,
                        User = obj.Text2,
                        Name = obj.Text1
                    });
                    break;
                case AuthType.LittleSkin:
                    ShowAdd(new AddUserModel(Window.WindowId)
                    {
                        Type = (int)type,
                        User = obj.Text2
                    });
                    break;
            }
        }
        else
        {
            Window.Notify(LangUtils.Get("UserWindow.Text13"));
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
                Window.Show(LangUtils.Get("UserWindow.Text29"));
                return;
            }
            type = index;
        }
        else
        {
            type = (int)obj.AuthType;
        }

        ShowAdd(new AddUserModel(Window.WindowId)
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
            _userTypeList.AddRange(LangUtils.GetLoginUserType());
            DisplayUserTypeList.AddRange(LangUtils.GetDisplayUserTypes());
        }

        Load();
        OnPropertyChanged(nameof(HaveLogin));
    }

    /// <summary>
    /// 加载账户
    /// </summary>
    public void Load()
    {
        var select = UserManager.GetLastUser();
        foreach (var item in UserList)
        {
            item.Close();
        }
        UserList.Clear();

        var list = new List<LoginObj>();

        foreach (var item in AuthDatabase.Auths)
        {
            //是否为锁定账户模式
            if (LockLogin)
            {
                foreach (var item2 in _locks)
                {
                    if (item2.Type == item.Value.AuthType && (item2.Type == AuthType.OAuth
                        || item2.Data == item.Value.Text1))
                    {
                        list.Add(item.Value);
                    }

                    UserCount = UserList.Count;
                }
            }
            else
            {
                list.Add(item.Value);
            }
        }

        UserCount = list.Count;

        if (LockLogin)
        {
            //选中了分类
            if (DisplayType <= 0)
            {
                foreach (var item in list)
                {
                    UserList.Add(new UserDisplayModel(this, item)
                    {
                        IsSelect = select == item
                    });
                }
            }
            else
            {
                var login = _locks[DisplayType - 1];
                foreach (var item in list)
                {
                    if (login.Type == item.AuthType && (login.Type == AuthType.OAuth
                        || login.Data == item.Text1))
                    {
                        UserList.Add(new UserDisplayModel(this, item)
                        {
                            IsSelect = select == item
                        });
                    }
                }
            }
        }
        else
        {
            foreach (var item in list)
            {
                if (DisplayType <= 0)
                {
                    UserList.Add(new UserDisplayModel(this, item)
                    {
                        IsSelect = select == item
                    });
                }
                else if ((DisplayType - 1) == (int)item.AuthType)
                {
                    UserList.Add(new UserDisplayModel(this, item)
                    {
                        IsSelect = select == item
                    });
                }
            }
        }

        ListUserCount = UserList.Count;
    }

    /// <summary>
    /// 取消登录
    /// </summary>
    public void SetCancel()
    {
        _cancel = true;
        _tokenSource.Cancel();
    }

    public override void Close()
    {
        EventManager.LockUserChange -= EventManager_LockUserChange;
        EventManager.SkinChange -= EventManager_SkinChange;

        foreach (var item in UserList)
        {
            item.Close();
        }
        UserList.Clear();
        if (_isOAuth)
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }

    /// <summary>
    /// 重新登录
    /// </summary>
    public void ReLogin()
    {
        var user = UserManager.GetLastUser();
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

        var dialog = new InputModel(Window.WindowId)
        {
            Text1 = obj.Name,
            Text2 = obj.UUID,
            Watermark1 = LangUtils.Get("UserWindow.Text10"),
            Watermark2 = LangUtils.Get("UserWindow.Text21")
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return;
        }
        else if (string.IsNullOrWhiteSpace(dialog.Text1) || string.IsNullOrWhiteSpace(dialog.Text2))
        {
            Window.Show(LangUtils.Get("UserWindow.Text27"));
            return;
        }
        else if (dialog.Text2.Length != 32 || !CheckHelpers.CheckIs(dialog.Text2))
        {
            Window.Show(LangUtils.Get("UserWindow.Text28"));
            return;
        }

        UserManager.EditUser(obj.Obj, dialog.Text1, dialog.Text2);
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

        var res = await Window.ShowDialogWait(model);
        if (res is not true)
        {
            return;
        }
        Add(model);
    }

    private void ReadyCancel()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new();
    }
}
