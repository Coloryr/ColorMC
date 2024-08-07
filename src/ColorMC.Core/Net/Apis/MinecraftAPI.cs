using System.Net.Http.Headers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Objs.MoJang;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Mojang网络请求
/// </summary>
public static class MinecraftAPI
{
    public const string Profile = "https://api.minecraftservices.com/minecraft/profile";
    public const string UserProfile = "https://sessionserver.mojang.com/session/minecraft/profile";
    public const string LoginXbox = "https://api.minecraftservices.com/authentication/login_with_xbox";
    public const string News = "https://www.minecraft.net/content/minecraft-net/_jcr_content.articles.grid?tileselection=auto&tagsPath=minecraft:stockholm/news,minecraft:stockholm/guides,minecraft:stockholm/events,minecraft:stockholm/minecraft-builds,minecraft:stockholm/marketplace,minecraft:stockholm/deep-dives,minecraft:stockholm/merch,minecraft:article/culture,minecraft:article/insider,minecraft:article/merch,minecraft:article/news&propResPath=/content/minecraft-net/language-masters/en-us/articles/jcr:content/root/generic-container/par/bleeding_page_sectio/page-section-par/grid_copy&count=2000&pageSize={0}&offset={1}&locale=en-us&lang=/content/minecraft-net/language-masters/en-us";
    public const int PageSize = 18;

    /// <summary>
    /// 获取账户信息
    /// </summary>
    /// <param name="accessToken">token</param>
    /// <returns>账户信息</returns>
    public static async Task<MinecraftProfileObj?> GetMinecraftProfileAsync(string accessToken)
    {
        HttpRequestMessage message = new(HttpMethod.Get, Profile);
        message.Headers.Add("Authorization", $"Bearer {accessToken}");
        var data = await WebClient.LoginClient.SendAsync(message);
        var data1 = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<MinecraftProfileObj>(data1); ;
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
        var data = await WebClient.LoginClient.GetStringAsync(url);

        return JsonConvert.DeserializeObject<UserProfileObj>(data);
    }

    /// <summary>
    /// 获取账户信息
    /// </summary>
    public static async Task<MinecraftLoginRes> GetMinecraftAsync(string xstsUhs, string xstsToken)
    {
        var json = await WebClient.LoginPostJsonAsync(LoginXbox, new
        {
            identityToken = $"XBL3.0 x={xstsUhs};{xstsToken}"
        });
        var accessToken = json?["access_token"]?.ToString();
        var expireTime = json?["expires_in"]?.ToString();

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
        var url = string.Format(News, PageSize, page * PageSize);
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        var data = await WebClient.DownloadClient.SendAsync(req);
        var data1 = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<MinecraftNewObj>(data1);
    }
}
