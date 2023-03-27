using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Collections.Concurrent;
using System.Net;

namespace ColorMC.Core.Net;

public static class BaseClient
{
    public static SourceLocal Source { get; private set; }

    public static HttpClient DownloadClient { get; private set; }
    public static HttpClient LoginClient { get; private set; }

    private static readonly Thread[] threads = new Thread[5];
    private static readonly ConcurrentBag<(string, CancellationToken, Action<bool, Stream?>)> tasks = new();
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

    private static void Run()
    {
        while (run)
        {
            while (tasks.TryTake(out var item))
            {
                try
                {
                    if (item.Item2.IsCancellationRequested)
                        item.Item3(false, null);

                    using var data1 = DownloadClient.GetAsync(item.Item1, item.Item2).Result;
                    if (item.Item2.IsCancellationRequested)
                        item.Item3(false, null);

                    using var data2 = data1.Content.ReadAsStream(item.Item2);
                    if (item.Item2.IsCancellationRequested)
                        item.Item3(false, null);

                    item.Item3(data1.IsSuccessStatusCode, data2);
                }
                catch (AggregateException e)
                {
                    if (item.Item2.IsCancellationRequested)
                        item.Item3(false, null);

                    Logs.Error("http error", e);
                }
                catch (Exception e)
                {
                    if (item.Item2.IsCancellationRequested)
                        item.Item3(false, null);

                    Logs.Error("http error", e);
                }
            }
            Thread.Sleep(1000);
        }
    }

    public static void Poll(string url, Action<bool, Stream?> action, CancellationToken token)
    {
        tasks.Add((url, token, action));
    }
}
