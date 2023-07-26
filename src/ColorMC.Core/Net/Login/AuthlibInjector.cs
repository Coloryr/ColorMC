using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Net.Login;

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
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)> Authenticate(
         string clientToken, string user, string pass, string server)
    {
        var obj = await LoginOld.Authenticate(server, clientToken, user, pass);
        if (obj.State != LoginState.Done)
            return obj;

        obj.Obj!.AuthType = AuthType.AuthlibInjector;
        obj.Obj.Text1 = server;

        return obj;
    }

    /// <summary>
    /// 刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)> Refresh(LoginObj obj)
    {
        if (await LoginOld.Validate(obj.Text1, obj))
        {
            return (LoginState.Done, obj, null);
        }
        return await LoginOld.Refresh(obj.Text1, obj);
    }
}