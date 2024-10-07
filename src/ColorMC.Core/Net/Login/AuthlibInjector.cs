using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Net.Login;

/// <summary>
/// 外置登录
/// </summary>
public static class AuthlibInjector
{
    /// <summary>
    /// 外置登录
    /// </summary>
    /// <param name="clientToken">客户端代码</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <param name="server">服务器地址</param>
    /// <returns></returns>
    public static async Task<LegacyLoginRes> AuthenticateAsync(string clientToken, string user, string pass, string server)
    {
        var obj = await LegacyLogin.AuthenticateAsync(server, clientToken, user, pass);
        if (obj.State != LoginState.Done)
            return obj;

        obj.Auth!.AuthType = AuthType.AuthlibInjector;
        obj.Auth.Text1 = server;

        if (obj.IsOne)
        {
            return obj;
        }

        return await LegacyLogin.RefreshAsync(server, obj.Auth, true);
    }

    /// <summary>
    /// 刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    public static async Task<LegacyLoginRes> RefreshAsync(LoginObj obj)
    {
        if (await LegacyLogin.ValidateAsync(obj.Text1, obj))
        {
            return await LegacyLogin.RefreshAsync(obj.Text1, obj, false);
        }

        return new() { State = LoginState.Error };
    }
}