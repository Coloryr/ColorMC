using ColorMC.Core.Objs.Java;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Java;

public static class Adoptium
{
    public readonly static List<string> JavaVersion = new()
    {
        "8", "11", "16", "17", "18", "19", "20"
    };

    public readonly static List<string> SystemType = new()
    {
        "", "Windows", "Linux", "Alpine Linux", "MacOS", "AIX", "Solaris"
    };

    /// <summary>
    /// 获取系统类型
    /// </summary>
    /// <param name="type">类型</param>
    public static string GetOs(int type)
    {
        return type switch
        {
            1 => "windows",
            2 => "linux",
            3 => "alpine-linux",
            4 => "mac",
            5 => "aix",
            6 => "solaris",
            _ => ""
        };
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="version">版本</param>
    /// <param name="os">系统</param>
    /// <returns></returns>
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
