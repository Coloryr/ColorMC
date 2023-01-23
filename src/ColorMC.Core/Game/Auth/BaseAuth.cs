using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Login;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game.Auth;

public enum AuthType
{
    /// <summary>
    /// 离线账户
    /// </summary>
    Offline, 
    /// <summary>
    /// 正版登录
    /// </summary>
    OAuth, 
    /// <summary>
    /// 统一通行证
    /// </summary>
    Nide8, 
    /// <summary>
    /// 外置登录
    /// </summary>
    AuthlibInjector, 
    /// <summary>
    /// 皮肤站
    /// </summary>
    LittleSkin,
    /// <summary>
    /// 自建皮肤站
    /// </summary>
    SelfLittleSkin
}

public enum AuthState
{
    OAuth, XBox, XSTS, Token, Profile
}

public enum LoginState
{
    Done, TimeOut, JsonError, Error, ErrorType, Crash
}

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
            CoreMain.AuthStateUpdate?.Invoke(AuthState.OAuth);
            var oauth = await OAuthAPI.GetCode();
            if (oauth.Done != LoginState.Done)
            {
                return (AuthState.OAuth, oauth.Done, null, oauth.Url!, null);
            }
            CoreMain.LoginOAuthCode?.Invoke(oauth.Url!, oauth.Code!);
            var oauth1 = await OAuthAPI.RunGetCode();
            if (oauth1.Done != LoginState.Done)
            {
                return (AuthState.OAuth, oauth1.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error1"), null);
            }
            now = AuthState.XBox;
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XBox);
            var xbox = await OAuthAPI.GetXBLAsync(oauth1.Obj!.access_token);
            if (xbox.Done != LoginState.Done)
            {
                return (AuthState.XBox, xbox.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error2"), null);
            }
            now = AuthState.XSTS;
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XSTS);
            var xsts = await OAuthAPI.GetXSTSAsync(xbox.XNLToken!);
            if (xsts.Done != LoginState.Done)
            {
                return (AuthState.XSTS, xsts.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error3"), null);
            }
            now = AuthState.Token;
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var auth = await OAuthAPI.GetMinecraftAsync(xsts.XSTSUhs!, xsts.XSTSToken!);
            if (auth.Done != LoginState.Done)
            {
                return (AuthState.Token, auth.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error4"), null);
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(auth.AccessToken!);
            if (profile == null)
            {
                return (AuthState.Profile, LoginState.Error, null,
                    LanguageHelper.GetName("Core.Http.Login.Error5"), null);
            }

            return (AuthState.Profile, LoginState.Done, new()
            {
                Text1 = oauth1.Obj!.refresh_token,
                AuthType = AuthType.OAuth,
                AccessToken = auth.AccessToken!,
                UserName = profile.name,
                UUID = profile.id
            }, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error6");
            Logs.Error(text, e);
            return (now, LoginState.Crash, null, text, e);
        }
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
            if (obj.AuthType != AuthType.OAuth)
            {
                return (AuthState.OAuth, LoginState.ErrorType, null,
                    LanguageHelper.GetName("Core.Http.Login.Error7"), null);
            }

            CoreMain.AuthStateUpdate?.Invoke(AuthState.OAuth);
            var oauth = await OAuthAPI.RefreshTokenAsync(obj.Text1);
            if (oauth.Done != LoginState.Done)
            {
                return (AuthState.OAuth, oauth.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error1"), null);
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XBox);
            var xbox = await OAuthAPI.GetXBLAsync(oauth.Auth!.access_token);
            if (xbox.Done != LoginState.Done)
            {
                return (AuthState.XBox, xbox.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error2"), null);
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XSTS);
            var xsts = await OAuthAPI.GetXSTSAsync(xbox.XNLToken!);
            if (xsts.Done != LoginState.Done)
            {
                return (AuthState.XSTS, xsts.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error3"), null);
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var auth = await OAuthAPI.GetMinecraftAsync(xsts.XSTSUhs!, xsts.XSTSToken!);
            if (auth.Done != LoginState.Done)
            {
                return (AuthState.Token, auth.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error4"), null);
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(auth.AccessToken!);
            if (auth.Done != LoginState.Done)
            {
                return (AuthState.Token, LoginState.Error, null,
                    LanguageHelper.GetName("Core.Http.Login.Error5"), null);
            }

            obj.UserName = profile.name;
            obj.UUID = profile.id;
            obj.Text1 = oauth.Auth!.refresh_token;
            obj.AccessToken = auth.AccessToken!;

            return (AuthState.Profile, LoginState.Done, obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error8");
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
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var (State, Obj, Msg) = await Nide8.Authenticate(server, Funtcions.NewUUID(), user, pass);
            if (State != LoginState.Done)
            {
                return (AuthState.Token, State, null,
                    string.Format(LanguageHelper.GetName("Core.Http.Login.Error9"), Msg), null);
            }

            Obj!.Text2 = user;
            return (AuthState.Profile, LoginState.Done, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error10");
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
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            if (obj.AuthType != AuthType.Nide8)
            {
                return (AuthState.Token, LoginState.ErrorType, null,
                    LanguageHelper.GetName("Core.Http.Login.Error7"), null);
            }

            var (State, Obj) = await Nide8.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null,
                    LanguageHelper.GetName("Core.Http.Login.Error11"), null);

            return (AuthState.Profile, LoginState.Done, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error12");
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
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var (State, Obj, Msg) = await AuthlibInjector.Authenticate(server, Funtcions.NewUUID(), user, pass);
            if (State != LoginState.Done)
            {
                return (AuthState.Token, State, null,
                    string.Format(LanguageHelper.GetName("Core.Http.Login.Error13"), Msg), null);
            }

            Obj!.Text2 = user;
            return (AuthState.Profile, LoginState.Done, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error14");
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
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            if (obj.AuthType != AuthType.AuthlibInjector)
            {
                return (AuthState.Token, LoginState.ErrorType, null,
                    LanguageHelper.GetName("Core.Http.Login.Error7"), null);
            }

            var (State, Obj) = await AuthlibInjector.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null,
                    LanguageHelper.GetName("Core.Http.Login.Error15"), null);

            return (AuthState.Token, State, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error16");
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
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var (State, Obj, Msg) = await LittleSkin.Authenticate(Funtcions.NewUUID(), user, pass, server);
            if (State != LoginState.Done)
            {
                return (AuthState.Token, State, null,
                    string.Format(LanguageHelper.GetName("Core.Http.Login.Error17"), Msg), null);
            }

            Obj!.Text2 = user;
            return (AuthState.Profile, LoginState.Done, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error18");
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
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            if (obj.AuthType != AuthType.LittleSkin
                || obj.AuthType != AuthType.SelfLittleSkin)
            {
                return (AuthState.Token, LoginState.ErrorType, null,
                    LanguageHelper.GetName("Core.Http.Login.Error7"), null);
            }

            var (State, Obj) = await LittleSkin.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null,
                    LanguageHelper.GetName("Core.Http.Login.Error19"), null);

            return (AuthState.Token, State, Obj, null, null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error20");
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
        switch (obj.AuthType)
        {
            case AuthType.OAuth:
                return await RefreshWithOAuth(obj);
            case AuthType.Nide8:
                return await RefreshWithNide8(obj);
            case AuthType.AuthlibInjector:
                return await RefreshWithAuthlibInjector(obj);
            case AuthType.LittleSkin:
            case AuthType.SelfLittleSkin:
                return await RefreshWithLittleSkin(obj);
            default:
                return (AuthState.Token, LoginState.Done, obj, null, null);
        }
    }
}
