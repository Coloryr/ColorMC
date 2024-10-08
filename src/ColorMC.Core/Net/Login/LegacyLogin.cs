using System.Net;
using System.Net.Http.Headers;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Net.Login;

/// <summary>
/// 旧版账户验证
/// </summary>
public static class LegacyLogin
{
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="server">服务器地址</param>
    /// <param name="clientToken">客户端代码</param>
    /// <param name="user">用户名</param>
    /// <param name="pass">密码</param>
    public static async Task<LegacyLoginRes> AuthenticateAsync(string server, string clientToken, string user, string pass)
    {
        var obj = new AuthenticateObj
        {
            agent = new()
            {
                name = "ColorMC",
                version = ColorMCCore.Version
            },
            username = user,
            password = pass,
            clientToken = clientToken
        };
        if (!server.EndsWith('/'))
        {
            server += "/";
        }
        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new(server + "authserver/authenticate")
        };
        message.Headers.UserAgent.Add(new("ColorMC", ColorMCCore.Version));
        message.Content = new StringContent(JsonConvert.SerializeObject(obj),
            MediaTypeHeaderValue.Parse("application/json"));

        var res = await CoreHttpClient.LoginClient.SendAsync(message);
        var data = await res.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(data))
        {
            return new LegacyLoginRes
            {
                State = LoginState.Error,
                Message = LanguageHelper.Get("Core.Login.Error22")
            };
        }

        var obj2 = JsonConvert.DeserializeObject<AuthenticateResObj>(data);

        if (obj2 == null)
        {
            return new LegacyLoginRes
            {
                State = LoginState.DataError,
                Message = LanguageHelper.Get("Core.Login.Error22")
            };
        }

        if (obj2.selectedProfile == null && obj2.availableProfiles.Count > 0)
        {
            foreach (var item in obj2.availableProfiles)
            {
                if (item.name.Equals(user, StringComparison.CurrentCultureIgnoreCase))
                {
                    obj2.selectedProfile = item;
                    break;
                }
            }
        }

        if (obj2.selectedProfile == null)
        {
            var obj1 = JObject.Parse(data);
            if (obj1?["errorMessage"]?.ToString() is { } msg)
            {
                return new LegacyLoginRes
                {
                    State = LoginState.Error,
                    Message = msg
                };
            }

            return new LegacyLoginRes
            {
                State = LoginState.Error,
                Message = LanguageHelper.Get("Core.Login.Error23")
            };
        }

        return new LegacyLoginRes
        {
            State = LoginState.Done,
            Auth = new()
            {
                UserName = obj2.selectedProfile.name,
                UUID = obj2.selectedProfile.id,
                AccessToken = obj2.accessToken,
                ClientToken = obj2.clientToken
            },
            IsOne = obj2.availableProfiles.Count <= 1
        };
    }

    /// <summary>
    /// 刷新登录
    /// </summary>
    /// <param name="server">服务器地址</param>
    /// <param name="obj">保存的账户</param>
    public static async Task<LegacyLoginRes> RefreshAsync(string server, LoginObj obj, bool select)
    {
        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new(server + "/authserver/refresh")
        };
        message.Headers.UserAgent.Add(new("ColorMC", ColorMCCore.Version));

        if (select)
        {
            var obj1 = new RefreshObj
            {
                accessToken = obj.AccessToken,
                clientToken = obj.ClientToken,
                selectedProfile = new()
                {
                    name = obj.UserName,
                    id = obj.UUID
                }
            };
            message.Content = new StringContent(JsonConvert.SerializeObject(obj1),
            MediaTypeHeaderValue.Parse("application/json"));
        }
        else
        {
            var obj1 = new RefreshObj
            {
                accessToken = obj.AccessToken,
                clientToken = obj.ClientToken
            };
            message.Content = new StringContent(JsonConvert.SerializeObject(obj1),
            MediaTypeHeaderValue.Parse("application/json"));
        }
        var res = await CoreHttpClient.LoginClient.SendAsync(message);
        var data = await res.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(data))
        {
            return new LegacyLoginRes
            {
                State = LoginState.Error,
                Message = LanguageHelper.Get("Core.Login.Error24")
            };
        }
        if (data.Contains("error") && data.Contains("errorMessage"))
        {
            var jobj = JObject.Parse(data);
            return new LegacyLoginRes
            {
                State = LoginState.Error,
                Message = jobj["errorMessage"]!.ToString()
            };
        }
        var obj2 = JsonConvert.DeserializeObject<AuthenticateResObj>(data);
        if (obj2 == null || (obj2.selectedProfile == null && !select))
        {
            return new LegacyLoginRes
            {
                State = LoginState.DataError,
                Message = LanguageHelper.Get("Core.Login.Error22")
            };
        }
        if (obj2.selectedProfile != null)
        {
            obj.UserName = obj2.selectedProfile.name;
            obj.UUID = obj2.selectedProfile.id;
        }
        obj.AccessToken = obj2.accessToken;
        obj.ClientToken = obj2.clientToken;

        return new LegacyLoginRes
        {
            State = LoginState.Done,
            Auth = obj
        };
    }

    /// <summary>
    /// 检测Token可用性
    /// </summary>
    /// <param name="server">检测地址</param>
    /// <param name="obj">保存的账户</param>
    /// <returns>可用性</returns>
    public static async Task<bool> ValidateAsync(string server, LoginObj obj)
    {
        var obj1 = new RefreshObj
        {
            accessToken = obj.AccessToken,
            clientToken = obj.ClientToken
        };
        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new(server + "/authserver/validate")
        };
        message.Headers.UserAgent.Add(new("ColorMC", ColorMCCore.Version));
        message.Content = new StringContent(JsonConvert.SerializeObject(obj1),
            MediaTypeHeaderValue.Parse("application/json"));

        var res = await CoreHttpClient.LoginClient.SendAsync(message);
        if (res.StatusCode == HttpStatusCode.NoContent)
        {
            return true;
        }

        return false;
    }
}
