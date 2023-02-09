using ColorMC.Core.Objs.Java;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Net.Java;

public static class Zulu
{
    public static string GetOs()
    {
        return SystemInfo.Os switch
        {
            OsType.Windows => "windows",
            OsType.Linux => "linux",
            OsType.MacOS => "mac",
            _ => ""
        };
    }

    public static async Task<List<ZuluObj>?> GetJavaList()
    {
        var url = "https://www.azul.com/wp-admin/admin-ajax.php?action=bundles&endpoint=community&use_stage=false&include_fields=java_version%2Copenjdk_build_number%2Crelease_status%2Csupport_term%2Cos%2Carch%2Chw_bitness%2Cabi%2Cbundle_type%2Cjavafx%2Clatest%2Cext%2Cname%2Csha256_hash%2Curl%2Ccpu_gen%2Cfeatures";
        var data = await BaseClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<ZuluObj>>(str);
    }
}
