using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Mojang网络请求
/// </summary>
public static class MinecraftAPI
{
    public const string ProfileName = "https://api.mojang.com/users/profiles/minecraft/";
    public const string Profile = "https://api.minecraftservices.com/minecraft/profile";
    public const string UserProfile = "https://sessionserver.mojang.com/session/minecraft/profile";
    public const string LoginXbox = "https://api.minecraftservices.com/authentication/login_with_xbox";
    public const string News = "https://www.minecraft.net/content/minecraftnet/language-masters/zh-hans/jcr:content/root/container/image_grid_a_copy_64.articles.page-{0}.json";

    private static readonly HttpClient _client = new();

    static MinecraftAPI()
    {
        _client.DefaultRequestVersion = HttpVersion.Version11;
        _client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        _client.Timeout = TimeSpan.FromSeconds(20);
    }

    ///// <summary>
    ///// 从游戏名字获取UUID
    ///// </summary>
    ///// <param name="name"></param>
    ///// <returns></returns>
    //public static async Task<ProfileNameObj?> GetMinecraftProfileNameAsync(string name)
    //{
    //    string url = $"{ProfileName}/{name}";
    //    using var stream = await SendAsync(url);
    //    return JsonUtils.ToObj(stream, JsonType.ProfileNameObj);
    //}

    /// <summary>
    /// 获取账户信息
    /// </summary>
    /// <param name="accessToken">token</param>
    /// <returns>账户信息</returns>
    public static async Task<MinecraftProfileObj?> GetMinecraftProfileAsync(string accessToken)
    {
        HttpRequestMessage message = new(HttpMethod.Get, Profile);
        message.Headers.Add("Authorization", $"Bearer {accessToken}");
        using var data = await CoreHttpClient.SendLoginAsync(message);
        using var stream = await data.Content.ReadAsStreamAsync();
        return JsonUtils.ToObj(stream, JsonType.MinecraftProfileObj);
    }

    /// <summary>
    /// 获取皮肤信息
    /// </summary>
    /// <param name="uuid">uuid</param>
    /// <param name="url">网址</param>
    /// <returns>皮肤信息</returns>
    public static async Task<UserProfileObj?> GetUserProfile(string? uuid, string? url = null)
    {
        url ??= $"{UserProfile}/{uuid}";
        using var data = await CoreHttpClient.LoginGetAsync(url);
        using var stream = await data.Content.ReadAsStreamAsync();
        return JsonUtils.ToObj(stream, JsonType.UserProfileObj);
    }

    /// <summary>
    /// 获取账户信息
    /// </summary>
    public static async Task<MinecraftLoginRes> GetMinecraftAsync(string xstsUhs, string xstsToken)
    {
        var obj = new JsonObject()
        {
            { "identityToken", $"XBL3.0 x={xstsUhs};{xstsToken}" }
        };
        var json = await CoreHttpClient.LoginPostJsonAsync(LoginXbox, obj.ToJsonString());
        var accessToken = json?.GetString("access_token");
        var expireTime = json?.GetString("expires_in");

        if (string.IsNullOrWhiteSpace(accessToken) ||
            string.IsNullOrWhiteSpace(expireTime))
        {
            return new MinecraftLoginRes
            {
                State = LoginState.DataError
            };
        }

        return new MinecraftLoginRes
        {
            State = LoginState.Done,
            AccessToken = accessToken
        };
    }

    /// <summary>
    /// 获取Minecraft新闻信息
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static async Task<MinecraftNewObj?> GetMinecraftNew(int page = 0)
    {
        var url = string.Format(News, page + 1);
        var data = await _client.GetAsync(url);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }
        var stream = await data.Content.ReadAsStreamAsync();
        return JsonUtils.ToObj(stream, JsonType.MinecraftNewObj);
    }
}
