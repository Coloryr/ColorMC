using ColorMC.Core.Objs.Java;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Zulu下载源
/// </summary>
public static class ZuluApi
{
    public const string Url = "https://www.azul.com/wp-admin/admin-ajax.php?action=bundles&endpoint=community&use_stage=false&include_fields=java_version%2Copenjdk_build_number%2Crelease_status%2Csupport_term%2Cos%2Carch%2Chw_bitness%2Cabi%2Cbundle_type%2Cjavafx%2Clatest%2Cext%2Cname%2Csha256_hash%2Curl%2Ccpu_gen%2Cfeatures";

    /// <summary>
    /// 获取列表
    /// </summary>
    public static async Task<List<ZuluObj>?> GetJavaList()
    {
        var data = await CoreHttpClient._downloadClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();
        if (str.StartsWith('<'))
        {
            ColorMCCore.OnError(str, null, false);
            return null;
        }
        return JsonConvert.DeserializeObject<List<ZuluObj>>(str);
    }
}
