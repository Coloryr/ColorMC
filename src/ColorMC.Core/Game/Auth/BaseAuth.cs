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
    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string Message, Exception? e)> LoginWithOAuth()
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
                RefreshToken = oauth1.Obj!.refresh_token,
                AuthType = AuthType.OAuth,
                AccessToken = auth.AccessToken!,
                UserName = profile.name,
                UUID = profile.id
            }, LanguageHelper.GetName("Core.Http.Login.Done"), null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error6");
            Logs.Error(text, e);
            return (now, LoginState.Crash, null, text, e);
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string Message, Exception? Ex)> RefreshWithOAuth(LoginObj obj)
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
            var oauth = await OAuthAPI.RefreshTokenAsync(obj.RefreshToken);
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
            obj.RefreshToken = oauth.Auth!.refresh_token;
            obj.AccessToken = auth.AccessToken!;

            return (AuthState.Profile, LoginState.Done, obj,
                LanguageHelper.GetName("Core.Http.Login.Refresh"), null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error8");
            Logs.Error(text, e);
            return (now, LoginState.Crash, null, text, e);
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string Message, Exception? Ex)> LoginWithNide8(string server, string user, string pass)
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
            return (AuthState.Profile, LoginState.Done, Obj,
                LanguageHelper.GetName("Core.Http.Login.Done1"), null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error10");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string Message, Exception? Ex)> RefreshWithNide8(LoginObj obj)
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

            return (AuthState.Profile, LoginState.Done, Obj,
                LanguageHelper.GetName("Core.Http.Login.Refresh1"), null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error12");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string Message, Exception? Ex)> LoginWithAuthlibInjector(string server, string user, string pass)
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
            return (AuthState.Profile, LoginState.Done, Obj,
                LanguageHelper.GetName("Core.Http.Login.Done2"), null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error14");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string Message, Exception? Ex)> RefreshWithAuthlibInjector(LoginObj obj)
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

            return (AuthState.Token, State, Obj,
                LanguageHelper.GetName("Core.Http.Login.Refresh2"), null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error16");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string Message, Exception? Ex)> LoginWithLittleSkin(string user, string pass, string? server = null)
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
            return (AuthState.Profile, LoginState.Done, Obj,
                LanguageHelper.GetName("Core.Http.Login.Done3"), null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error18");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }

    public static async Task<(AuthState State, LoginState State1, LoginObj? Obj,
        string Message, Exception? Ex)> RefreshWithLittleSkin(LoginObj obj)
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

            return (AuthState.Token, State, Obj,
                LanguageHelper.GetName("Core.Http.Login.Refresh3"), null);
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Http.Login.Error20");
            Logs.Error(text, e);
            return (AuthState.Profile, LoginState.Crash, null, text, e);
        }
    }
}
