using ColorMC.Core.GuiHandle;
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
    /// <param name="gui">登录参数</param>
    /// <returns>登录状态</returns>
    public static async Task<LoginObj?> LoginOAuthAsync(ILoginOAuthGui gui, CancellationToken token)
    {
        //获取登录码
        var res1 = await OAuthApi.GetCodeAsync(token);
        gui.LoginOAuthCode(res1.Url, res1.Code);
        //获取用户登录
        var res2 = await OAuthApi.RunGetCodeAsync(res1, token);
        if (res2 == null)
        {
            return null;
        }
        //Xbox登录
        gui.LoginOAuthState(AuthState.XBox);
        var res3 = await OAuthApi.GetXBoxAsync(res2.AccessToken, token);
        if (res3 == null)
        {
            return null;
        }
        //XSTS登录
        gui.LoginOAuthState(AuthState.XSTS);
        var res4 = await OAuthApi.GetXSTSAsync(res3.XBLToken, token);
        if (res4 == null)
        {
            return null;
        }
        //获取mojang token
        gui.LoginOAuthState(AuthState.Token);
        var res5 = await MinecraftAPI.GetMinecraftAsync(res4.XSTSUhs, res4.XSTSToken, token);
        if (res5 == null)
        {
            return null;
        }
        //获取minecraft账户
        gui.LoginOAuthState(AuthState.Profile);
        var profile = await MinecraftAPI.GetMinecraftProfileAsync(res5.AccessToken, token);
        if (profile == null || string.IsNullOrWhiteSpace(profile.Id))
        {
            throw new LoginException(LoginFailState.GetDataFail, AuthState.Profile);
        }

        return new LoginObj()
        {
            Text1 = res2.RefreshToken,
            AuthType = AuthType.OAuth,
            AccessToken = res5.AccessToken!,
            UserName = profile.Name,
            UUID = profile.Id,
            LastLogin = DateTime.Now
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
            throw new LoginException(LoginFailState.GetDataFail, AuthState.Profile);
        }

        obj.UserName = profile.Name;
        obj.UUID = profile.Id;
        obj.Text1 = res1.RefreshToken;
        obj.AccessToken = res4.AccessToken!;
        obj.LastLogin = DateTime.Now;

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
        var res1 = await Nide8.AuthenticateAsync(server, FunctionUtils.NewUUID(), user, pass, token);

        res1.Text2 = user;
        res1.LastLogin = DateTime.Now;
        return res1;
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
        var res1 = await AuthlibInjector.AuthenticateAsync(FunctionUtils.NewUUID(), user, pass, server, select, token);

        res1.Text2 = user;
        res1.LastLogin = DateTime.Now;
        return res1;
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
        var res1 = await LittleSkin.AuthenticateAsync(FunctionUtils.NewUUID(), user, pass, server, select, token);

        res1.Text2 = user;
        res1.LastLogin = DateTime.Now;
        return res1;
    }

    /// <summary>
    /// 刷新登录登录
    /// </summary>
    /// <param name="obj">登录信息</param>
    /// <returns>登录结果</returns>
    public static async Task<LoginObj> RefreshTokenAsync(this LoginObj obj, CancellationToken token)
    {
        var login = obj.AuthType switch
        {
            AuthType.OAuth => await RefreshOAuthAsync(obj, token),
            AuthType.Nide8 => await Nide8.RefreshAsync(obj, token),
            AuthType.AuthlibInjector => await AuthlibInjector.RefreshAsync(obj, token),
            AuthType.LittleSkin or AuthType.SelfLittleSkin => await LittleSkin.RefreshAsync(obj, token),
            _ => obj,
        };

        login.LastLogin = DateTime.Now;

        return login;
    }
}
