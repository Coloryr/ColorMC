using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UIBinding;

public static class UserBinding
{
    /// <summary>
    /// 账户编辑回调
    /// </summary>
    public static event Action? UserEdit;

    /// <summary>
    /// 锁定的账户类型
    /// </summary>
    private static readonly List<UserKeyObj> s_lockUser = [];

    /// <summary>
    /// 添加账户
    /// </summary>
    /// <param name="type">账户类型</param>
    /// <param name="loginOAuth">UI相关</param>
    /// <param name="select">UI相关</param>
    /// <param name="input1">附加信息</param>
    /// <param name="input2">附加信息</param>
    /// <param name="input3">附加信息</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<MessageRes> AddUser(AuthType type, ColorMCCore.LoginOAuthCode loginOAuth, ColorMCCore.Select? select,
        string? input1 = null, string? input2 = null, string? input3 = null)
    {
        if (type == AuthType.Offline)
        {
            var user = new LoginObj()
            {
                UserName = input1!,
                ClientToken = FuntionUtils.NewUUID(),
                UUID = HashHelper.GenMd5(Encoding.UTF8.GetBytes(input1!.ToLower())),
                AuthType = AuthType.Offline
            };
            user.Save();
            SetSelectUser(user.UUID, user.AuthType);
            return new() { State = true };
        }
        var res1 = type switch
        {
            AuthType.OAuth => await GameAuth.LoginOAuthAsync(loginOAuth),
            AuthType.Nide8 => await GameAuth.LoginNide8Async(input1!, input2!, input3!),
            AuthType.AuthlibInjector => await GameAuth.LoginAuthlibInjectorAsync(input1!, input2!, input3!, select),
            AuthType.LittleSkin => await GameAuth.LoginLittleSkinAsync(input1!, input2!, select),
            AuthType.SelfLittleSkin => await GameAuth.LoginLittleSkinAsync(input1!, input2!, select, input3!),
            _ => throw new Exception(App.Lang("UserBinding.Error2"))
        };

        if (res1.LoginState != LoginState.Done)
        {
            if (res1.Ex != null)
            {
                WindowManager.ShowError(res1.Message!, res1.Ex);
                return new() { Message = App.Lang("UserBinding.Error1") };
            }
            else
            {
                return new() { Message = res1.Message };
            }
        }
        if (string.IsNullOrWhiteSpace(res1.Auth?.UUID))
        {
            WebBinding.OpenWeb(WebType.Minecraft);
            return new() { Message = App.Lang("UserBinding.Error3") };
        }
        res1.Auth!.Save();
        SetSelectUser(res1.Auth.UUID, res1.Auth.AuthType);
        return new() { State = true };
    }

    /// <summary>
    /// 获取所有账户
    /// </summary>
    /// <returns></returns>
    public static Dictionary<UserKeyObj, LoginObj> GetAllUser()
    {
        return AuthDatabase.Auths;
    }

    /// <summary>
    /// 删除账户
    /// </summary>
    /// <param name="uuid">UUID</param>
    /// <param name="type">账户类型</param>
    public static void Remove(string uuid, AuthType type)
    {
        if (GuiConfigUtils.Config.LastUser is { } last
            && type == last.Type && uuid == last.UUID)
        {
            ClearLastUser();
        }
        AuthDatabase.Get(uuid, type)?.Delete();

        OnUserEdit();
    }

    /// <summary>
    /// 获取所有账户
    /// </summary>
    /// <returns></returns>
    public static LoginObj? GetLastUser()
    {
        var obj = GuiConfigUtils.Config?.LastUser;
        if (obj == null)
        {
            return null;
        }
        return AuthDatabase.Get(obj.UUID, obj.Type);
    }

    /// <summary>
    /// 重新登录账户
    /// </summary>
    /// <param name="uuid">UUID</param>
    /// <param name="type">账户类型</param>
    /// <returns></returns>
    public static async Task<bool> ReLogin(string uuid, AuthType type)
    {
        var obj = AuthDatabase.Get(uuid, type);
        if (obj == null)
        {
            return false;
        }

        //obj.AccessToken = "";
        return (await obj.RefreshTokenAsync()).LoginState == LoginState.Done;
    }

    /// <summary>
    /// 设置选中账户
    /// </summary>
    /// <param name="uuid">UUID</param>
    /// <param name="type">账户类型</param>
    public static void SetSelectUser(string uuid, AuthType type)
    {
        GuiConfigUtils.Config.LastUser = new()
        {
            Type = type,
            UUID = uuid
        };

        GuiConfigUtils.Save();

        OnUserEdit();
    }

    /// <summary>
    /// 删除选中账户
    /// </summary>
    public static void ClearLastUser()
    {
        GuiConfigUtils.Config.LastUser = null;
        GuiConfigUtils.Save();
    }

    /// <summary>
    /// 锁定账户
    /// </summary>
    /// <param name="obj">账户</param>
    public static void AddLockUser(LoginObj obj)
    {
        var key = obj.GetKey();
        if (!s_lockUser.Contains(key))
        {
            s_lockUser.Add(key);
        }
    }

    /// <summary>
    /// 解锁账户
    /// </summary>
    /// <param name="obj">账户</param>
    public static void UnLockUser(LoginObj obj)
    {
        s_lockUser.Remove(obj.GetKey());
    }

    /// <summary>
    /// 账户是否锁定
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    public static bool IsLock(LoginObj obj)
    {
        return s_lockUser.Contains(obj.GetKey());
    }

    /// <summary>
    /// 加载账户皮肤
    /// </summary>
    /// <returns></returns>
    public static async Task LoadSkin()
    {
        var obj = GetLastUser();

        if (obj == null)
        {
            ImageManager.SetDefaultHead();
            return;
        }

        await ImageManager.LoadSkinHead(obj);
    }

    /// <summary>
    /// 重新绘制账户皮肤
    /// </summary>
    public static void ReloadSkin()
    {
        var obj = GetLastUser();

        if (obj == null)
        {
            return;
        }

        ImageManager.ReloadSkinHead();
    }

    /// <summary>
    /// 编辑皮肤
    /// </summary>
    /// <param name="top">窗口</param>
    public static async void EditSkin(TopLevel top)
    {
        var obj = GetLastUser();
        if (obj == null)
        {
            return;
        }

        switch (obj.AuthType)
        {
            case AuthType.Offline:
                var file = await PathBinding.SelectFile(top, FileType.Head);
                if (file.Item1 != null)
                {
                    obj.SaveSkin(file.Item1);
                }
                break;
            case AuthType.OAuth:
                WebBinding.OpenWeb(WebType.EditSkin);
                break;
            case AuthType.Nide8:
                BaseBinding.OpenUrl($"https://login.mc-user.com:233/{obj.Text1}/skin");
                break;
            //case AuthType.AuthlibInjector:
            //BaseBinding.OpUrl($"https://login.mc-user.com:233/{obj.Text1}/skin");
            //break;
            case AuthType.LittleSkin:
                WebBinding.OpenWeb(WebType.LittleSkinEditSkin);
                break;
            case AuthType.SelfLittleSkin:
                BaseBinding.OpenUrl($"{obj.Text1}/user/closet");
                break;
        }
    }

    /// <summary>
    /// 清空所有账户
    /// </summary>
    public static void ClearAllUser()
    {
        AuthDatabase.ClearAuths();

        GuiConfigUtils.Config.LastUser = null;
        GuiConfigUtils.Save();

        OnUserEdit();
    }

    /// <summary>
    /// 取消微软登录
    /// </summary>
    public static void OAuthCancel()
    {
        GameAuth.CancelWithOAuth();
    }

    /// <summary>
    /// 编辑账户
    /// </summary>
    /// <param name="obj">原来的账户</param>
    /// <param name="text1">新的名字</param>
    /// <param name="text2">新的UUID</param>
    public static void EditUser(LoginObj obj, string text1, string text2)
    {
        foreach (var item in AuthDatabase.Auths.Values)
        {
            if (item.UserName == obj.UserName && item.UUID == obj.UUID && item.AuthType == AuthType.Offline)
            {
                item.UserName = text1;
                item.UUID = text2;
                AuthDatabase.Save();
                break;
            }
        }
    }

    /// <summary>
    /// 是否有微软账户
    /// </summary>
    /// <returns></returns>
    public static bool HaveOnline()
    {
        return AuthDatabase.Auths.Keys.Any(a => a.Type == AuthType.OAuth);
    }

    /// <summary>
    /// 测试登录
    /// </summary>
    /// <param name="user">账户</param>
    /// <returns></returns>
    public static async Task<bool> TestLogin(LoginObj user)
    {
        return (await user.RefreshTokenAsync()).LoginState == LoginState.Done;
    }

    /// <summary>
    /// 当账户编辑后
    /// </summary>
    public static void OnUserEdit()
    {
        UserEdit?.Invoke();
    }

    /// <summary>
    /// 获取选中的账户
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<GameLaunchUserRes> GetLaunchUser(BaseModel model)
    {
        var login = GetLastUser();
        if (login == null)
        {
            return new() { Message = App.Lang("GameBinding.Error3") };
        }
        if (login.AuthType == AuthType.Offline)
        {
            var have = AuthDatabase.Auths.Keys.Any(a => a.Type == AuthType.OAuth);
            if (!have)
            {
                WebBinding.OpenWeb(WebType.Minecraft);
                return new() { Message = App.Lang("GameBinding.Error4") };
            }
        }

        if (IsLock(login) && GuiConfigUtils.Config.LaunchCheck.CheckUser)
        {
            var res = await model.ShowAsync(App.Lang("GameBinding.Error1"));
            if (!res)
            {
                return new() { Message = App.Lang("GameBinding.Error5") };
            }

            res = await model.ShowAsync(App.Lang("GameBinding.Info18"));
            if (res)
            {
                GuiConfigUtils.Config.LaunchCheck.CheckUser = false;
                GuiConfigUtils.Save();
            }
        }

        return new() { User = login };
    }
}
