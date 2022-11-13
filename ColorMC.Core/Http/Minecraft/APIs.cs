using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.MoJang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Http.MoJang;

public static class APIs
{
    /// <summary>
    /// Get player's minecraft profile
    /// </summary>
    /// <param name="accessToken">Minecraft access token</param>
    /// <returns></returns>
    /// <exception cref="FailedAuthenticationException">If authenticated user don't have minecraft, the exception will be thrown</exception>
    public static async Task<MinecraftProfileObj> GetMinecraftProfileAsync(string accessToken)
    {
        var endpoint = "https://api.minecraftservices.com/minecraft/profile";
        HttpRequestMessage message = new(HttpMethod.Get, endpoint);
        message.Headers.Add("Authorization", $"Bearer {accessToken}");
        var data = await BaseClient.Client.SendAsync(message);

        var data1 = await data.Content.ReadAsStringAsync();

        var re = JsonConvert.DeserializeObject<MinecraftProfileObj>(data1);
        if (re == null)
        {
            throw new Exception("Failed to retrieve Minecraft profile");
        }

        return re;
    }

    public static async Task<PlayerAttributesObj> GetPlayerAttributes(string accessToken)
    {
        var endpoint = "https://api.minecraftservices.com/minecraft/profile";
        HttpRequestMessage message = new(HttpMethod.Get, endpoint);
        message.Headers.Add("Authorization", $"Bearer {accessToken}");
        var data = await BaseClient.Client.SendAsync(message);

        var data1 = await data.Content.ReadAsStringAsync();

        var re = JsonConvert.DeserializeObject<PlayerAttributesObj>(data1);
        if (re == null)
        {
            throw new Exception("Failed to retrieve Player Attributes");
        }

        return re;
    }
}
