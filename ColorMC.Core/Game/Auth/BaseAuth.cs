using ColorMC.Core.Http.Apis;
using ColorMC.Core.Http.Login;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Utils;
using System.Data.Common;

namespace ColorMC.Core.Game.Auth;

public enum AuthType
{
    Offline, OAuth, Nide8, AuthlibInjector, LittleSkin, SelfLittleSkin
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
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> LoginWithOAuth()
    {
        AuthState now = AuthState.OAuth;
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.OAuth);
            var oauth = await OAuthAPI.GetCode();
            if (oauth.Done != LoginState.Done)
            {
                return (AuthState.OAuth, oauth.Done, null, oauth.Url!);
            }
            CoreMain.LoginOAuthCode?.Invoke(oauth.Url!, oauth.Code!);
            var oauth1 = await OAuthAPI.RunGetCode();
            if (oauth1.Done != LoginState.Done)
            {
                return (AuthState.OAuth, oauth1.Done, null, "OAuth验证错误");
            }
            now = AuthState.XBox;
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XBox);
            var xbox = await OAuthAPI.GetXBLAsync(oauth1.Obj!.access_token);
            if (xbox.Done != LoginState.Done)
            {
                return (AuthState.XBox, xbox.Done, null, "XboxLive验证错误");
            }
            now = AuthState.XSTS;
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XSTS);
            var xsts = await OAuthAPI.GetXSTSAsync(xbox.XNLToken!);
            if (xsts.Done != LoginState.Done)
            {
                return (AuthState.XSTS, xsts.Done, null, "XSTS验证错误");
            }
            now = AuthState.Token;
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var auth = await OAuthAPI.GetMinecraftAsync(xsts.XSTSUhs!, xsts.XSTSToken!);
            if (auth.Done != LoginState.Done)
            {
                return (AuthState.Token, auth.Done, null, "Minecraft验证错误");
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(auth.AccessToken!);

            return (AuthState.Profile, LoginState.Done, new()
            {
                RefreshToken = oauth1.Obj!.refresh_token,
                AuthType = AuthType.OAuth,
                AccessToken = auth.AccessToken!,
                UserName = profile.name,
                UUID = profile.id
            }, "登录完成");
        }
        catch (Exception e)
        {
            Logs.Error("登录发生错误", e);
            return (now, LoginState.Crash, null, "登录发生错误" + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> RefreshWithOAuth(LoginObj obj)
    {
        AuthState now = AuthState.OAuth;
        try
        {
            if (obj.AuthType != AuthType.OAuth)
            {
                return (AuthState.OAuth, LoginState.ErrorType, null, "错误的用户类型");
            }

            CoreMain.AuthStateUpdate?.Invoke(AuthState.OAuth);
            var oauth = await OAuthAPI.RefreshTokenAsync(obj.RefreshToken);
            if (oauth.Done != LoginState.Done)
            {
                return (AuthState.OAuth, oauth.Done, null, "OAuth验证错误");
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XBox);
            var xbox = await OAuthAPI.GetXBLAsync(oauth.Auth!.access_token);
            if (xbox.Done != LoginState.Done)
            {
                return (AuthState.XBox, xbox.Done, null, "XboxLive验证错误");
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XSTS);
            var xsts = await OAuthAPI.GetXSTSAsync(xbox.XNLToken!);
            if (xsts.Done != LoginState.Done)
            {
                return (AuthState.XSTS, xsts.Done, null, "XSTS验证错误");
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var auth = await OAuthAPI.GetMinecraftAsync(xsts.XSTSUhs!, xsts.XSTSToken!);
            if (auth.Done != LoginState.Done)
            {
                return (AuthState.Token, auth.Done, null, "Minecraft验证错误");
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(auth.AccessToken!);
            obj.UserName = profile.name;
            obj.UUID = profile.id;
            obj.RefreshToken = oauth.Auth!.refresh_token;
            obj.AccessToken = auth.AccessToken!;

            return (AuthState.Profile, LoginState.Done, obj, "刷新登录完成");
        }
        catch (Exception e)
        {
            Logs.Error("刷新登录发生错误", e);
            return (now, LoginState.Crash, null, "刷新登录发生错误" + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> LoginWithNide8(string server, string user, string pass)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var (State, Obj, Msg) = await Nide8.Authenticate(server, Funtcions.NewUUID(), user, pass);
            if (State != LoginState.Done)
            {
                return (AuthState.Token, State, null, "Nide8验证错误:" + Msg);
            }

            return (AuthState.Profile, LoginState.Done, Obj, "Nide8登陆完成");
        }
        catch (Exception e)
        {
            Logs.Error("Nide8登录发生错误", e);
            return (AuthState.Profile, LoginState.Crash, null, "Nide8验证错误:" + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> RefreshWithNide8(LoginObj obj)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            if (obj.AuthType != AuthType.Nide8)
            {
                return (AuthState.Token, LoginState.ErrorType, null, "错误的用户类型");
            }

            var (State, Obj) = await Nide8.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null, "Nide8刷新登录失败");

            return (AuthState.Profile, LoginState.Done, Obj, "刷新登录完成");
        }
        catch (Exception e)
        {
            Logs.Error("Nide8刷新登录发生错误", e);
            return (AuthState.Profile, LoginState.Crash, null, "Nide8刷新登录发生错误" + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> LoginWithAuthlibInjector(string server, string user, string pass)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var (State, Obj, Msg) = await AuthlibInjector.Authenticate(server, Funtcions.NewUUID(), user, pass);
            if (State != LoginState.Done)
            {
                return (AuthState.Token, State, null, "AuthlibInjector验证错误:" + Msg);
            }

            return (AuthState.Profile, LoginState.Done, Obj, "AuthlibInjector登陆完成");
        }
        catch (Exception e)
        {
            Logs.Error("AuthlibInjector登录发生错误", e);
            return (AuthState.Profile, LoginState.Crash, null, "AuthlibInjector登录发生错误" + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> RefreshWithAuthlibInjector(LoginObj obj)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            if (obj.AuthType != AuthType.AuthlibInjector)
            {
                return (AuthState.Token, LoginState.ErrorType, null, "错误的用户类型");
            }

            var (State, Obj) = await AuthlibInjector.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null, "AuthlibInjector刷新登录失败");

            return (AuthState.Token, State, Obj, "AuthlibInjector刷新登录完成");
        }
        catch (Exception e)
        {
            Logs.Error("刷新登录发生错误", e);
            return (AuthState.Profile, LoginState.Crash, null, "AuthlibInjector刷新登录发生错误" + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> LoginWithLittleSkin(string user, string pass, string? server = null)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var (State, Obj, Msg) = await AuthlibInjector.Authenticate(Funtcions.NewUUID(), user, pass, server);
            if (State != LoginState.Done)
            {
                return (AuthState.Token, State, null, "LittleSkin验证错误:" + Msg);
            }

            return (AuthState.Profile, LoginState.Done, Obj, "LittleSkin登陆完成");
        }
        catch (Exception e)
        {
            Logs.Error("登录发生错误", e);
            return (AuthState.Profile, LoginState.Crash, null, "LittleSkin登录发生错误" + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> RefreshWithLittleSkin(LoginObj obj)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            if (obj.AuthType != AuthType.AuthlibInjector)
            {
                return (AuthState.Token, LoginState.ErrorType, null, "错误的用户类型");
            }

            var (State, Obj) = await AuthlibInjector.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null, "LittleSkin刷新登录失败");

            return (AuthState.Token, State, Obj, "LittleSkin刷新登录完成");
        }
        catch (Exception e)
        {
            Logs.Error("刷新登录发生错误", e);
            return (AuthState.Profile, LoginState.Crash, null, "LittleSkin刷新登录发生错误" + Environment.NewLine + e.ToString());
        }
    }
}
