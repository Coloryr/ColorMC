using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Login;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏登录类
/// </summary>
public static class BaseAuth
{
    /// <summary>
    /// 从OAuth登录
    /// </summary>
    /// <returns></returns>
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string? Message, Exception? e)> LoginWithOAuth()
    {
        AuthState now = AuthState.OAuth;
        try
        {
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.OAuth);
            var (done, code, url) = await OAuthAPI.GetCode();
            if (done != LoginState.Done)
            {
                return (AuthState.OAuth, done, null, url!, null);
            }
            ColorMCCore.LoginOAuthCode?.Invoke(url!, code!);
            (done, var obj) = await OAuthAPI.RunGetCode();
            if (done != LoginState.Done)
            {
                return (AuthState.OAuth, done, null,
                    LanguageHelper.Get("Core.Login.Error1"), null);
            }
            now = AuthState.XBox;
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.XBox);
            (done, var XNLToken, var XBLUhs) = await OAuthAPI.GetXBLAsync(obj!.access_token);
            if (done != LoginState.Done)
            {
                return (AuthState.XBox, done, null,
                    LanguageHelper.Get("Core.Login.Error2"), null);
            }
            now = AuthState.XSTS;
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.XSTS);
            (done, var xsts, var xsts1) = await OAuthAPI.GetXSTSAsync(XNLToken!);
            if (done != LoginState.Done)
            {
                return (AuthState.XSTS, done, null,
                    LanguageHelper.Get("Core.Login.Error3"), null);
            }
            now = AuthState.Token;
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.Token);
            (done, var token) = await OAuthAPI.GetMinecraftAsync(xsts1!, xsts!);
            if (done != LoginState.Done)
            {
                return (AuthState.Token, done, null,
                    LanguageHelper.Get("Core.Login.Error4"), null);
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(token!);
            if (profile == null)
            {
                return (AuthState.Profile, LoginState.Error, null,
                    LanguageHelper.Get("Core.Login.Error5"), null);
            }

            return (AuthState.Profile, LoginState.Done, new()
            {
                Text1 = obj!.refresh_token,
                AuthType = AuthType.OAuth,
                AccessToken = token!,
                UserName = profile.name,
                UUID = profile.id
            }, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.Get("Core.Login.Error6");
            Logs.Error(text, e);
            return (now, LoginState.Crash, null, text, e);
        }
    }

    /// <summary>
    /// 取消OAuth登录
    /// </summary>
    public static void CancelWithOAuth()
    {
        OAuthAPI.Cancel();
    }

    /// <summary>
    /// 从OAuth刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns></returns>
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string? Message, Exception? Ex)> RefreshWithOAuth(LoginObj obj)
    {
        AuthState now = AuthState.OAuth;
        try
        {
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.OAuth);
            var oauth = await OAuthAPI.RefreshTokenAsync(obj.Text1);
            if (oauth.Done != LoginState.Done)
            {
                return (AuthState.OAuth, oauth.Done, null,
                    LanguageHelper.Get("Core.Login.Error1"), null);
            }
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.XBox);
            var xbox = await OAuthAPI.GetXBLAsync(oauth.Auth!.access_token);
            if (xbox.Done != LoginState.Done)
            {
                return (AuthState.XBox, xbox.Done, null,
                    LanguageHelper.Get("Core.Login.Error2"), null);
            }
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.XSTS);
            var xsts = await OAuthAPI.GetXSTSAsync(xbox.XNLToken!);
            if (xsts.Done != LoginState.Done)
            {
                return (AuthState.XSTS, xsts.Done, null,
                    LanguageHelper.Get("Core.Login.Error3"), null);
            }
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.Token);
            var auth = await OAuthAPI.GetMinecraftAsync(xsts.XSTSUhs!, xsts.XSTSToken!);
            if (auth.Done != LoginState.Done)
            {
                return (AuthState.Token, auth.Done, null,
                    LanguageHelper.Get("Core.Login.Error4"), null);
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(auth.AccessToken!);
            if (auth.Done != LoginState.Done)
            {
                return (AuthState.Token, LoginState.Error, null,
                    LanguageHelper.Get("Core.Login.Error5"), null);
            }

            obj.UserName = profile!.name;
            obj.UUID = profile.id;
            obj.Text1 = oauth.Auth!.refresh_token;
            obj.AccessToken = auth.AccessToken!;

            return (AuthState.Profile, LoginState.Done, obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.Get("Core.Login.Error8");
            Logs.Error(text, e);
            return (now, LoginState.Crash, null, text, e);
        }
    }

    /// <summary>
    /// 从统一通行证登录
    /// </summary>
    /// <param name="server">服务器UUID</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <returns></returns>
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string? Message, Exception? Ex)> LoginWithNide8(string server, string user, string pass)
    {
        try
        {
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.Token);
            var (State, Obj, Msg) = await Nide8.Authenticate(server, Funtcions.NewUUID(), user, pass);
            if (State != LoginState.Done)
            {
                return (AuthState.Token, State, null,
                    string.Format(LanguageHelper.Get("Core.Login.Error9"), Msg), null);
            }

            Obj!.Text2 = user;
            return (AuthState.Profile, LoginState.Done, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.Get("Core.Login.Error10");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    /// <summary>
    /// 从统一通行证刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns></returns>
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string? Message, Exception? Ex)> RefreshWithNide8(LoginObj obj)
    {
        try
        {
            var (State, Obj, Msg) = await Nide8.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null,
                    LanguageHelper.Get("Core.Login.Error11") + " " + Msg, null);

            return (AuthState.Profile, LoginState.Done, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.Get("Core.Login.Error12");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    /// <summary>
    /// 从外置登录登录
    /// </summary>
    /// <param name="server">服务器地址</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <returns></returns>
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string? Message, Exception? Ex)> LoginWithAuthlibInjector(string server, string user, string pass)
    {
        try
        {
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.Token);
            var (State, Obj, Msg) = await AuthlibInjector.Authenticate(Funtcions.NewUUID(), user, pass, server);
            if (State != LoginState.Done)
            {
                return (AuthState.Token, State, null,
                    string.Format(LanguageHelper.Get("Core.Login.Error13"), Msg), null);
            }

            Obj!.Text2 = user;
            return (AuthState.Profile, LoginState.Done, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.Get("Core.Login.Error14");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    /// <summary>
    /// 从外置登录刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns></returns>
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string? Message, Exception? Ex)> RefreshWithAuthlibInjector(LoginObj obj)
    {
        try
        {
            var (State, Obj, Msg) = await AuthlibInjector.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null,
                    LanguageHelper.Get("Core.Login.Error15") + " " + Msg, null);

            return (AuthState.Token, State, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.Get("Core.Login.Error16");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    /// <summary>
    /// 从皮肤站登录
    /// </summary>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <param name="server">自定义皮肤站地址</param>
    /// <returns></returns>
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string? Message, Exception? Ex)> LoginWithLittleSkin(string user, string pass, string? server = null)
    {
        try
        {
            ColorMCCore.AuthStateUpdate?.Invoke(AuthState.Token);
            var (State, Obj, Msg) = await LittleSkin.Authenticate(Funtcions.NewUUID(), user, pass, server);
            if (State != LoginState.Done)
            {
                return (AuthState.Token, State, null,
                    string.Format(LanguageHelper.Get("Core.Login.Error17"), Msg), null);
            }

            Obj!.Text2 = user;
            return (AuthState.Profile, LoginState.Done, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.Get("Core.Login.Error18");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    /// <summary>
    /// 从皮肤站刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns></returns>
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string? Message, Exception? Ex)> RefreshWithLittleSkin(LoginObj obj)
    {
        try
        {
            var (State, Obj, Msg) = await LittleSkin.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null,
                    LanguageHelper.Get("Core.Login.Error19") + " " + Msg, null);

            return (AuthState.Token, State, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.Get("Core.Login.Error20");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    /// <summary>
    /// 刷新登录登录
    /// </summary>
    /// <param name="obj">登录信息</param>
    /// <returns>结果</returns>
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string? Message, Exception? Ex)> RefreshToken(this LoginObj obj)
    {
        return obj.AuthType switch
        {
            AuthType.OAuth => await RefreshWithOAuth(obj),
            AuthType.Nide8 => await RefreshWithNide8(obj),
            AuthType.AuthlibInjector => await RefreshWithAuthlibInjector(obj),
            AuthType.LittleSkin or AuthType.SelfLittleSkin => await RefreshWithLittleSkin(obj),
            _ => (AuthState.Token, LoginState.Done, obj, null, null),
        };
    }
}
