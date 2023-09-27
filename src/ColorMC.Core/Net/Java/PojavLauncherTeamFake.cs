using ColorMC.Core.Objs.Java;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Net.Java;

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
        string url = "https://kmy-ap-northeast-1.public-storage.kamiya-external.net/PojavLauncherTeam/android-openjdk-build-multiarch/meta.json";

        var data = await BaseClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<PojavLauncherTeamObj>(str);
    }
}
