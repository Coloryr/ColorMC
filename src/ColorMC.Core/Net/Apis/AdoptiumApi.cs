using ColorMC.Core.Objs.Java;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Adoptium下载源
/// </summary>
public static class AdoptiumApi
{
    public const string AdoptiumUrl = "https://api.adoptium.net/";

    public static readonly List<string> SystemType =
    [
        "", "Windows", "Linux", "Alpine Linux", "MacOS", "AIX", "Solaris"
    ];

    private static List<string> _javaVersion;

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

    public static async Task<List<string>?> GetJavaVersion()
    {
        if (_javaVersion != null)
        {
            return _javaVersion;
        }
        string url = $"{AdoptiumUrl}v3/info/available_releases";
        var data = await BaseClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();
        var obj = JsonConvert.DeserializeObject<AdoptiumJavaVersionObj>(str);
        if (obj == null || obj.available_releases == null)
            return null;
        var list = new List<string>();
        obj.available_releases.ForEach(item =>
        {
            list.Add(item.ToString());
        });
        _javaVersion = list;
        return list;
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
            url = $"{AdoptiumUrl}v3/assets/latest/{version}/hotspot";
        }
        else
        {
            url = $"{AdoptiumUrl}v3/assets/latest/{version}/hotspot?os={GetOs(os)}";
        }
        var data = await BaseClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<AdoptiumObj>>(str);
    }
}
