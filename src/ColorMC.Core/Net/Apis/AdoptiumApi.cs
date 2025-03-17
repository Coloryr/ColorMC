using ColorMC.Core.Objs.Java;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Adoptium下载源
/// </summary>
public static class AdoptiumApi
{
    /// <summary>
    /// 下载地址
    /// </summary>
    public const string AdoptiumUrl = "https://api.adoptium.net/";

    /// <summary>
    /// 系统类型
    /// </summary>
    public static readonly List<string> SystemType =
    [
        "", "Windows", "Linux", "Alpine Linux", "MacOS", "AIX", "Solaris"
    ];

    /// <summary>
    /// Java版本
    /// </summary>
    private static List<string> s_javaVersion;

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
    /// 获取Java版本
    /// </summary>
    /// <returns></returns>
    public static async Task<List<string>?> GetJavaVersion()
    {
        if (s_javaVersion != null)
        {
            return s_javaVersion;
        }
        string url = $"{AdoptiumUrl}v3/info/available_releases";
        var data = await CoreHttpClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();
        var obj = JsonConvert.DeserializeObject<AdoptiumJavaVersionObj>(str);
        if (obj == null || obj.AvailableReleases == null)
            return null;
        var list = new List<string>();
        obj.AvailableReleases.ForEach(item =>
        {
            list.Add(item.ToString());
        });
        s_javaVersion = list;
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
        var data = await CoreHttpClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
            return null;
        var str = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<AdoptiumObj>>(str);
    }
}
