using System.Text.Json;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// OAuth登录
/// </summary>
public static class OAuthApi
{
    public const string OAuthCode = "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode";
    public const string OAuthToken = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
    public const string XboxLive = "https://user.auth.xboxlive.com/user/authenticate";
    public const string XSTS = "https://xsts.auth.xboxlive.com/xsts/authorize";

    public static readonly Dictionary<string, string> Arg1 = new()
    {
        { "client_id", ColorMCCore.CoreArg.OAuthKey ?? throw new Exception("OAuth key is null") },
        { "scope", "XboxLive.signin offline_access" }
    };

    public static readonly Dictionary<string, string> Arg2 = new()
    {
        { "client_id", ColorMCCore.CoreArg.OAuthKey },
        { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" },
        { "code", "" }
    };

    public static readonly Dictionary<string, string> Arg3 = new()
    {
        { "client_id", ColorMCCore.CoreArg.OAuthKey },
        { "grant_type", "refresh_token" },
        { "refresh_token", "" }
    };

    /// <summary>
    /// 获取登录码
    /// </summary>
    public static async Task<OAuthGetCodeRes> GetCodeAsync(CancellationToken token)
    {
        using var stream = await CoreHttpClient.LoginPostStreamAsync(OAuthCode, Arg1, token);
        var obj = JsonUtils.ToObj(stream, JsonType.OAuthObj);
        if (obj == null)
        {
            throw new LoginException(LoginFailState.GetOAuthCodeDataFail, AuthState.OAuth);
        }
        else if (!string.IsNullOrWhiteSpace(obj.Error))
        {
            throw new LoginException(LoginFailState.GetOAuthCodeDataError, AuthState.OAuth, data: obj.Error);
        }

        return new OAuthGetCodeRes
        {
            Code = obj.UserCode,
            Url = obj.VerificationUri,
            DeviceCode = obj.DeviceCode,
            ExpiresIn = obj.ExpiresIn
        };
    }

    /// <summary>
    /// 获取token
    /// </summary>
    public static async Task<OAuthGetCodeObj?> RunGetCodeAsync(OAuthGetCodeRes res, CancellationToken token)
    {
        Arg2["code"] = res.Code;
        long startTime = DateTime.Now.Ticks;
        int delay = 2;
        do
        {
            await Task.Delay(delay * 1000, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }
            long estimatedTime = DateTime.Now.Ticks - startTime;
            long sec = estimatedTime / 10000000;
            if (sec > res.ExpiresIn)
            {
                throw new LoginException(LoginFailState.OAuthGetTokenTimeout, AuthState.OAuth);
            }
            using var stream = await CoreHttpClient.LoginPostStreamAsync(OAuthToken, Arg2, token);
            var obj = JsonUtils.ToObj(stream, JsonType.OAuthGetCodeObj)
                ?? throw new LoginException(LoginFailState.GetOAuthCodeDataFail, AuthState.OAuth);
            if (!string.IsNullOrWhiteSpace(obj.Error))
            {
                if (obj.Error == "authorization_pending")
                {
                    continue;
                }
                else if (obj.Error == "slow_down")
                {
                    delay += 5;
                }
                else if (obj.Error == "expired_token")
                {
                    throw new LoginException(LoginFailState.GetOAuthCodeDataError, AuthState.OAuth, data: obj.Error);
                }
            }
            else
            {
                return obj;
            }
        } while (true);
    }

    /// <summary>
    /// 刷新密匙
    /// </summary>
    public static async Task<OAuthGetCodeObj> RefreshOAuthTokenAsync(string token, CancellationToken cancel)
    {
        var dir = new Dictionary<string, string>(Arg3)
        {
            ["refresh_token"] = token
        };

        using var stream = await CoreHttpClient.LoginPostStreamAsync(OAuthToken, dir, cancel)
            ?? throw new LoginException(LoginFailState.GetOAuthCodeDataFail, AuthState.OAuth);
        var obj = JsonUtils.ToObj(stream, JsonType.OAuthGetCodeObj)
            ?? throw new LoginException(LoginFailState.GetOAuthCodeDataFail, AuthState.OAuth);
        if (!string.IsNullOrWhiteSpace(obj.Error))
        {
            throw new LoginException(LoginFailState.GetOAuthCodeDataError, AuthState.OAuth, data: obj.Error);
        }

        return obj;
    }

    /// <summary>
    /// Get Xbox live token & userhash
    /// </summary>
    /// <returns></returns>
    public static async Task<OAuthXboxLiveRes> GetXBoxAsync(string token, CancellationToken cancel)
    {
        var obj = new OAuthLoginObj()
        {
            Properties = new()
            {
                AuthMethod = "RPS",
                SiteName = "user.auth.xboxlive.com",
                RpsTicket = $"d={token}"
            },
            RelyingParty = "http://auth.xboxlive.com",
            TokenType = "JWT"
        };
        var obj1 = await CoreHttpClient.LoginPostJsonAsync(XboxLive, JsonUtils.ToString(obj, JsonType.OAuthLoginObj), cancel)
            ?? throw new LoginException(LoginFailState.GetOAuthCodeDataFail, AuthState.XBox);
        var json = obj1.RootElement;
        var xblToken = json.GetProperty("Token").GetString();
        var list = json.GetProperty("DisplayClaims").GetProperty("xui");
        if (list.ValueKind != JsonValueKind.Array)
        {
            throw new LoginException(LoginFailState.GetOAuthCodeDataError, AuthState.XBox, data: json.ToString());
        }
        var xblUhs = list.EnumerateArray().FirstOrDefault().GetProperty("uhs").GetString();

        if (string.IsNullOrWhiteSpace(xblToken) ||
            string.IsNullOrWhiteSpace(xblUhs))
        {
            throw new LoginException(LoginFailState.GetOAuthCodeDataError, AuthState.XBox, data: json.ToString());
        }

        return new OAuthXboxLiveRes
        {
            XBLToken = xblToken,
            XBLUhs = xblUhs
        };
    }

    /// <summary>
    /// Get Xbox security token service token & userhash
    /// </summary>
    /// <returns></returns>
    public static async Task<OAuthXSTSRes> GetXSTSAsync(string token, CancellationToken cancel)
    {
        var obj = new OAuthLogin1Obj()
        {
            Properties = new()
            {
                SandboxId = "RETAIL",
                UserTokens = [token]
            },
            RelyingParty = "rp://api.minecraftservices.com/",
            TokenType = "JWT"
        };
        var obj1 = await CoreHttpClient.LoginPostJsonAsync(XSTS, JsonUtils.ToString(obj, JsonType.OAuthLogin1Obj), cancel)
            ?? throw new LoginException(LoginFailState.GetOAuthCodeDataFail, AuthState.XSTS);
        var json = obj1.RootElement;
        var xstsToken = json.GetProperty("Token").GetString();
        var list = json.GetProperty("DisplayClaims").GetProperty("xui");
        if (list.ValueKind != JsonValueKind.Array)
        {
            throw new LoginException(LoginFailState.GetOAuthCodeDataError, AuthState.XSTS, data: json.ToString());
        }
        var xstsUhs = list.EnumerateArray().FirstOrDefault().GetProperty("uhs").GetString();

        if (string.IsNullOrWhiteSpace(xstsToken) ||
            string.IsNullOrWhiteSpace(xstsUhs))
        {
            throw new LoginException(LoginFailState.GetOAuthCodeDataError, AuthState.XSTS, data: json.ToString());
        }

        return new OAuthXSTSRes
        {
            XSTSToken = xstsToken,
            XSTSUhs = xstsUhs
        };
    }
}
