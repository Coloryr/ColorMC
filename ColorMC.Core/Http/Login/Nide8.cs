using ColorMC.Core.Game.Auth;
using ColorMC.Core.Objs.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace ColorMC.Core.Http.Login;

public static class Nide8
{
    private const string BaseUrl = "https://auth.mc-user.com:233/";
    public static async Task<(LoginState State, LoginObj? Obj, string? Msg)> Authenticate(string server, string clientToken,
        string user, string pass)
    {
        var obj = new AuthenticateObj
        {
            agent = new()
            {
                name = "ColorMC",
                version = CoreMain.Version
            },
            username = user,
            password = pass,
            clientToken = clientToken
        };
        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new(BaseUrl + server + "/authserver/authenticate")
        };
        message.Headers.UserAgent.Add(ProductInfoHeaderValue.Parse($"ColorMC/{CoreMain.Version}"));
        message.Content = new StringContent(JsonConvert.SerializeObject(obj),
            MediaTypeHeaderValue.Parse("application/json"));

        var res = await BaseClient.Client.SendAsync(message);
        var data = await res.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(data))
            return (LoginState.Error, null, null);
        var obj1 = JObject.Parse(data);

        if (obj1.ContainsKey("error"))
        {
            return (LoginState.Error, null, obj1["errorMessage"].ToString());
        }

        var obj2 = obj1.ToObject<AuthenticateResObj>();
        if (obj2 == null)
            return (LoginState.JsonError, null, null);

        return (LoginState.Done, new()
        {
            UserName = obj2.selectedProfile.name,
            UUID = obj2.selectedProfile.id,
            AccessToken = obj2.accessToken,
            ClientToken = obj2.clientToken,
            AuthType = AuthType.Nide8,
            Text1 = server
        }, null);
    }

    public static async Task<(LoginState State, LoginObj? Obj)> Refresh(LoginObj obj)
    {
        var obj1 = new RefreshObj
        {
            accessToken = obj.AccessToken,
            clientToken = obj.ClientToken
        };
        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new(BaseUrl + obj.Text1 + "/authserver/refresh")
        };
        message.Headers.UserAgent.Add(new("ColorMC"));
        message.Content = new StringContent(JsonConvert.SerializeObject(obj1),
            MediaTypeHeaderValue.Parse("application/json"));

        var res = await BaseClient.Client.SendAsync(message);
        var data = await res.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(data))
            return (LoginState.Error, null);
        var obj2 = JsonConvert.DeserializeObject<AuthenticateResObj>(data);
        if (obj2 == null)
            return (LoginState.JsonError, null);

        obj.UserName = obj2.selectedProfile.name;
        obj.UUID = obj2.selectedProfile.id;
        obj.AccessToken = obj2.accessToken;
        obj.ClientToken = obj2.clientToken;

        return (LoginState.Done, obj);
    }
}
