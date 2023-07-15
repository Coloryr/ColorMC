using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Net.Login;

public static class LittleSkin
{
    /// <summary>
    /// 皮肤站登录
    /// </summary>
    /// <param name="clientToken">客户端代码</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <param name="server">服务器地址</param>
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)> Authenticate(string clientToken,
        string user, string pass, string? server = null)
    {
        var type = AuthType.LittleSkin;
        string server1;
        if (string.IsNullOrWhiteSpace(server))
        {
            server1 = UrlHelper.LittleSkin;
        }
        else
        {
            type = AuthType.SelfLittleSkin;
            if (server.EndsWith("/api/yggdrasil"))
            {
                server = server.Replace("/api/yggdrasil", "/");
            }
            if (server.EndsWith("/user"))
            {
                server = server.Replace("/user", "/");
            }
            if (!server.EndsWith("/"))
            {
                server += "/";
            }
            server1 = server;
        }

        var obj = await LoginOld.Authenticate(server1 + "api/yggdrasil", clientToken, user, pass);
        if (obj.State != LoginState.Done)
            return obj;

        obj.Obj!.AuthType = type;
        if (type == AuthType.SelfLittleSkin)
        {
            obj.Obj.Text1 = server!;
        }

        return obj;
    }

    /// <summary>
    /// 刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)> Refresh(LoginObj obj)
    {
        string server;
        if (obj.AuthType == AuthType.LittleSkin)
        {
            server = UrlHelper.LittleSkin;
        }
        else
        {
            server = obj.Text1;
        }

        if (!server.EndsWith("/"))
        {
            server += "/";
        }

        server += "api/yggdrasil";

        if (await LoginOld.Validate(server, obj))
        {
            return (LoginState.Done, obj, null);
        }

        return await LoginOld.Refresh(server, obj);
    }
}
