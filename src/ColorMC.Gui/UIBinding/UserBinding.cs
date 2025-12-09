using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using ColorMC.Core.Game;
using ColorMC.Core.GuiHandle;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
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
    public static async Task<StringRes> AddUserAsync(AuthType type, ILoginOAuthGui loginOAuth, ILoginGui select,
        string? input1 = null, string? input2 = null, string? input3 = null)
    {
        if (type == AuthType.Offline)
        {
            var user = new LoginObj()
            {
                UserName = input1!,
                ClientToken = FunctionUtils.NewUUID(),
                UUID = HashHelper.GenMd5(Encoding.UTF8.GetBytes(input1!.ToLower())),
                AuthType = AuthType.Offline
            };
            user.Save();
            SetSelectUser(user.UUID, user.AuthType);
            return new() { State = true };
        }
        string uuid = type switch
        {
            AuthType.Nide8 => input2!,
            AuthType.AuthlibInjector => input2!,
            AuthType.LittleSkin => input1!,
            AuthType.SelfLittleSkin => input1!,
            _ => ""
        };

        try
        {
            var res1 = type switch
            {
                AuthType.OAuth => await GameAuth.LoginOAuthAsync(loginOAuth),
                AuthType.Nide8 => await GameAuth.LoginNide8Async(input1!, input2!, input3!, loginOAuth.Token),
                AuthType.AuthlibInjector => await GameAuth.LoginAuthlibInjectorAsync(input1!, input2!, input3!, select, loginOAuth.Token),
                AuthType.LittleSkin => await GameAuth.LoginLittleSkinAsync(input1!, input2!, select, loginOAuth.Token),
                AuthType.SelfLittleSkin => await GameAuth.LoginLittleSkinAsync(input1!, input2!, select, loginOAuth.Token, input3!),
                _ => throw new Exception(LanguageUtils.Get("App.Text78"))
            };
            if (loginOAuth.Token.IsCancellationRequested || res1 == null)
            {
                return new StringRes();
            }
            if (string.IsNullOrWhiteSpace(res1.UUID))
            {
                WebBinding.OpenWeb(WebType.Minecraft);
                return new StringRes { Data = LanguageUtils.Get("App.Text79") };
            }
            res1.Save();
            SetSelectUser(res1.UUID, res1.AuthType);
            return new StringRes { State = true };
        }
        catch (Exception e)
        {
            string title;
            if (e is LoginException e1)
            {
                title = e1.State.GetNameFail();
                string data = e1.Fail.GetName(type, uuid);
                string text = "";
                if (e1.Fail == LoginFailState.GetDataFail)
                {
                    text = title + Environment.NewLine + data;
                }
                else if (e1.Fail == LoginFailState.GetDataError)
                {
                    text = title + Environment.NewLine + string.Format(data, e1.Json);
                }
                if (e1.Inner != null)
                {
                    Logs.Error(text, e1.Inner);
                    WindowManager.ShowError(title, data, e1.Inner);
                }
                else
                {
                    Logs.Error(text);
                }
            }
            else
            {
                title = LanguageUtils.Get("App.Text104");
                Logs.Error(title, e);
                WindowManager.ShowError(title, e);
            }

            return new StringRes { Data = title };
        }
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
            ConfigBinding.ClearLastUser();
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
    public static async Task<StringRes> ReLoginAsync(string uuid, AuthType type, CancellationToken token)
    {
        var obj = AuthDatabase.Get(uuid, type);
        if (obj == null)
        {
            return new StringRes() { Data = LanguageUtils.Get("App.Text105") };
        }

        try
        {
            await obj.RefreshTokenAsync(token);
        }
        catch (Exception e)
        {
            string title;
            if (e is LoginException e1)
            {
                title = e1.State.GetNameFail();
                string data = e1.Fail.GetName(type, uuid);
                string text = "";
                if (e1.Fail == LoginFailState.GetDataFail)
                {
                    text = title + Environment.NewLine + data;
                }
                else if (e1.Fail == LoginFailState.GetDataError)
                {
                    text = title + Environment.NewLine + string.Format(data, e1.Json);
                }
                if (e1.Inner != null)
                {
                    Logs.Error(text, e1.Inner);
                    WindowManager.ShowError(title, data, e1.Inner);
                }
                else
                {
                    Logs.Error(text);
                }
            }
            else
            {
                title = LanguageUtils.Get("App.Text106");
                Logs.Error(title, e);
                WindowManager.ShowError(title, e);
            }

            return new StringRes() { Data = title };
        }

        return new StringRes() { State = true };
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

        await ImageManager.LoadSkinHeadAsync(obj);
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
                var file = await PathBinding.SelectFileAsync(top, FileType.Head);
                if (file.Path != null)
                {
                    obj.SaveSkin(file.Path);
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
    public static async Task<StringRes> TestLogin(LoginObj user, CancellationToken token)
    {
        try
        {
            await user.RefreshTokenAsync(token);

            return new StringRes();
        }
        catch (Exception e)
        {
            string title;
            if (e is LoginException e1)
            {
                title = e1.State.GetNameFail();
                string data = e1.Fail.GetName(user.AuthType, user.UUID);
                string text = "";
                if (e1.Fail == LoginFailState.GetDataFail)
                {
                    text = title + Environment.NewLine + data;
                }
                else if (e1.Fail == LoginFailState.GetDataError)
                {
                    text = title + Environment.NewLine + string.Format(data, e1.Json);
                }
                if (e1.Inner != null)
                {
                    Logs.Error(text, e1.Inner);
                    WindowManager.ShowError(title, data, e1.Inner);
                }
                else
                {
                    Logs.Error(text);
                }
            }
            else
            {
                title = LanguageUtils.Get("App.Text104");
                Logs.Error(title, e);
                WindowManager.ShowError(title, e);
            }

            return new StringRes() { Data = title };
        }
    }

    /// <summary>
    /// 当账户编辑后
    /// </summary>
    public static void OnUserEdit()
    {
        WindowManager.UserWindow?.LoadUsers();
    }

    /// <summary>
    /// 获取选中的账户
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<GameLaunchUserRes> GetLaunchUser(WindowModel model)
    {
        var login = GetLastUser();
        if (login == null)
        {
            return new GameLaunchUserRes { Message = LanguageUtils.Get("App.Text60") };
        }
        if (login.AuthType == AuthType.Offline)
        {
            var have = AuthDatabase.Auths.Keys.Any(a => a.Type == AuthType.OAuth);
            if (!have)
            {
                WebBinding.OpenWeb(WebType.Minecraft);
                return new GameLaunchUserRes { Message = LanguageUtils.Get("App.Text61") };
            }
        }

        if (UserManager.IsLock(login) && GuiConfigUtils.Config.LaunchCheck.CheckUser)
        {
            var res = await model.ShowChoice(LanguageUtils.Get("App.Text58"));
            if (!res)
            {
                return new GameLaunchUserRes { Message = LanguageUtils.Get("App.Text62") };
            }

            res = await model.ShowChoice(LanguageUtils.Get("App.Text52"));
            if (res)
            {
                GuiConfigUtils.Config.LaunchCheck.CheckUser = false;
                GuiConfigUtils.Save();
            }
        }

        return new GameLaunchUserRes { User = login };
    }
}
