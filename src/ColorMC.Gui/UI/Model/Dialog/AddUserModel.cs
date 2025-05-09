using System;
using System.Collections.ObjectModel;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Controls.User;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 添加账户
/// </summary>
public partial class AddUserModel : ObservableObject
{
    /// <summary>
    /// 账户类型列表
    /// </summary>
    public ObservableCollection<string> UserTypeList { get; init; } = [];

    /// <summary>
    /// 选中的账户类型
    /// </summary>
    [ObservableProperty]
    private int _type = -1;

    /// <summary>
    /// 是否允许输入名字
    /// </summary>
    [ObservableProperty]
    private bool _enableName;
    /// <summary>
    /// 是否允许输入用户名
    /// </summary>
    [ObservableProperty]
    private bool _enableUser;
    /// <summary>
    /// 是否允许输入密码
    /// </summary>
    [ObservableProperty]
    private bool _enablePassword;
    /// <summary>
    /// 是否允许注册
    /// </summary>
    [ObservableProperty]
    private bool _canRegister;

    /// <summary>
    /// 提示名字
    /// </summary>
    [ObservableProperty]
    private string _watermarkName;
    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string? _name;
    /// <summary>
    /// 用户名
    /// </summary>
    [ObservableProperty]
    private string? _user;
    /// <summary>
    /// 密码
    /// </summary>
    [ObservableProperty]
    private string? _password;

    /// <summary>
    /// 是否为锁定登录
    /// </summary>
    public bool LockLogin { get; set; }

    /// <summary>
    /// 锁定账户类型
    /// </summary>
    public LockLoginSetting[] Locks { get; set; }

    partial void OnTypeChanged(int value)
    {
        AuthType type;
        if (LockLogin)
        {
            type = Locks[value].Type;
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
                    Name = Locks[Type].Data;
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
                    Name = Locks[Type].Data;
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
                    Name = Locks[Type].Data;
                }
                else
                {
                    EnableName = true;
                    Name = "";
                }
                EnableName = false;
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
                    Name = Locks[Type].Data;
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

    partial void OnPasswordChanging(string? value)
    {
        if (value?.EndsWith(Environment.NewLine) == true)
        {
            Password = value.TrimEnd();
            Add();
        }
    }

    partial void OnUserChanging(string? value)
    {
        if (value?.EndsWith(Environment.NewLine) == true)
        {
            User = value.TrimEnd();
            Add();
        }
    }

    partial void OnNameChanging(string? value)
    {
        if (value?.EndsWith(Environment.NewLine) == true)
        {
            Name = value.TrimEnd();
            Add();
        }
    }

    [RelayCommand]
    public void Register()
    {
        AuthType type;
        if (LockLogin)
        {
            type = Locks[Type].Type;
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
    public void Add()
    {
        DialogHost.Close(UsersControl.DialogName, true);
    }

    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(UsersControl.DialogName, false);
    }
}
