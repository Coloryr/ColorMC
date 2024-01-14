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
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)>
        AuthenticateAsync(string server, string clientToken, string user, string pass)
    {
        string url = UrlHelper.Nide8 + server;

        var obj = await LoginOld.AuthenticateAsync(url, clientToken, user, pass);
        if (obj.State != LoginState.Done)
            return obj;

        obj.Obj!.AuthType = AuthType.Nide8;
        obj.Obj.Text1 = server;

        return obj;
    }

    /// <summary>
    /// 刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)> RefreshAsync(LoginObj obj)
    {
        string server = UrlHelper.Nide8 + obj.Text1;
        if (await LoginOld.ValidateAsync(server + "/authserver/validate", obj))
        {
            return (LoginState.Done, obj, null);
        }

        return await LoginOld.RefreshAsync(server, obj);
    }
}
