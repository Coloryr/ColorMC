using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Http.Login;

public record OAuthObj
{ 
    public string user_code { get; set; }
    public string device_code { get; set; }
    public string verification_uri { get; set; }
    public int expires_in { get; set; }
    public int interval { get; set; }
    public string message { get; set; }
}

public record OAuth1Obj
{
    public string token_type { get; set; }
    public string scope { get; set; }
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public int ext_expires_in { get; set; }
    public string id_token { get; set; }
    public string refresh_token { get; set; }
}

public static class OAuth
{
    private const string OAuthCode = "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode";
    private const string OAuthToken = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
    private const string XboxLive = "https://user.auth.xboxlive.com/user/authenticate";
    private const string XSTS = "https://xsts.auth.xboxlive.com/xsts/authorize";
    private const string XBoxProfile = "https://api.minecraftservices.com/authentication/login_with_xbox";


    private static Dictionary<string, string> Arg1 = new()
    {
        { "client_id", "aa0dd576-d717-4950-b257-a478d2c20968"},
        { "scope", "XboxLive.signin offline_access"}
    };

    private static Dictionary<string, string> Arg2 = new()
    {
        { "client_id", "aa0dd576-d717-4950-b257-a478d2c20968"},
        { "grant_type", "urn:ietf:params:oauth:grant-type:device_code"},
        { "code", "" }
    };

    private static Dictionary<string, string> Arg3 = new()
    {
        { "client_id", "aa0dd576-d717-4950-b257-a478d2c20968"},
        { "grant_type", "refresh_token"},
        { "scope", "XboxLive.signin offline_access"},
        { "refresh_token", "" }
    };

    public static string code { get; private set; }
    public static string url { get; private set; }

    public static async Task<OAuth1Obj?> AddAuth()
    {
        var data = await BaseClient.PostString(OAuthCode, Arg1);
        var obj1 = JObject.Parse(data);
        if (obj1.ContainsKey("error"))
        {
            return null;
        }
        var obj2 = obj1.ToObject<OAuthObj>();
        if (obj2 == null)
            return null;
        code = obj2.user_code;
        url = obj2.verification_uri;
        Console.WriteLine(code);
        Console.WriteLine(url);
        Arg2["code"] = obj2.device_code;
        long startTime = DateTime.Now.Ticks;
        int delay = 5;
        OAuth1Obj obj4;
        do
        {
            Thread.Sleep(delay * 1000);
            long estimatedTime = DateTime.Now.Ticks - startTime;
            long sec = estimatedTime / 10000000;
            if (sec > obj2.expires_in)
            {
                return null;
            }
            data = await BaseClient.PostString(OAuthToken, Arg2);
            var obj3 = JObject.Parse(data);
            if (obj3.ContainsKey("error"))
            {
                string error = obj3["error"].ToString();
                if (error == "authorization_pending")
                {
                    continue;
                }
                else if (error == "slow_down")
                {
                    delay += 5;
                }
                else if (error == "expired_token")
                {
                    return null;
                }
            }
            else
            {
                obj4 = obj3.ToObject<OAuth1Obj>();
                return obj4;
            }
        } while (true);
    }

    public static async Task<OAuth1Obj?> RefreshTokenAsync(string token)
    {
        var dir = new Dictionary<string, string>(Arg3);
        dir["refresh_token"] = token;

        var obj1 = await BaseClient.PostObj(OAuthToken, dir);
        if (obj1.ContainsKey("error"))
        {
            return null;
        }
        return obj1.ToObject<OAuth1Obj>();
    }

    /// <summary>
    /// Get Xbox live token & userhash
    /// </summary>
    /// <returns></returns>
    public static async Task<(bool Done, string? XNLToken, string? XBLUhs)> GetXBLAsync(string token)
    {
        try
        {
            var json = await BaseClient.PostObj(XboxLive, new
            {
                Properties = new
                {
                    AuthMethod = "RPS",
                    SiteName = "user.auth.xboxlive.com",
                    RpsTicket = $"d={token}"
                },
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            });
            var xblToken = json.GetValue("Token")?.ToString();
            var list = json["DisplayClaims"]?["xui"] as JArray;
            var xblUhs = (list?.First() as JObject)?.GetValue("uhs")?.ToString();

            if (string.IsNullOrWhiteSpace(xblToken) ||
                string.IsNullOrWhiteSpace(xblUhs))
            {
                return (false, null, null);
            }

            return (true, xblToken, xblUhs);
        }
        catch (Exception e)
        {
            Logs.Error("OAuth登录错误", e);
            return (false, null, null);
        }
    }

    /// <summary>
    /// Get Xbox security token service token & userhash
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FailedAuthenticationException"></exception>
    public static async Task<(bool Done, string? XSTSToken, string? XSTSUhs)> GetXSTSAsync(string token)
    {
        try
        {
            var json = await BaseClient.PostObj(XSTS, new
            {
                Properties = new
                {
                    SandboxId = "RETAIL",
                    UserTokens = new[]
                    {
                        token
                    }
                },
                RelyingParty = "rp://api.minecraftservices.com/",
                TokenType = "JWT"
            });
            var xstsToken = json.GetValue("Token")?.ToString();
            var list = json["DisplayClaims"]?["xui"] as JArray; 
            var xstsUhs = (list?.First() as JObject)?.GetValue("uhs")?.ToString();

            if (string.IsNullOrWhiteSpace(xstsToken) ||
                string.IsNullOrWhiteSpace(xstsUhs))
            {
                return (false, null, null);
            }

            return (true, xstsToken, xstsUhs);
        }
        catch (Exception e)
        {
            Logs.Error("OAuth登录错误", e);
            return (false, null, null);
        }
    }

    public static async Task<(bool Done, string? AccessToken)> GetMinecraftAsync(string token, string token1)
    {
        try
        {
            var json = await BaseClient.PostObj(XBoxProfile, new
            {
                identityToken = $"XBL3.0 x={token};{token1}"
            });
            var accessToken = json.GetValue("access_token")?.ToString();
            var expireTime = json.GetValue("expires_in")?.ToString();

            if (string.IsNullOrWhiteSpace(accessToken) ||
                string.IsNullOrWhiteSpace(expireTime))
            {
                return (false, null);
            }

            return (true, accessToken);
        }
        catch (Exception e)
        {
            Logs.Error("OAuth登录错误", e);
            return (false, null);
        }
    }
}
