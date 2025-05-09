using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Net.Login;

/// <summary>
/// LittleSkin登录
/// </summary>
public static class LittleSkin
{
    /// <summary>
    /// 皮肤站登录
    /// </summary>
    /// <param name="clientToken">客户端代码</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    /// <param name="server">服务器地址</param>
    public static async Task<LegacyLoginRes> AuthenticateAsync(string clientToken, string user, string pass, string? server, ColorMCCore.Select? select)
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
            if (!server.EndsWith('/'))
            {
                server += "/";
            }
            server1 = server;
        }

        var obj = await LegacyLogin.AuthenticateAsync(server1 + "api/yggdrasil", clientToken, user, pass, true);
        if (obj.State != LoginState.Done)
            return obj;

        if (obj.Logins != null)
        {
            if (select != null)
            {
                var index = await select(LanguageHelper.Get("Core.Login.Info1"), [.. obj.Logins.Select(item => item.UserName)]);
                if (index >= obj.Logins.Count || index < 0)
                {
                    return new()
                    {
                        State = LoginState.Error,
                        Message = LanguageHelper.Get("Core.Login.Error23")
                    };
                }
                var item = obj.Logins[index];
                obj.Auth!.UUID = item.UUID;
                obj.Auth.UserName = item.UserName;
            }
            else
            {
                var item = obj.Logins[0];
                obj.Auth!.UUID = item.UUID;
                obj.Auth.UserName = item.UserName;
            }
        }

        obj.Auth!.AuthType = type;
        if (type == AuthType.SelfLittleSkin)
        {
            obj.Auth.Text1 = server!;
        }

        return await LegacyLogin.RefreshAsync(server1 + "api/yggdrasil", obj.Auth, true);
    }

    /// <summary>
    /// 刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    public static async Task<LegacyLoginRes> RefreshAsync(LoginObj obj)
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

        if (!server.EndsWith('/'))
        {
            server += "/";
        }

        server += "api/yggdrasil";

        if (await LegacyLogin.ValidateAsync(server, obj))
        {
            return await LegacyLogin.RefreshAsync(server, obj, false);
        }

        return new() { State = LoginState.Error };
    }
}
