using ColorMC.Core.Http.Login;
using ColorMC.Core.Objs.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Login;

public enum AuthType
{
    Offline, OAuth
}

public enum AuthState
{
    OAuth, XBox, XSTS, Profile,
    Login, Token
}

public enum LoginState
{ 
    Done, TimeOut, JsonError, Error
}

public static class Auth
{
    public static async Task<LoginObj?> LoginWithOAuth()
    {
        try
        {
            CoreMain.AuthState?.Invoke(AuthState.OAuth);
            var oauth = await OAuth.AddAuth();
            if (oauth.Done != LoginState.Done)
                return null;
            CoreMain.AuthState?.Invoke(AuthState.XBox);
            var (Done, XNLToken, XBLUhs) = await OAuth.GetXBLAsync(oauth.Auth!.access_token);
            if (Done != LoginState.Done)
                return null;
            CoreMain.AuthState?.Invoke(AuthState.XSTS);
            var XSTS = await OAuth.GetXSTSAsync(XNLToken!);
            if (XSTS.Done != LoginState.Done)
                return null;
            CoreMain.AuthState?.Invoke(AuthState.Profile);
            var Auth = await OAuth.GetMinecraftAsync(XSTS.XSTSUhs!, XSTS.XSTSToken!);
            if (Auth.Done != LoginState.Done)
                return null;

            return new()
            {
                RefreshToken = oauth.Auth!.refresh_token,
                AuthType = AuthType.OAuth, 
                Token = Auth.AccessToken!
            };
        }
        catch (Exception e)
        {
            CoreMain.OnError("登录错误", e, false);
            return null;
        }
    }

    public static async Task<LoginObj?> LoginWithOAuth(LoginObj obj)
    {
        try
        {
            CoreMain.AuthState?.Invoke(AuthState.OAuth);
            var oauth = await OAuth.RefreshTokenAsync(obj.RefreshToken);
            if (oauth.Done != LoginState.Done)
                return null;
            CoreMain.AuthState?.Invoke(AuthState.XBox);
            var (Done, XNLToken, XBLUhs) = await OAuth.GetXBLAsync(oauth.Auth!.access_token);
            if (Done != LoginState.Done)
                return null;
            CoreMain.AuthState?.Invoke(AuthState.XSTS);
            var XSTS = await OAuth.GetXSTSAsync(XNLToken!);
            if (XSTS.Done != LoginState.Done)
                return null;
            CoreMain.AuthState?.Invoke(AuthState.Profile);
            var Auth = await OAuth.GetMinecraftAsync(XSTS.XSTSUhs!, XSTS.XSTSToken!);
            if (Auth.Done != LoginState.Done)
                return null;

            obj.RefreshToken = oauth.Auth!.refresh_token;
            obj.Token = Auth.AccessToken!;

            return obj;
        }
        catch (Exception e)
        {
            CoreMain.OnError("刷新登录错误", e, false);
            return null;
        }
    }
}
