using System.Net;
using System.Net.Http.Headers;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
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
    public static async Task<LegacyLoginRes> AuthenticateAsync(string server, string clientToken, string user, string pass, bool useminecraft)
    {
        var obj = new AuthenticateObj
        {
            Agent = new()
            {
                Name = useminecraft ? "Minecraft" : "ColorMC",
                Version = useminecraft ? 1 : ColorMCCore.VersionNum
            },
            Username = user,
            Password = pass,
            ClientToken = clientToken
        };
        if (!server.EndsWith('/'))
        {
            server += "/";
        }
        var message = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new(server + "authserver/authenticate"),
            Content = new StringContent(JsonUtils.ToString(obj, JsonType.AuthenticateObj),
            MediaTypeHeaderValue.Parse("application/json"))
        };

        using var res = await CoreHttpClient.SendLoginAsync(message);
        using var data = await res.Content.ReadAsStreamAsync();
        var obj2 = JsonUtils.ToObj(data, JsonType.AuthenticateResObj);

        if (obj2 == null)
        {
            return new LegacyLoginRes
            {
                State = LoginState.DataError,
                Message = LanguageHelper.Get("Core.Login.Error22")
            };
        }
        else if (!string.IsNullOrWhiteSpace(obj2.ErrorMessage))
        {
            return new LegacyLoginRes
            {
                State = LoginState.Error,
                Message = obj2.ErrorMessage
            };
        }
        else if (obj2.SelectedProfile == null && obj2.AvailableProfiles == null)
        {
            return new LegacyLoginRes
            {
                State = LoginState.Error,
                Message = LanguageHelper.Get("Core.Login.Error23")
            };
        }
        else if (obj2.SelectedProfile != null)
        {
            return new LegacyLoginRes
            {
                State = LoginState.Done,
                Auth = new()
                {
                    UserName = obj2.SelectedProfile.Name,
                    UUID = obj2.SelectedProfile.Id,
                    AccessToken = obj2.AccessToken,
                    ClientToken = obj2.ClientToken
                }
            };
        }
        else if (obj2.AvailableProfiles.Count > 0)
        {
            if (obj2.AvailableProfiles.Count == 1)
            {
                obj2.SelectedProfile = obj2.AvailableProfiles.First();

                return new LegacyLoginRes
                {
                    State = LoginState.Done,
                    Auth = new()
                    {
                        UserName = obj2.SelectedProfile.Name,
                        UUID = obj2.SelectedProfile.Id,
                        AccessToken = obj2.AccessToken,
                        ClientToken = obj2.ClientToken
                    }
                };
            }
            else
            {
                foreach (var item in obj2.AvailableProfiles)
                {
                    if (item.Name.Equals(user, StringComparison.CurrentCultureIgnoreCase))
                    {
                        obj2.SelectedProfile = item;
                        break;
                    }
                }
                if (obj2.SelectedProfile != null)
                {
                    return new LegacyLoginRes
                    {
                        State = LoginState.Done,
                        Auth = new()
                        {
                            UserName = obj2.SelectedProfile.Name,
                            UUID = obj2.SelectedProfile.Id,
                            AccessToken = obj2.AccessToken,
                            ClientToken = obj2.ClientToken
                        }
                    };
                }

                var list = new List<LoginObj>();
                foreach (var item in obj2.AvailableProfiles)
                {
                    list.Add(new()
                    {
                        UserName = item.Name,
                        UUID = item.Id
                    });
                }
                return new LegacyLoginRes
                {
                    State = LoginState.Done,
                    Logins = list,
                    Auth = new()
                    {
                        AccessToken = obj2.AccessToken,
                        ClientToken = obj2.ClientToken
                    }
                };
            }
        }

        return new LegacyLoginRes
        {
            State = LoginState.DataError,
            Message = LanguageHelper.Get("Core.Login.Error22")
        };
    }

    /// <summary>
    /// 刷新登录
    /// </summary>
    /// <param name="server">服务器地址</param>
    /// <param name="obj">保存的账户</param>
    public static async Task<LegacyLoginRes> RefreshAsync(string server, LoginObj obj, bool select)
    {
        var message = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new(server + "/authserver/refresh")
        };

        if (select)
        {
            var obj1 = new RefreshObj
            {
                AccessToken = obj.AccessToken,
                ClientToken = obj.ClientToken,
                SelectedProfile = new()
                {
                    Name = obj.UserName,
                    Id = obj.UUID
                }
            };
            message.Content = new StringContent(JsonUtils.ToString(obj1, JsonType.RefreshObj),
            MediaTypeHeaderValue.Parse("application/json"));
        }
        else
        {
            var obj1 = new RefreshObj
            {
                AccessToken = obj.AccessToken,
                ClientToken = obj.ClientToken
            };
            message.Content = new StringContent(JsonUtils.ToString(obj1, JsonType.RefreshObj),
            MediaTypeHeaderValue.Parse("application/json"));
        }
        using var res = await CoreHttpClient.SendLoginAsync(message);
        using var data = await res.Content.ReadAsStreamAsync();
        var obj2 = JsonUtils.ToObj(data, JsonType.AuthenticateResObj);
        if (obj2 == null)
        {
            return new LegacyLoginRes
            {
                State = LoginState.Error,
                Message = LanguageHelper.Get("Core.Login.Error24")
            };
        }
        else if (obj2.Error != null && !string.IsNullOrEmpty(obj2.ErrorMessage))
        {
            return new LegacyLoginRes
            {
                State = LoginState.Error,
                Message = obj2.ErrorMessage
            };
        }
        if (obj2.SelectedProfile == null && !select)
        {
            return new LegacyLoginRes
            {
                State = LoginState.DataError,
                Message = LanguageHelper.Get("Core.Login.Error22")
            };
        }
        if (obj2.SelectedProfile != null)
        {
            obj.UserName = obj2.SelectedProfile.Name;
            obj.UUID = obj2.SelectedProfile.Id;
        }
        obj.AccessToken = obj2.AccessToken;
        obj.ClientToken = obj2.ClientToken;

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
            AccessToken = obj.AccessToken,
            ClientToken = obj.ClientToken
        };
        var message = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new(server + "/authserver/validate"),
            Content = new StringContent(JsonUtils.ToString(obj1, JsonType.RefreshObj),
            MediaTypeHeaderValue.Parse("application/json"))
        };

        var res = await CoreHttpClient.SendLoginAsync(message);
        if (res.StatusCode == HttpStatusCode.NoContent)
        {
            return true;
        }

        return false;
    }
}
