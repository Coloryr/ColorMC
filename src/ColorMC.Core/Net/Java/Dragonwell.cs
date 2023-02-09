using ColorMC.Core.Objs.Java;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Net.Java;

public static class Dragonwell
{
    public static async Task<DragonwellObj?> GetJavaList()
    {
        var url = "https://dragonwell-jdk.io/releases.json";
        var data = await BaseClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<DragonwellObj>(str);
    }
}
