using ColorMC.Core.Objs.Java;
using System.Text.Json;

namespace ColorMC.Core.Net.Java;

public static class Dragonwell
{
    /// <summary>
    /// 获取列表
    /// </summary>
    public static async Task<DragonwellObj?> GetJavaList()
    {
        var url = "https://dragonwell-jdk.io/releases.json";
        var data = await BaseClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<DragonwellObj>(str);
    }
}
