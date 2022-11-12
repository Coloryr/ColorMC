using ColorMC.Core.Http.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Login;

public enum AuthType
{
    OAuth
}

public enum AuthState
{
    OAuth, XBox, XSTS, Profile,
    Login, Token
}

public static class Auth
{
    public static async Task LoginWithOAuth()
    {
        try
        {
            var oauth = await OAuth.AddAuth();
            var XBL = await OAuth.GetXBLAsync(oauth.access_token);
            var XSTS = await OAuth.GetXSTSAsync(XBL.XNLToken);
            var Auth = await OAuth.GetMinecraftAsync(XSTS.XSTSUhs, XSTS.XSTSToken);
            return Auth.AccessToken;
        }
        catch (Exception e)
        {

        }
    }
}
