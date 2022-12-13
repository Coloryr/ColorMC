using ColorMC.Core.Game.Auth;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Http.Login;

public static class Nide8
{
    private const string BaseUrl = "https://auth.mc-user.com:233/";
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)> Authenticate(string server, string clientToken,
        string user, string pass)
    {
        server = BaseUrl + server;

        var obj = await LoginOld.Authenticate(server, clientToken, user, pass);
        if (obj.State != LoginState.Done)
            return obj;

        obj.Obj.AuthType = AuthType.Nide8;
        obj.Obj.Text1 = server;

        return obj;
    }

    public static Task<(LoginState State, LoginObj? Obj)> Refresh(LoginObj obj)
    {
        return LoginOld.Refresh(BaseUrl + obj.Text1, obj);
    }
}
