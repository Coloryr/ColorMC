using ColorMC.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;

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

    public static HttpClient DownloadClient;
    public static HttpClient LoginClient;

    public static void Init()
    {
        Logs.Info(LanguageHelper.GetName("Core.Http.Init"));
        if (ConfigUtils.Config.Http.DownloadProxy ||
            ConfigUtils.Config.Http.GameProxy ||
            ConfigUtils.Config.Http.LoginProxy)
        {
            Logs.Info(string.Format(LanguageHelper.GetName("Core.Http.Proxy"),
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

    public static async Task<string> GetString(string url, Dictionary<string, string>? arg = null)
    {
        if (arg == null)
        {
            return await DownloadClient.GetStringAsync(url);
        }
        else
        {
            string temp = url;
            foreach (var item in arg)
            {
                temp += $"{item.Key}={item.Value}&";
            }
            temp = temp[..^1];
            return await DownloadClient.GetStringAsync(temp);
        }
    }

    public static async Task<byte[]> GetBytes(string url, Dictionary<string, string>? arg = null)
    {
        if (arg == null)
        {
            return await DownloadClient.GetByteArrayAsync(url);
        }
        else
        {
            string temp = url;
            foreach (var item in arg)
            {
                temp += $"{item.Key}={item.Value}&";
            }
            temp = temp[..^1];
            return await DownloadClient.GetByteArrayAsync(temp);
        }
    }
}
