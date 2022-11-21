using ColorMC.Core.Http.Apis;
using ColorMC.Core.Http.Login;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game.Auth;

public enum AuthType
{
    Offline, OAuth, Nide8
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
    public static async Task<LoginObj?> LoginWithOAuth()
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.OAuth);
            var oauth = await OAuthAPI.GetCode();
            if (oauth.Done != LoginState.Done)
            {
                CoreMain.LoginFail?.Invoke(oauth.Done, oauth.Url!);
                return null;
            }
            CoreMain.LoginOAuthCode?.Invoke(oauth.Url!, oauth.Code!);
            var oauth1 = await OAuthAPI.RunGetCode();
            if (oauth1.Done != LoginState.Done)
            {
                CoreMain.LoginFail?.Invoke(oauth.Done, "OAuth验证错误");
                return null;
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XBox);
            var xbox = await OAuthAPI.GetXBLAsync(oauth1.Obj!.access_token);
            if (xbox.Done != LoginState.Done)
            {
                CoreMain.LoginFail?.Invoke(oauth.Done, "XboxLive验证错误");
                return null;
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XSTS);
            var xsts = await OAuthAPI.GetXSTSAsync(xbox.XNLToken!);
            if (xsts.Done != LoginState.Done)
            {
                CoreMain.LoginFail?.Invoke(oauth.Done, "XSTS验证错误");
                return null;
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var auth = await OAuthAPI.GetMinecraftAsync(xsts.XSTSUhs!, xsts.XSTSToken!);
            if (auth.Done != LoginState.Done)
            {
                CoreMain.LoginFail?.Invoke(oauth.Done, "Minecraft验证错误");
                return null;
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(auth.AccessToken!);

            return new()
            {
                RefreshToken = oauth1.Obj!.refresh_token,
                AuthType = AuthType.OAuth,
                AccessToken = auth.AccessToken!,
                UserName = profile.name,
                UUID = profile.id
            };
        }
        catch (Exception e)
        {
            Logs.Error("登录发生错误", e);
            CoreMain.LoginFail?.Invoke(LoginState.Crash, "登录发生错误" + Environment.NewLine + e.ToString());
            return null;
        }
    }

    public static async Task<LoginObj?> RefreshWithOAuth(LoginObj obj)
    {
        try
        {
            if (obj.AuthType != AuthType.OAuth)
            {
                CoreMain.LoginFail?.Invoke(LoginState.Error, "错误的用户类型");
                return null;
            }

            CoreMain.AuthStateUpdate?.Invoke(AuthState.OAuth);
            var oauth = await OAuthAPI.RefreshTokenAsync(obj.RefreshToken);
            if (oauth.Done != LoginState.Done)
            {
                CoreMain.LoginFail?.Invoke(oauth.Done, "OAuth验证错误");
                return null;
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XBox);
            var xbox = await OAuthAPI.GetXBLAsync(oauth.Auth!.access_token);
            if (xbox.Done != LoginState.Done)
            {
                CoreMain.LoginFail?.Invoke(oauth.Done, "XboxLive验证错误");
                return null;
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XSTS);
            var xsts = await OAuthAPI.GetXSTSAsync(xbox.XNLToken!);
            if (xsts.Done != LoginState.Done)
            {
                CoreMain.LoginFail?.Invoke(oauth.Done, "XSTS验证错误");
                return null;
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var auth = await OAuthAPI.GetMinecraftAsync(xsts.XSTSUhs!, xsts.XSTSToken!);
            if (auth.Done != LoginState.Done)
            {
                CoreMain.LoginFail?.Invoke(oauth.Done, "Minecraft验证错误");
                return null;
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(auth.AccessToken!);
            obj.UserName = profile.name;
            obj.UUID = profile.id;
            obj.RefreshToken = oauth.Auth!.refresh_token;
            obj.AccessToken = auth.AccessToken!;

            return obj;
        }
        catch (Exception e)
        {
            Logs.Error("刷新登录发生错误", e);
            CoreMain.LoginFail?.Invoke(LoginState.Crash, "刷新登录发生错误" + Environment.NewLine + e.ToString());
            return null;
        }
    }

    public static async Task<LoginObj?> LoginWithNide8(string server, string user, string pass)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var data = await Nide8.Authenticate(server, Funtcions.NewUUID(), user, pass);
            if (data.State != LoginState.Done)
            {
                CoreMain.LoginFail?.Invoke(data.State, "Nide8验证错误:" + data.Msg);
                return null;
            }

            return data.Obj;
        }
        catch (Exception e)
        {
            Logs.Error("登录发生错误", e);
            CoreMain.LoginFail?.Invoke(LoginState.Crash, "登录发生错误" + Environment.NewLine + e.ToString());
            return null;
        }
    }

    public static async Task<LoginObj?> RefreshWithNide8(LoginObj obj)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            if (obj.AuthType != AuthType.Nide8)
            {
                CoreMain.LoginFail?.Invoke(LoginState.Error, "错误的用户类型");
                return null;
            }

            var data = await Nide8.Refresh(obj);
            if (data.State != LoginState.Done)
                return null;

            return data.Obj;
        }
        catch (Exception e)
        {
            Logs.Error("刷新登录发生错误", e);
            CoreMain.LoginFail?.Invoke(LoginState.Crash, "刷新登录发生错误" + Environment.NewLine + e.ToString());
            return null;
        }
    }
}
