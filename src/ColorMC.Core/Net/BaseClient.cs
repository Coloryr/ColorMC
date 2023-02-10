using ColorMC.Core.Utils;
using System.Net;

namespace ColorMC.Core.Net;

public enum SourceLocal
{
    Offical = 0,
    BMCLAPI = 1,
    MCBBS = 2
}

public static class BaseClient
{
    public static SourceLocal Source { get; set; }

    public static HttpClient DownloadClient { get; private set; }
    public static HttpClient LoginClient { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        Logs.Info(LanguageHelper.GetName("Core.Http.Info5"));
        if (ConfigUtils.Config.Http.DownloadProxy ||
            ConfigUtils.Config.Http.GameProxy ||
            ConfigUtils.Config.Http.LoginProxy)
        {
            Logs.Info(string.Format(LanguageHelper.GetName("Core.Http.Info6"),
               ConfigUtils.Config.Http.ProxyIP, ConfigUtils.Config.Http.ProxyPort));
        }

        if (ConfigUtils.Config.Http.DownloadProxy)
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

        if (ConfigUtils.Config.Http.LoginProxy)
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

        DownloadClient.Timeout = TimeSpan.FromSeconds(10);
        LoginClient.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// GET 获取字符串
    /// </summary>
    /// <param name="url">地址</param>
    /// <returns></returns>
    public static async Task<string> GetString(string url)
    {
        return await DownloadClient.GetStringAsync(url);
    }

    /// <summary>
    /// GET 获取二进制
    /// </summary>
    /// <param name="url">地址</param>
    /// <returns></returns>
    public static async Task<byte[]> GetBytes(string url)
    {
        return await DownloadClient.GetByteArrayAsync(url);
    }
}
