using ColorMC.Core.GuiHandel;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Login;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 账户登录
/// </summary>
public static class GameAuth
{
    /// <summary>
    /// 从OAuth登录
    /// </summary>
    /// <param name="loginOAuth">登录参数</param>
    /// <returns>登录状态</returns>
    public static async Task<LoginObj?> LoginOAuthAsync(ILoginOAuthGui loginOAuth)
    {
        //获取登录码
        var res1 = await OAuthApi.GetCodeAsync(loginOAuth.Token);
        loginOAuth.LoginOAuthCode(res1.Url, res1.Code);
        //获取用户登录
        var res2 = await OAuthApi.RunGetCodeAsync(res1, loginOAuth.Token);
        if (res2 == null)
        {
            return null;
        }
        //Xbox登录
        loginOAuth.LoginOAuthState(AuthState.XBox);
        var res3 = await OAuthApi.GetXBoxAsync(res2.AccessToken, loginOAuth.Token);
        if (res3 == null)
        {
            return null;
        }
        //XSTS登录
        loginOAuth.LoginOAuthState(AuthState.XSTS);
        var res4 = await OAuthApi.GetXSTSAsync(res3.XBLToken, loginOAuth.Token);
        if (res4 == null)
        {
            return null;
        }
        //获取mojang token
        loginOAuth.LoginOAuthState(AuthState.Token);
        var res5 = await MinecraftAPI.GetMinecraftAsync(res4.XSTSUhs, res4.XSTSToken, loginOAuth.Token);
        if (res5 == null)
        {
            return null;
        }
        //获取minecraft账户
        loginOAuth.LoginOAuthState(AuthState.Profile);
        var profile = await MinecraftAPI.GetMinecraftProfileAsync(res5.AccessToken, loginOAuth.Token);
        if (profile == null || string.IsNullOrWhiteSpace(profile.Id))
        {
            throw new LoginException(LoginFailState.GetOAuthCodeDataFail, AuthState.Profile);
        }

        return new LoginObj()
        {
            Text1 = res2.RefreshToken,
            AuthType = AuthType.OAuth,
            AccessToken = res5.AccessToken!,
            UserName = profile.Name,
            UUID = profile.Id
        };
    }

    /// <summary>
    /// 从OAuth刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns>登录结果</returns>
    private static async Task<LoginObj> RefreshOAuthAsync(LoginObj obj, CancellationToken token)
    {
        var profile = await MinecraftAPI.GetMinecraftProfileAsync(obj.AccessToken, token);
        if (profile != null && !string.IsNullOrWhiteSpace(profile.Id))
        {
            return obj;
        }
        var res1 = await OAuthApi.RefreshOAuthTokenAsync(obj.Text1, token);
        var res2 = await OAuthApi.GetXBoxAsync(res1.AccessToken, token);
        var res3 = await OAuthApi.GetXSTSAsync(res2.XBLToken, token);
        var res4 = await MinecraftAPI.GetMinecraftAsync(res3.XSTSUhs, res3.XSTSToken, token);
        profile = await MinecraftAPI.GetMinecraftProfileAsync(res4.AccessToken, token);
        if (profile == null || string.IsNullOrWhiteSpace(profile.Id))
        {
            throw new LoginException(LoginFailState.GetOAuthCodeDataFail, AuthState.Profile);
        }

        obj.UserName = profile.Name;
        obj.UUID = profile.Id;
        obj.Text1 = res1.RefreshToken;
        obj.AccessToken = res4.AccessToken!;

        return obj;
    }

    /// <summary>
    /// 从统一通行证登录
    /// </summary>
    /// <param name="server">服务器UUID</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <returns>登录结果</returns>
    public static async Task<LoginObj> LoginNide8Async(string server, string user, string pass, CancellationToken token)
    {
        var res1 = await Nide8.AuthenticateAsync(server, FuntionUtils.NewUUID(), user, pass, token);

        res1.Text2 = user;
        return res1;
    }

    /// <summary>
    /// 从统一通行证刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns>登录结果</returns>
    private static async Task<LoginObj> RefreshNide8Async(LoginObj obj, CancellationToken token)
    {
        return await Nide8.RefreshAsync(obj, token);
    }

    /// <summary>
    /// 从外置登录登录
    /// </summary>
    /// <param name="server">服务器地址</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <param name="select">选择项目</param>
    /// <returns>登录结果</returns>
    public static async Task<LoginObj> LoginAuthlibInjectorAsync(string server, string user, string pass, ILoginGui select, CancellationToken token)
    {
        var res1 = await AuthlibInjector.AuthenticateAsync(FuntionUtils.NewUUID(), user, pass, server, select, token);

        res1.Auth!.Text2 = user;
        return res1.Auth;
    }

    /// <summary>
    /// 从外置登录刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns>登录结果</returns>
    private static async Task<LoginObj> RefreshAuthlibInjectorAsync(LoginObj obj, CancellationToken token)
    {
        var res1 = await AuthlibInjector.RefreshAsync(obj, token);

        return res1.Auth!;
    }

    /// <summary>
    /// 从皮肤站登录
    /// </summary>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <param name="select">选择项目</param>
    /// <param name="server">自定义皮肤站地址</param>
    /// <returns>登录结果</returns>
    public static async Task<LoginObj> LoginLittleSkinAsync(string user, string pass, ILoginGui select, CancellationToken token, string? server = null)
    {
        var res1 = await LittleSkin.AuthenticateAsync(FuntionUtils.NewUUID(), user, pass, server, select, token);

        res1.Auth!.Text2 = user;
        return res1.Auth;
    }

    /// <summary>
    /// 从皮肤站刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns>登录结果</returns>
    private static async Task<LoginObj> RefreshLittleSkinAsync(LoginObj obj, CancellationToken token)
    {
        var res1 = await LittleSkin.RefreshAsync(obj, token);
        return res1.Auth!;
    }

    /// <summary>
    /// 刷新登录登录
    /// </summary>
    /// <param name="obj">登录信息</param>
    /// <returns>登录结果</returns>
    public static async Task<LoginObj> RefreshTokenAsync(this LoginObj obj, CancellationToken token)
    {
        return obj.AuthType switch
        {
            AuthType.OAuth => await RefreshOAuthAsync(obj, token),
            AuthType.Nide8 => await RefreshNide8Async(obj, token),
            AuthType.AuthlibInjector => await RefreshAuthlibInjectorAsync(obj, token),
            AuthType.LittleSkin or AuthType.SelfLittleSkin => await RefreshLittleSkinAsync(obj, token),
            _ => obj,
        };
    }
}
