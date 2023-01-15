using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Login;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

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
                return (AuthState.OAuth, oauth1.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error1"));
            }
            now = AuthState.XBox;
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XBox);
            var xbox = await OAuthAPI.GetXBLAsync(oauth1.Obj!.access_token);
            if (xbox.Done != LoginState.Done)
            {
                return (AuthState.XBox, xbox.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error2"));
            }
            now = AuthState.XSTS;
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XSTS);
            var xsts = await OAuthAPI.GetXSTSAsync(xbox.XNLToken!);
            if (xsts.Done != LoginState.Done)
            {
                return (AuthState.XSTS, xsts.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error3"));
            }
            now = AuthState.Token;
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var auth = await OAuthAPI.GetMinecraftAsync(xsts.XSTSUhs!, xsts.XSTSToken!);
            if (auth.Done != LoginState.Done)
            {
                return (AuthState.Token, auth.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error4"));
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(auth.AccessToken!);
            if (profile == null)
            {
                return (AuthState.Profile, LoginState.Error, null,
                    LanguageHelper.GetName("Core.Http.Login.Error5"));
            }

            return (AuthState.Profile, LoginState.Done, new()
            {
                RefreshToken = oauth1.Obj!.refresh_token,
                AuthType = AuthType.OAuth,
                AccessToken = auth.AccessToken!,
                UserName = profile.name,
                UUID = profile.id
            }, LanguageHelper.GetName("Core.Http.Login.Done"));
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error6");
            Logs.Error(text, e);
            return (now, LoginState.Crash, null, text + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> RefreshWithOAuth(LoginObj obj)
    {
        AuthState now = AuthState.OAuth;
        try
        {
            if (obj.AuthType != AuthType.OAuth)
            {
                return (AuthState.OAuth, LoginState.ErrorType, null,
                    LanguageHelper.GetName("Core.Http.Login.Error7"));
            }

            CoreMain.AuthStateUpdate?.Invoke(AuthState.OAuth);
            var oauth = await OAuthAPI.RefreshTokenAsync(obj.RefreshToken);
            if (oauth.Done != LoginState.Done)
            {
                return (AuthState.OAuth, oauth.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error1"));
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XBox);
            var xbox = await OAuthAPI.GetXBLAsync(oauth.Auth!.access_token);
            if (xbox.Done != LoginState.Done)
            {
                return (AuthState.XBox, xbox.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error2"));
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.XSTS);
            var xsts = await OAuthAPI.GetXSTSAsync(xbox.XNLToken!);
            if (xsts.Done != LoginState.Done)
            {
                return (AuthState.XSTS, xsts.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error3"));
            }
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var auth = await OAuthAPI.GetMinecraftAsync(xsts.XSTSUhs!, xsts.XSTSToken!);
            if (auth.Done != LoginState.Done)
            {
                return (AuthState.Token, auth.Done, null,
                    LanguageHelper.GetName("Core.Http.Login.Error4"));
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(auth.AccessToken!);
            if (auth.Done != LoginState.Done)
            {
                return (AuthState.Token, LoginState.Error, null,
                    LanguageHelper.GetName("Core.Http.Login.Error5"));
            }

            obj.UserName = profile.name;
            obj.UUID = profile.id;
            obj.RefreshToken = oauth.Auth!.refresh_token;
            obj.AccessToken = auth.AccessToken!;

            return (AuthState.Profile, LoginState.Done, obj,
                LanguageHelper.GetName("Core.Http.Login.Refresh"));
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error8");
            Logs.Error(text, e);
            return (now, LoginState.Crash, null, text + Environment.NewLine + e.ToString());
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
                return (AuthState.Token, State, null,
                    string.Format(LanguageHelper.GetName("Core.Http.Login.Error9"), Msg));
            }

            Obj!.Text2 = user;
            return (AuthState.Profile, LoginState.Done, Obj,
                LanguageHelper.GetName("Core.Http.Login.Done1"));
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error10");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> RefreshWithNide8(LoginObj obj)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            if (obj.AuthType != AuthType.Nide8)
            {
                return (AuthState.Token, LoginState.ErrorType, null,
                    LanguageHelper.GetName("Core.Http.Login.Error7"));
            }

            var (State, Obj) = await Nide8.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null,
                    LanguageHelper.GetName("Core.Http.Login.Error11"));

            return (AuthState.Profile, LoginState.Done, Obj,
                LanguageHelper.GetName("Core.Http.Login.Refresh1"));
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error12");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text + Environment.NewLine + e.ToString());
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
                return (AuthState.Token, State, null,
                    string.Format(LanguageHelper.GetName("Core.Http.Login.Error13"), Msg));
            }

            Obj!.Text2 = user;
            return (AuthState.Profile, LoginState.Done, Obj,
                LanguageHelper.GetName("Core.Http.Login.Done2"));
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error14");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> RefreshWithAuthlibInjector(LoginObj obj)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            if (obj.AuthType != AuthType.AuthlibInjector)
            {
                return (AuthState.Token, LoginState.ErrorType, null,
                    LanguageHelper.GetName("Core.Http.Login.Error7"));
            }

            var (State, Obj) = await AuthlibInjector.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null,
                    LanguageHelper.GetName("Core.Http.Login.Error15"));

            return (AuthState.Token, State, Obj,
                LanguageHelper.GetName("Core.Http.Login.Refresh2"));
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error16");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> LoginWithLittleSkin(string user, string pass, string? server = null)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            var (State, Obj, Msg) = await LittleSkin.Authenticate(Funtcions.NewUUID(), user, pass, server);
            if (State != LoginState.Done)
            {
                return (AuthState.Token, State, null,
                    string.Format(LanguageHelper.GetName("Core.Http.Login.Error17"), Msg));
            }

            Obj!.Text2 = user;
            return (AuthState.Profile, LoginState.Done, Obj,
                LanguageHelper.GetName("Core.Http.Login.Done3"));
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error18");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text + Environment.NewLine + e.ToString());
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj, string Message)> RefreshWithLittleSkin(LoginObj obj)
    {
        try
        {
            CoreMain.AuthStateUpdate?.Invoke(AuthState.Token);
            if (obj.AuthType != AuthType.LittleSkin
                || obj.AuthType != AuthType.SelfLittleSkin)
            {
                return (AuthState.Token, LoginState.ErrorType, null,
                    LanguageHelper.GetName("Core.Http.Login.Error7"));
            }

            var (State, Obj) = await LittleSkin.Refresh(obj);
            if (State != LoginState.Done)
                return (AuthState.Token, State, null,
                    LanguageHelper.GetName("Core.Http.Login.Error19"));

            return (AuthState.Token, State, Obj,
                LanguageHelper.GetName("Core.Http.Login.Refresh3"));
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error20");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text + Environment.NewLine + e.ToString());
        }
    }
}
