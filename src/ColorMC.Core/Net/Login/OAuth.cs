using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace ColorMC.Core.Net.Login;

/// <summary>
/// OAuth登录
/// </summary>
public static class OAuthAPI
{
    public const string OAuthCode = "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode";
    public const string OAuthToken = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
    public const string XboxLive = "https://user.auth.xboxlive.com/user/authenticate";
    public const string XSTS = "https://xsts.auth.xboxlive.com/xsts/authorize";
    public const string XBoxProfile = "https://api.minecraftservices.com/authentication/login_with_xbox";

    public readonly static Dictionary<string, string> Arg1 = new()
    {
        { "client_id", "aa0dd576-d717-4950-b257-a478d2c20968"},
        { "scope", "XboxLive.signin offline_access"}
    };

    public readonly static Dictionary<string, string> Arg2 = new()
    {
        { "client_id", "aa0dd576-d717-4950-b257-a478d2c20968"},
        { "grant_type", "urn:ietf:params:oauth:grant-type:device_code"},
        { "code", "" }
    };

    public readonly static Dictionary<string, string> Arg3 = new()
    {
        { "client_id", "aa0dd576-d717-4950-b257-a478d2c20968"},
        { "grant_type", "refresh_token"},
        { "refresh_token", "" }
    };

    private static string s_code;
    private static string s_url;
    private static string s_deviceCode;
    private static int s_expiresIn;
    private static CancellationTokenSource s_cancel;

    /// <summary>
    /// 请求数据
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="arg">参数</param>
    /// <returns>数据</returns>
    private static async Task<string> PostStringAsync(string url, Dictionary<string, string> arg)
    {
        FormUrlEncodedContent content = new(arg);
        using var message = await BaseClient.LoginClient.PostAsync(url, content);

        return await message.Content.ReadAsStringAsync();
    }
    /// <summary>
    /// 请求数据
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="arg">参数</param>
    /// <returns>数据</returns>
    private static async Task<JObject?> PostObjAsync(string url, object arg)
    {
        var data1 = JsonConvert.SerializeObject(arg);
        StringContent content = new(data1, MediaTypeHeaderValue.Parse("application/json"));
        using var message = await BaseClient.LoginClient.PostAsync(url, content);
        var data = await message.Content.ReadAsStringAsync();
        return JObject.Parse(data);
    }
    /// <summary>
    /// 请求数据
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="arg">参数</param>
    /// <returns>数据</returns>
    private static async Task<JObject?> PostObj(string url, Dictionary<string, string> arg)
    {
        FormUrlEncodedContent content = new(arg);
        using var message = await BaseClient.LoginClient.PostAsync(url, content);
        var data = await message.Content.ReadAsStringAsync();
        return JObject.Parse(data);
    }

    /// <summary>
    /// 获取登录码
    /// </summary>
    public static async Task<(LoginState Done, string? Code, string? Url)> GetCodeAsync()
    {
        var data = await PostStringAsync(OAuthCode, Arg1);
        if (data.Contains("error"))
        {
            return (LoginState.Error,
                LanguageHelper.Get("Core.Login.Error21"), null);
        }
        var obj1 = JsonConvert.DeserializeObject<OAuthObj>(data);
        if (obj1 == null)
        {
            return (LoginState.JsonError,
                LanguageHelper.Get("Core.Login.Error22"), null);
        }
        s_code = obj1.user_code;
        s_url = obj1.verification_uri;
        s_deviceCode = obj1.device_code;
        s_expiresIn = obj1.expires_in;

        return (LoginState.Done, s_code, s_url);
    }

    /// <summary>
    /// 获取token
    /// </summary>
    public static async Task<(LoginState Done, OAuth1Obj? Obj)> RunGetCodeAsync()
    {
        Arg2["code"] = s_deviceCode;
        long startTime = DateTime.Now.Ticks;
        int delay = 2;
        s_cancel = new();
        do
        {
            await Task.Delay(delay * 1000);
            if (s_cancel.IsCancellationRequested)
            {
                return (LoginState.Error, null);
            }
            long estimatedTime = DateTime.Now.Ticks - startTime;
            long sec = estimatedTime / 10000000;
            if (sec > s_expiresIn)
            {
                return (LoginState.TimeOut, null);
            }
            var data = await PostStringAsync(OAuthToken, Arg2);
            var obj3 = JObject.Parse(data);
            if (obj3 == null)
            {
                return (LoginState.JsonError, null);
            }
            if (obj3.ContainsKey("error"))
            {
                string? error = obj3["error"]?.ToString();
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
                    return (LoginState.Error, null);
                }
            }
            else
            {
                var obj4 = JsonConvert.DeserializeObject<OAuth1Obj>(data);
                if (obj4 == null)
                {
                    return (LoginState.JsonError, null);
                }

                return (LoginState.Done, obj4);
            }
        } while (true);
    }

    /// <summary>
    /// 刷新密匙
    /// </summary>
    public static async Task<(LoginState Done, OAuth1Obj? Auth)> RefreshTokenAsync(string token)
    {
        var dir = new Dictionary<string, string>(Arg3)
        {
            ["refresh_token"] = token
        };

        var obj1 = await PostObj(OAuthToken, dir);
        if (obj1 == null)
        {
            return (LoginState.JsonError, null);
        }
        if (obj1.ContainsKey("error"))
        {
            return (LoginState.Error, null);
        }
        return (LoginState.Done, obj1.ToObject<OAuth1Obj>());
    }

    /// <summary>
    /// Get Xbox live token & userhash
    /// </summary>
    /// <returns></returns>
    public static async Task<(LoginState Done, string? XNLToken, string? XBLUhs)> GetXBLAsync(string token)
    {
        var json = await PostObjAsync(XboxLive, new
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
        var xblToken = json?["Token"]?.ToString();
        var list = json?["DisplayClaims"]?["xui"] as JArray;
        var xblUhs = (list?[0] as JObject)?["uhs"]?.ToString();

        if (string.IsNullOrWhiteSpace(xblToken) ||
            string.IsNullOrWhiteSpace(xblUhs))
        {
            return (LoginState.JsonError, null, null);
        }

        return (LoginState.Done, xblToken, xblUhs);
    }

    /// <summary>
    /// Get Xbox security token service token & userhash
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FailedAuthenticationException"></exception>
    public static async Task<(LoginState Done, string? XSTSToken, string? XSTSUhs)> GetXSTSAsync(string token)
    {
        var json = await PostObjAsync(XSTS, new
        {
            Properties = new
            {
                SandboxId = "RETAIL",
                UserTokens = new[] { token }
            },
            RelyingParty = "rp://api.minecraftservices.com/",
            TokenType = "JWT"
        });
        var xstsToken = json?["Token"]?.ToString();
        var list = json?["DisplayClaims"]?["xui"] as JArray;
        var xstsUhs = (list?[0] as JObject)?["uhs"]?.ToString();

        if (string.IsNullOrWhiteSpace(xstsToken) ||
            string.IsNullOrWhiteSpace(xstsUhs))
        {
            return (LoginState.JsonError, null, null);
        }

        return (LoginState.Done, xstsToken, xstsUhs);
    }

    /// <summary>
    /// 获取账户信息
    /// </summary>
    public static async Task<(LoginState Done, string? AccessToken)> GetMinecraftAsync(string token, string token1)
    {
        var json = await PostObjAsync(XBoxProfile, new
        {
            identityToken = $"XBL3.0 x={token};{token1}"
        });
        var accessToken = json?["access_token"]?.ToString();
        var expireTime = json?["expires_in"]?.ToString();

        if (string.IsNullOrWhiteSpace(accessToken) ||
            string.IsNullOrWhiteSpace(expireTime))
        {
            return (LoginState.JsonError, null);
        }

        return (LoginState.Done, accessToken);
    }

    /// <summary>
    /// 取消请求
    /// </summary>
    public static void Cancel()
    {
        s_cancel.Cancel();
    }
}
