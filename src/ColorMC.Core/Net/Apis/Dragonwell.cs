using System.Net;
using ColorMC.Core.Objs.Java;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Dragonwell下载源
/// </summary>
public static class Dragonwell
{
    public const string Url = "https://dragonwell-jdk.io/releases.json";
    /// <summary>
    /// 获取列表
    /// </summary>
    public static async Task<DragonwellObj?> GetJavaList()
    {
        using var data = await CoreHttpClient.GetAsync(Url);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }
        using var str = await data.Content.ReadAsStreamAsync();
        return JsonUtils.ToObj(str, JsonType.DragonwellObj);
    }
}
