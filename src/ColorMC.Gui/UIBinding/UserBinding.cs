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
    /// <param name="gui">UI相关</param>
    /// <param name="select">UI相关</param>
    /// <param name="name">附加信息</param>
    /// <param name="password">附加信息</param>
    /// <param name="server">附加信息</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<StringRes> AddUserAsync(AuthType type, ILoginOAuthGui gui, ILoginGui select,
        CancellationToken token,
        string? name = null, string? password = null, string? server = null)
    {
        if (type == AuthType.Offline)
        {
            var user = new LoginObj()
            {
                UserName = name!,
                ClientToken = FunctionUtils.NewUUID(),
                UUID = HashHelper.GenMd5(Encoding.UTF8.GetBytes(name!.ToLower())),
                AuthType = AuthType.Offline
            };
            user.Save();
            UserManager.SetSelect(user);
            return new StringRes { State = true };
        }

        try
        {
            var res1 = type switch
            {
                AuthType.OAuth => await GameAuth.LoginOAuthAsync(gui, token),
                AuthType.Nide8 => await GameAuth.LoginNide8Async(server!, name!, password!, token),
                AuthType.AuthlibInjector => await GameAuth.LoginAuthlibInjectorAsync(server!, name!, password!, select, token),
                AuthType.LittleSkin => await GameAuth.LoginLittleSkinAsync(name!, password!, select, token),
                AuthType.SelfLittleSkin => await GameAuth.LoginLittleSkinAsync(name!, password!, select, token, server!),
                _ => throw new Exception(LangUtils.Get("App.Text78"))
            };
            if (token.IsCancellationRequested || res1 == null)
            {
                return new StringRes();
            }
            if (string.IsNullOrWhiteSpace(res1.UUID))
            {
                WebBinding.OpenWeb(WebType.Minecraft);
                return new StringRes { Data = LangUtils.Get("App.Text79") };
            }
            res1.LastLogin = DateTime.Now;
            res1.Save();
            UserManager.SetSelect(res1);
            return new StringRes { State = true };
        }
        catch (Exception e)
        {
            string title;
            if (e is LoginException e1)
            {
                title = e1.State.GetNameFail();
                string data = e1.Fail.GetName(type, name ?? server ?? "");
                string text = "";
                if (e1.Fail == LoginFailState.GetDataFail)
                {
                    text = title + Environment.NewLine + data;
                }
                else if (e1.Fail == LoginFailState.GetDataError)
                {
                    text = title + Environment.NewLine + string.Format(data, e1.ServerMessage);
                }
                if (e1.ServerMessage != null)
                {
                    text += Environment.NewLine + e1.ServerMessage;
                    title += " " + e1.ServerMessage;
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
                title = LangUtils.Get("App.Text104");
                Logs.Error(title, e);
                WindowManager.ShowError(title, e);
            }

            return new StringRes { Data = title };
        }
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
            return new StringRes() { Data = LangUtils.Get("App.Text105") };
        }

        try
        {
            var login = await obj.RefreshTokenAsync(token);
            login.LastLogin = DateTime.Now;
            login.Save();
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
                    text = title + Environment.NewLine + string.Format(data, e1.ServerMessage);
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
                title = LangUtils.Get("App.Text106");
                Logs.Error(title, e);
                WindowManager.ShowError(title, e);
            }

            return new StringRes() { Data = title };
        }

        return new StringRes() { State = true };
    }

    /// <summary>
    /// 加载账户皮肤
    /// </summary>
    /// <returns></returns>
    public static async Task LoadSkin()
    {
        var obj = UserManager.GetLastUser();

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
        var obj = UserManager.GetLastUser();

        if (obj == null)
        {
            return;
        }

        ImageManager.ReloadHead();
    }

    /// <summary>
    /// 编辑皮肤
    /// </summary>
    /// <param name="top">窗口</param>
    public static async void EditSkin(TopLevel top)
    {
        var obj = UserManager.GetLastUser();
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
            var login = await user.RefreshTokenAsync(token);
            login.LastLogin = DateTime.Now;
            login.Save();
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
                    text = title + Environment.NewLine + string.Format(data, e1.ServerMessage);
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
                title = LangUtils.Get("App.Text104");
                Logs.Error(title, e);
                WindowManager.ShowError(title, e);
            }

            return new StringRes() { Data = title };
        }
    }

    /// <summary>
    /// 获取选中的账户
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<GameLaunchUserRes> GetLaunchUser(WindowModel model)
    {
        var login = UserManager.GetLastUser();
        if (login == null)
        {
            return new GameLaunchUserRes { Message = LangUtils.Get("App.Text60") };
        }
        if (login.AuthType == AuthType.Offline)
        {
            var have = AuthDatabase.Auths.Keys.Any(a => a.Type == AuthType.OAuth);
            if (!have)
            {
                WebBinding.OpenWeb(WebType.Minecraft);
                return new GameLaunchUserRes { Message = LangUtils.Get("App.Text61") };
            }
        }

        if (UserManager.IsLock(login) && GuiConfigUtils.Config.LaunchCheck.CheckUser)
        {
            var res = await model.ShowChoice(LangUtils.Get("App.Text58"));
            if (!res)
            {
                return new GameLaunchUserRes { Message = LangUtils.Get("App.Text62") };
            }

            res = await model.ShowChoice(LangUtils.Get("App.Text52"));
            if (res)
            {
                GuiConfigUtils.Config.LaunchCheck.CheckUser = false;
                GuiConfigUtils.Save();
            }
        }

        return new GameLaunchUserRes { User = login };
    }
}
