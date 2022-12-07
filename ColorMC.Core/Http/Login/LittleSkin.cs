using ColorMC.Core.Game.Auth;
using ColorMC.Core.Objs.Game;

namespace ColorMC.Core.Http.Login;

public static class LittleSkin
{
    private const string ServerUrl = "https://littleskin.cn/api/yggdrasil";
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)> Authenticate(string clientToken,
        string user, string pass, string? server = null)
    {
        server ??= ServerUrl;
        var obj = await LoginOld.Authenticate(server, clientToken, user, pass);
        if (obj.State != LoginState.Done)
            return obj;
        if (server == null)
        {
            obj.Obj.AuthType = AuthType.LittleSkin;
        }
        else
        {
            obj.Obj.AuthType = AuthType.SelfLittleSkin;
            obj.Obj.Text1 = server;
        }

        return obj;
    }

    public static Task<(LoginState State, LoginObj? Obj)> Refresh(LoginObj obj)
    {
        string server;
        if (obj.AuthType == AuthType.LittleSkin)
        {
            server = ServerUrl;
        }
        else
        {
            server = obj.Text1;
        }

        return LoginOld.Refresh(server, obj);
    }
}
