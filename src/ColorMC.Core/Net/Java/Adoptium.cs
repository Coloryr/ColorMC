using ColorMC.Core.Objs.Java;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Net.Java;

public static class Adoptium
{
    public readonly static List<string> JavaVersion = new()
    {
        "8", "11", "16", "17", "18", "19"
    };

    public static string GetOs(int type)
    {
        return type switch
        {
            1 => "windows",
            2 => "linux",
            3 => "mac",
            _ => ""
        };
    }

    public static async Task<List<AdoptiumObj>?> GetJavaList(string version, int os)
    {
        string url;
        if (os == 0)
        {
            url = $"https://api.adoptium.net/v3/assets/latest/{version}/hotspot";
        }
        else
        {
            url = $"https://api.adoptium.net/v3/assets/latest/{version}/hotspot?os={GetOs(os)}";
        }
        var data = await BaseClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<AdoptiumObj>>(str);
    }
}
