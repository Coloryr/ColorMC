using ColorMC.Core.Objs.Java;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Dragonwell下载源
/// </summary>
public static class Dragonwell
{
    private const string Url = "https://dragonwell-jdk.io/releases.json";
    /// <summary>
    /// 获取列表
    /// </summary>
    public static async Task<DragonwellObj?> GetJavaList()
    {
        var data = await CoreHttpClient.DownloadClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<DragonwellObj>(str);
    }
}
