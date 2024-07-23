using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Login;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 账户登录类
/// </summary>
public static class GameAuth
{
    /// <summary>
    /// 从OAuth登录
    /// </summary>
    /// <returns>AuthState验证过程
    /// LoginState登录状态
    /// Obj账户
    /// Message错误信息
    /// Ex异常</returns>
    public static async Task<LoginRes> LoginOAuthAsync(ColorMCCore.LoginOAuthCode loginOAuth)
    {
        var now = AuthState.OAuth;
        try
        {
            var res1 = await OAuthApi.GetCodeAsync();
            if (res1.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.OAuth,
                    LoginState = res1.State,
                    Message = res1.Message
                };
            }
            loginOAuth(res1.Message!, res1.Code!);
            var res2 = await OAuthApi.RunGetCodeAsync();
            if (res2.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.OAuth,
                    LoginState = res2.State,
                    Message = LanguageHelper.Get("Core.Login.Error1")
                };
            }
            now = AuthState.XBox;
            var res3 = await OAuthApi.GetXBLAsync(res2.Obj!.access_token);
            if (res3.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.XBox,
                    LoginState = res3.State,
                    Message = LanguageHelper.Get("Core.Login.Error2")
                };
            }
            now = AuthState.XSTS;
            var res4 = await OAuthApi.GetXSTSAsync(res3.XBLToken!);
            if (res4.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.XSTS,
                    LoginState = res4.State,
                    Message = LanguageHelper.Get("Core.Login.Error3")
                };
            }
            now = AuthState.Token;
            var res5 = await MinecraftAPI.GetMinecraftAsync(res4.XSTSUhs!, res4.XSTSToken!);
            if (res5.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.Token,
                    LoginState = res5.State,
                    Message = LanguageHelper.Get("Core.Login.Error4")
                };
            }

            var profile = await MinecraftAPI.GetMinecraftProfileAsync(res5.AccessToken!);
            if (profile == null || string.IsNullOrWhiteSpace(profile.id))
            {
                return new LoginRes
                {
                    AuthState = AuthState.Profile,
                    LoginState = LoginState.DataError,
                    Message = LanguageHelper.Get("Core.Login.Error5")
                };
            }

            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Done,
                Auth = new()
                {
                    Text1 = res2.Obj!.refresh_token,
                    AuthType = AuthType.OAuth,
                    AccessToken = res5.AccessToken!,
                    UserName = profile.name,
                    UUID = profile.id
                }
            };
        }
        catch (Exception e)
        {
            var text = LanguageHelper.Get("Core.Login.Error6");
            Logs.Error(text, e);
            return new LoginRes
            {
                AuthState = now,
                LoginState = LoginState.Crash,
                Message = text,
                Ex = e
            };
        }
    }

    /// <summary>
    /// 取消OAuth登录
    /// </summary>
    public static void CancelWithOAuth()
    {
        OAuthApi.Cancel();
    }

    /// <summary>
    /// 从OAuth刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns></returns>
    public static async Task<LoginRes> RefreshOAuthAsync(LoginObj obj)
    {
        AuthState now = AuthState.OAuth;
        try
        {
            var profile = await MinecraftAPI.GetMinecraftProfileAsync(obj.AccessToken);
            if (profile != null && !string.IsNullOrWhiteSpace(profile.id))
            {
                return new LoginRes
                {
                    AuthState = AuthState.Profile,
                    LoginState = LoginState.Done,
                    Auth = obj
                };
            }
            var res1 = await OAuthApi.RefreshTokenAsync(obj.Text1);
            if (res1.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.OAuth,
                    LoginState = res1.State,
                    Message = LanguageHelper.Get("Core.Login.Error1")
                };
            }
            var res2 = await OAuthApi.GetXBLAsync(res1.Obj!.access_token);
            if (res2.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.XBox,
                    LoginState = res1.State,
                    Message = LanguageHelper.Get("Core.Login.Error2")
                };
            }
            var res3 = await OAuthApi.GetXSTSAsync(res2.XBLToken!);
            if (res3.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.XSTS,
                    LoginState = res3.State,
                    Message = LanguageHelper.Get("Core.Login.Error3")
                };
            }
            var res4 = await MinecraftAPI.GetMinecraftAsync(res3.XSTSUhs!, res3.XSTSToken!);
            if (res4.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.Token,
                    LoginState = res4.State,
                    Message = LanguageHelper.Get("Core.Login.Error4")
                };
            }

            profile = await MinecraftAPI.GetMinecraftProfileAsync(res4.AccessToken!);
            if (profile == null || string.IsNullOrWhiteSpace(profile.id))
            {
                return new LoginRes
                {
                    AuthState = AuthState.Profile,
                    LoginState = LoginState.Error,
                    Message = LanguageHelper.Get("Core.Login.Error5")
                };
            }

            obj.UserName = profile!.name;
            obj.UUID = profile.id;
            obj.Text1 = res1.Obj!.refresh_token;
            obj.AccessToken = res4.AccessToken!;

            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Done,
                Auth = obj
            };
        }
        catch (Exception e)
        {
            var text = LanguageHelper.Get("Core.Login.Error8");
            Logs.Error(text, e);
            return new LoginRes
            {
                AuthState = now,
                LoginState = LoginState.Crash,
                Message = text,
                Ex = e
            };
        }
    }

    /// <summary>
    /// 从统一通行证登录
    /// </summary>
    /// <param name="server">服务器UUID</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <returns></returns>
    public static async Task<LoginRes> LoginNide8Async(string server, string user, string pass)
    {
        try
        {
            var res1 = await Nide8.AuthenticateAsync(server, FuntionUtils.NewUUID(), user, pass);
            if (res1.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.Token,
                    LoginState = res1.State,
                    Message = string.Format(LanguageHelper.Get("Core.Login.Error9"), res1.Message)
                };
            }

            res1.Auth!.Text2 = user;
            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Done,
                Auth = res1.Auth
            };
        }
        catch (Exception e)
        {
            var text = LanguageHelper.Get("Core.Login.Error10");
            Logs.Error(text, e);
            return new LoginRes
            {
                AuthState = AuthState.Token,
                LoginState = LoginState.Crash,
                Message = text,
                Ex = e
            };
        }
    }

    /// <summary>
    /// 从统一通行证刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns></returns>
    public static async Task<LoginRes> RefreshNide8Async(LoginObj obj)
    {
        try
        {
            var res1 = await Nide8.RefreshAsync(obj);
            if (res1.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.Profile,
                    LoginState = res1.State,
                    Message = string.Format(LanguageHelper.Get("Core.Login.Error11"), res1.Message)
                };
            }
            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Done,
                Auth = res1.Auth
            };
        }
        catch (Exception e)
        {
            var text = LanguageHelper.Get("Core.Login.Error12");
            Logs.Error(text, e);
            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Crash,
                Message = text,
                Ex = e
            };
        }
    }

    /// <summary>
    /// 从外置登录登录
    /// </summary>
    /// <param name="server">服务器地址</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <returns></returns>
    public static async Task<LoginRes> LoginAuthlibInjectorAsync(string server, string user, string pass)
    {
        try
        {
            var res1 = await AuthlibInjector.AuthenticateAsync(FuntionUtils.NewUUID(), user, pass, server);
            if (res1.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.Token,
                    LoginState = res1.State,
                    Message = string.Format(LanguageHelper.Get("Core.Login.Error13"), res1.Message)
                };
            }

            res1.Auth!.Text2 = user;
            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Done,
                Auth = res1.Auth
            };
        }
        catch (Exception e)
        {
            var text = LanguageHelper.Get("Core.Login.Error14");
            Logs.Error(text, e);
            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Crash,
                Message = text,
                Ex = e
            };
        }
    }

    /// <summary>
    /// 从外置登录刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns></returns>
    public static async Task<LoginRes> RefreshAuthlibInjectorAsync(LoginObj obj)
    {
        try
        {
            var res1 = await AuthlibInjector.RefreshAsync(obj);
            if (res1.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.Token,
                    LoginState = res1.State,
                    Message = string.Format(LanguageHelper.Get("Core.Login.Error15"), res1.Message)
                };
            }

            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Done,
                Auth = res1.Auth
            };
        }
        catch (Exception e)
        {
            var text = LanguageHelper.Get("Core.Login.Error16");
            Logs.Error(text, e);
            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Crash,
                Message = text,
                Ex = e
            };
        }
    }

    /// <summary>
    /// 从皮肤站登录
    /// </summary>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <param name="server">自定义皮肤站地址</param>
    /// <returns></returns>
    public static async Task<LoginRes> LoginLittleSkinAsync(string user, string pass, string? server = null)
    {
        try
        {
            var res1 = await LittleSkin.AuthenticateAsync(FuntionUtils.NewUUID(), user, pass, server);
            if (res1.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.Token,
                    LoginState = res1.State,
                    Message = string.Format(LanguageHelper.Get("Core.Login.Error17"), res1.Message)
                };
            }

            res1.Auth!.Text2 = user;
            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Done,
                Auth = res1.Auth
            };
        }
        catch (Exception e)
        {
            var text = LanguageHelper.Get("Core.Login.Error18");
            Logs.Error(text, e);
            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Crash,
                Message = text,
                Ex = e
            };
        }
    }

    /// <summary>
    /// 从皮肤站刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns></returns>
    public static async Task<LoginRes> RefreshLittleSkinAsync(LoginObj obj)
    {
        try
        {
            var res1 = await LittleSkin.RefreshAsync(obj);
            if (res1.State != LoginState.Done)
            {
                return new LoginRes
                {
                    AuthState = AuthState.Token,
                    LoginState = res1.State,
                    Message = string.Format(LanguageHelper.Get("Core.Login.Error19"), res1.Message)
                };
            }

            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Done,
                Auth = res1.Auth
            };
        }
        catch (Exception e)
        {
            var text = LanguageHelper.Get("Core.Login.Error20");
            Logs.Error(text, e);
            return new LoginRes
            {
                AuthState = AuthState.Profile,
                LoginState = LoginState.Crash,
                Message = text,
                Ex = e
            };
        }
    }

    /// <summary>
    /// 刷新登录登录
    /// </summary>
    /// <param name="obj">登录信息</param>
    /// <returns></returns>
    public static async Task<LoginRes> RefreshTokenAsync(this LoginObj obj)
    {
        return obj.AuthType switch
        {
            AuthType.OAuth => await RefreshOAuthAsync(obj),
            AuthType.Nide8 => await RefreshNide8Async(obj),
            AuthType.AuthlibInjector => await RefreshAuthlibInjectorAsync(obj),
            AuthType.LittleSkin or AuthType.SelfLittleSkin => await RefreshLittleSkinAsync(obj),
            _ => new LoginRes { AuthState = AuthState.Profile, LoginState = LoginState.Done, Auth = obj },
        };
    }
}
