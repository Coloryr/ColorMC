using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Net;

namespace ColorMC.Core.Net;

/// <summary>
/// 网络客户端
/// </summary>
public static class BaseClient
{
    /// <summary>
    /// 下载源
    /// </summary>
    public static SourceLocal Source { get; private set; }

    public static HttpClient DownloadClient { get; private set; }
    public static HttpClient LoginClient { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        Logs.Info(LanguageHelper.Get("Core.Http.Info5"));
        if (ConfigUtils.Config.Http.DownloadProxy ||
            ConfigUtils.Config.Http.GameProxy ||
            ConfigUtils.Config.Http.LoginProxy)
        {
            Logs.Info(string.Format(LanguageHelper.Get("Core.Http.Info6"),
               ConfigUtils.Config.Http.ProxyIP, ConfigUtils.Config.Http.ProxyPort));
        }

        Source = ConfigUtils.Config.Http.Source;

        DownloadClient?.CancelPendingRequests();
        DownloadClient?.Dispose();

        LoginClient?.CancelPendingRequests();
        LoginClient?.Dispose();

        //代理
        if (ConfigUtils.Config.Http.DownloadProxy
            && !string.IsNullOrWhiteSpace(ConfigUtils.Config.Http.ProxyIP))
        {
            DownloadClient = new(new HttpClientHandler()
            {
                Proxy = new WebProxy(ConfigUtils.Config.Http.ProxyIP, ConfigUtils.Config.Http.ProxyPort)
            });
        }
        else
        {
            DownloadClient = new();
        }

        if (ConfigUtils.Config.Http.LoginProxy
             && !string.IsNullOrWhiteSpace(ConfigUtils.Config.Http.ProxyIP))
        {
            LoginClient = new(new HttpClientHandler()
            {
                Proxy = new WebProxy(ConfigUtils.Config.Http.ProxyIP, ConfigUtils.Config.Http.ProxyPort)
            });
        }
        else
        {
            LoginClient = new();
        }

        LoginClient.Timeout = TimeSpan.FromSeconds(10);
        DownloadClient.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// GET 获取字符串
    /// </summary>
    /// <param name="url">地址</param>
    /// <returns></returns>
    public static async Task<(bool, string?)> GetString(string url)
    {
        var data = await DownloadClient.GetAsync(url);
        if (data.StatusCode == HttpStatusCode.NotFound)
        {
            return (false, null);
        }

        var data1 = await data.Content.ReadAsStringAsync();
        return (true, data1);
    }

    /// <summary>
    /// GET 获取二进制
    /// </summary>
    /// <param name="url">地址</param>
    /// <returns></returns>
    public static async Task<(bool, byte[]?)> GetBytes(string url)
    {
        var data = await DownloadClient.GetAsync(url);
        if (data.StatusCode == HttpStatusCode.NotFound)
        {
            return (false, null);
        }

        var data1 = await data.Content.ReadAsByteArrayAsync();
        return (true, data1);
    }
}
