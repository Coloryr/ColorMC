using ColorMC.Core.Helpers;
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

    private static string s_code;
    private static string s_url;
    private static string s_deviceCode;
    private static int s_expiresIn;
    private static CancellationTokenSource s_cancel;

    /// <summary>
    /// 获取登录码
    /// </summary>
    public static async Task<OAuthGetCodeRes> GetCodeAsync()
    {
        using var stream = await CoreHttpClient.LoginPostStreamAsync(OAuthCode, Arg1);
        var obj = JsonUtils.ToObj(stream, JsonType.OAuthObj);
        if (obj == null)
        {
            return new OAuthGetCodeRes
            {
                State = LoginState.DataError,
                Message = LanguageHelper.Get("Core.Login.Error22")
            };
        }
        else if (string.IsNullOrWhiteSpace(obj.Error))
        {
            return new OAuthGetCodeRes
            {
                State = LoginState.Error,
                Message = LanguageHelper.Get("Core.Login.Error21")
            };
        }
        s_code = obj.UserCode;
        s_url = obj.VerificationUri;
        s_deviceCode = obj.DeviceCode;
        s_expiresIn = obj.ExpiresIn;

        return new OAuthGetCodeRes
        {
            State = LoginState.Done,
            Code = s_code,
            Message = s_url
        };
    }

    /// <summary>
    /// 获取token
    /// </summary>
    public static async Task<OAuthGetTokenRes> RunGetCodeAsync()
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
                return new OAuthGetTokenRes
                {
                    State = LoginState.Error
                };
            }
            long estimatedTime = DateTime.Now.Ticks - startTime;
            long sec = estimatedTime / 10000000;
            if (sec > s_expiresIn)
            {
                return new OAuthGetTokenRes
                {
                    State = LoginState.TimeOut
                };
            }
            using var stream = await CoreHttpClient.LoginPostStreamAsync(OAuthToken, Arg2);
            var obj = JsonUtils.ToObj(stream, JsonType.OAuthGetCodeObj);
            if (obj == null)
            {
                return new OAuthGetTokenRes
                {
                    State = LoginState.DataError
                };
            }
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
                    return new OAuthGetTokenRes
                    {
                        State = LoginState.Error
                    };
                }
            }
            else
            {
                return new OAuthGetTokenRes
                {
                    State = LoginState.Done,
                    Obj = obj
                };
            }
        } while (true);
    }

    /// <summary>
    /// 刷新密匙
    /// </summary>
    public static async Task<OAuthRefreshTokenRes> RefreshTokenAsync(string token)
    {
        var dir = new Dictionary<string, string>(Arg3)
        {
            ["refresh_token"] = token
        };

        using var stream = await CoreHttpClient.LoginPostStreamAsync(OAuthToken, dir);
        if (stream == null)
        {
            return new OAuthRefreshTokenRes
            {
                State = LoginState.DataError
            };
        }
        var obj = JsonUtils.ToObj(stream, JsonType.OAuthGetCodeObj);
        if (obj == null)
        {
            return new OAuthRefreshTokenRes
            {
                State = LoginState.DataError
            };
        }
        if (!string.IsNullOrWhiteSpace(obj.Error))
        {
            return new OAuthRefreshTokenRes
            {
                State = LoginState.Error
            };
        }

        return new OAuthRefreshTokenRes
        {
            State = LoginState.Done,
            Obj = obj
        };
    }

    /// <summary>
    /// Get Xbox live token & userhash
    /// </summary>
    /// <returns></returns>
    public static async Task<OAuthXboxLiveRes> GetXBLAsync(string token)
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
        var json = await CoreHttpClient.LoginPostJsonAsync(XboxLive, JsonUtils.ToString(obj, JsonType.OAuthLoginObj));
        if (json == null)
        { 
            return new OAuthXboxLiveRes
            {
                State = LoginState.DataError
            };
        }
        var xblToken = json.GetString("Token");
        var list = json.GetObj("DisplayClaims")?.GetArray("xui");
        if (list == null)
        {
            return new OAuthXboxLiveRes
            {
                State = LoginState.DataError
            };
        }
        var xblUhs = list.FirstOrDefault()?.AsObject().GetString("uhs");

        if (string.IsNullOrWhiteSpace(xblToken) ||
            string.IsNullOrWhiteSpace(xblUhs))
        {
            return new OAuthXboxLiveRes
            {
                State = LoginState.DataError
            };
        }

        return new OAuthXboxLiveRes
        {
            State = LoginState.Done,
            XBLToken = xblToken,
            XBLUhs = xblUhs
        };
    }

    /// <summary>
    /// Get Xbox security token service token & userhash
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FailedAuthenticationException"></exception>
    public static async Task<OAuthXSTSRes> GetXSTSAsync(string token)
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
        var json = await CoreHttpClient.LoginPostJsonAsync(XSTS, JsonUtils.ToString(obj, JsonType.OAuthLogin1Obj));
        if (json == null)
        {
            return new OAuthXSTSRes
            {
                State = LoginState.DataError
            };
        }
        var xstsToken = json.GetString("Token");
        var list = json.GetObj("DisplayClaims")?.GetArray("xui");
        if (list == null)
        {
            return new OAuthXSTSRes
            {
                State = LoginState.DataError
            };
        }
        var xstsUhs = list.FirstOrDefault()?.AsObject().GetString("uhs");

        if (string.IsNullOrWhiteSpace(xstsToken) ||
            string.IsNullOrWhiteSpace(xstsUhs))
        {
            return new OAuthXSTSRes
            {
                State = LoginState.DataError
            };
        }

        return new OAuthXSTSRes
        {
            State = LoginState.Done,
            XSTSToken = xstsToken,
            XSTSUhs = xstsUhs
        };
    }

    /// <summary>
    /// 取消请求
    /// </summary>
    public static void Cancel()
    {
        s_cancel.Cancel();
    }
}
