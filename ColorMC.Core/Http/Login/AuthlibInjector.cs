using ColorMC.Core.Game.Auth;
using ColorMC.Core.Objs.Game;

namespace ColorMC.Core.Http.Login;

public static class AuthlibInjector
{
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)> Authenticate(
         string clientToken, string user, string pass, string? server = null)
    {
        var obj = await LoginOld.Authenticate(server, clientToken, user, pass);
        if (obj.State != LoginState.Done)
            return obj;

        obj.Obj.AuthType = AuthType.AuthlibInjector;
        obj.Obj.Text1 = server;

        return obj;
    }

    public static Task<(LoginState State, LoginObj? Obj)> Refresh(LoginObj obj)
    {
        return LoginOld.Refresh(obj.Text1, obj);
    }
}