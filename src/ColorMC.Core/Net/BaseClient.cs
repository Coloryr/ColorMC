using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Collections.Concurrent;
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

    private static readonly Thread[] threads = new Thread[5];
    private static readonly ConcurrentBag<(string, Action<bool, byte[]?>)> tasks = new();
    private static bool run;

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

        Source = ConfigUtils.Config.Http.Source;

        for (int a = 0; a < 5; a++)
        {
            run = true;
            threads[a] = new(Run)
            {
                Name = $"ColorMC-Http_{a}"
            };
            threads[a].Start();
        }

        DownloadClient?.CancelPendingRequests();
        DownloadClient?.Dispose();

        LoginClient?.CancelPendingRequests();
        LoginClient?.Dispose();

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

        LoginClient.Timeout = TimeSpan.FromSeconds(10);
        DownloadClient.Timeout = TimeSpan.FromSeconds(10);

        ColorMCCore.Stop += ColorMCCore_Stop;
    }

    private static void ColorMCCore_Stop()
    {
        run = false;
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
    public static async Task<byte[]> GetBytes(string url)
    {
        return await DownloadClient.GetByteArrayAsync(url);
    }

    private static async void Run()
    {
        while (run)
        {
            while (tasks.TryTake(out var item))
            {
                try
                {
                    var data1 = await DownloadClient.GetAsync(item.Item1);
                    var data2 = await data1.Content.ReadAsByteArrayAsync();

                    item.Item2(data1.IsSuccessStatusCode, data2);

                    data1.Content.Dispose();
                    data1.Dispose();
                }
                catch (Exception e)
                {
                    item.Item2(false, null);
                    Logs.Error(LanguageHelper.GetName("Core.Http.Error9"), e);
                }
            }
            Thread.Sleep(1000);
        }
    }

    public static void Poll(string url, Action<bool, byte[]?> action)
    {
        tasks.Add((url, action));
    }
}
