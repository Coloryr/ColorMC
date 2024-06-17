using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        var data = await BaseClient.LoginPostStringAsync(OAuthCode, Arg1);
        if (data.Contains("error"))
        {
            return new OAuthGetCodeRes
            {
                State = LoginState.Error,
                Message = LanguageHelper.Get("Core.Login.Error21")
            };
        }
        var obj1 = JsonConvert.DeserializeObject<OAuthObj>(data);
        if (obj1 == null)
        {
            return new OAuthGetCodeRes
            {
                State = LoginState.DataError,
                Message = LanguageHelper.Get("Core.Login.Error22")
            };
        }
        s_code = obj1.user_code;
        s_url = obj1.verification_uri;
        s_deviceCode = obj1.device_code;
        s_expiresIn = obj1.expires_in;

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
            var data = await BaseClient.LoginPostStringAsync(OAuthToken, Arg2);
            var obj3 = JObject.Parse(data);
            if (obj3 == null)
            {
                return new OAuthGetTokenRes
                {
                    State = LoginState.DataError
                };
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
                    return new OAuthGetTokenRes
                    {
                        State = LoginState.Error
                    };
                }
            }
            else
            {
                var obj4 = JsonConvert.DeserializeObject<OAuthGetCodeObj>(data);
                if (obj4 == null)
                {
                    return new OAuthGetTokenRes
                    {
                        State = LoginState.DataError
                    };
                }

                return new OAuthGetTokenRes
                {
                    State = LoginState.Done,
                    Obj = obj4
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

        var obj1 = await BaseClient.LoginPostAsync(OAuthToken, dir);
        if (obj1 == null)
        {
            return new OAuthRefreshTokenRes
            {
                State = LoginState.DataError
            };
        }
        if (obj1.ContainsKey("error"))
        {
            return new OAuthRefreshTokenRes
            {
                State = LoginState.Error
            };
        }

        var obj2 = obj1.ToObject<OAuthGetCodeObj>();
        if (obj2 == null)
        {
            return new OAuthRefreshTokenRes
            {
                State = LoginState.DataError
            };
        }

        return new OAuthRefreshTokenRes
        {
            State = LoginState.Done,
            Obj = obj2
        };
    }

    /// <summary>
    /// Get Xbox live token & userhash
    /// </summary>
    /// <returns></returns>
    public static async Task<OAuthXboxLiveRes> GetXBLAsync(string token)
    {
        var json = await BaseClient.LoginPostJsonAsync(XboxLive, new
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
        var json = await BaseClient.LoginPostJsonAsync(XSTS, new
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
