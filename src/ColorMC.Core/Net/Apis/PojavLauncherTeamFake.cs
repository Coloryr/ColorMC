using ColorMC.Core.Objs.Java;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// PojavLauncherTeam下载源
/// </summary>
public static class PojavLauncherTeamFake
{
    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="version">版本</param>
    /// <param name="os">系统</param>
    /// <returns></returns>
    public static async Task<PojavLauncherTeamObj?> GetJavaList()
    {
        string url = ColorMCAPI.BaseUrl + "update/java.json";

        var data = await BaseClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<PojavLauncherTeamObj>(str);
    }
}
