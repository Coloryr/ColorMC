using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Objs.MoJang;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Mojang网络请求
/// </summary>
public static class MinecraftAPI
{
    /// <summary>
    /// 获取账户信息
    /// </summary>
    /// <param name="accessToken">token</param>
    /// <returns>账户信息</returns>
    public static async Task<MinecraftProfileObj?> GetMinecraftProfileAsync(string accessToken)
    {
        var endpoint = "https://api.minecraftservices.com/minecraft/profile";
        HttpRequestMessage message = new(HttpMethod.Get, endpoint);
        message.Headers.Add("Authorization", $"Bearer {accessToken}");
        var data = await BaseClient.LoginClient.SendAsync(message);

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
        url ??= $"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}";
        var data = await BaseClient.LoginClient.GetStringAsync(url);

        return JsonConvert.DeserializeObject<UserProfileObj>(data);
    }
}
