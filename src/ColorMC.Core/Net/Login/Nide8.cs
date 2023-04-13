using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Net.Login;

public static class Nide8
{
    private const string BaseUrl = "https://auth.mc-user.com:233/";
    /// <summary>
    /// 统一通行证登录
    /// </summary>
    /// <param name="server">服务器UUID</param>
    /// <param name="clientToken">客户端代码</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)> Authenticate(string server, string clientToken,
        string user, string pass)
    {
        string url = BaseUrl + server;

        var obj = await LoginOld.Authenticate(url, clientToken, user, pass);
        if (obj.State != LoginState.Done)
            return obj;

        obj.Obj.AuthType = AuthType.Nide8;
        obj.Obj.Text1 = server;

        return obj;
    }

    /// <summary>
    /// 刷新登录
    /// </summary>
    /// <param name="obj">保存的账户</param>
    public static Task<(LoginState State, LoginObj? Obj, string? Msg)> Refresh(LoginObj obj)
    {
        return LoginOld.Refresh(BaseUrl + obj.Text1, obj);
    }
}
