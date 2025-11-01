using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Net.Login;

/// <summary>
/// 统一通行证
/// </summary>
public static class Nide8
{
    /// <summary>
    /// 统一通行证登录
    /// </summary>
    /// <param name="server">服务器UUID</param>
    /// <param name="clientToken">客户端代码</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    public static async Task<LoginObj> AuthenticateAsync(string server, string clientToken, string user, string pass, CancellationToken token)
    {
        string url = UrlHelper.Nide8 + server;

        var obj = await LegacyLogin.AuthenticateAsync(url, clientToken, user, pass, false, token);
        obj.Auth!.AuthType = AuthType.Nide8;
        obj.Auth.Text1 = server;

        return obj.Auth;
    }

    /// <summary>
    /// 刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    public static async Task<LoginObj> RefreshAsync(LoginObj obj, CancellationToken token)
    {
        string server = UrlHelper.Nide8 + obj.Text1;
        if (await LegacyLogin.ValidateAsync(server, obj, token))
        {
            return obj;
        }

        var res = await LegacyLogin.RefreshAsync(server, obj, false, token);
        return res.Auth!;
    }
}
