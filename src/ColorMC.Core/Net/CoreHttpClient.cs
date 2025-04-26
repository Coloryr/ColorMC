using System.Net;
using System.Net.Http.Headers;
using Ae.Dns.Client;
using Ae.Dns.Protocol;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Net;

/// <summary>
/// 网络客户端
/// </summary>
public static class CoreHttpClient
{
    /// <summary>
    /// 下载源
    /// </summary>
    public static SourceLocal Source { get; set; }

    /// <summary>
    /// 下载用http客户端
    /// </summary>
    public static HttpClient DownloadClient { get; private set; }
    /// <summary>
    /// 登录用http客户端
    /// </summary>
    public static HttpClient LoginClient { get; private set; }
    /// <summary>
    /// Dns列表
    /// </summary>
    private readonly static List<IDnsClient> _dnsClients = [];

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        var http = ConfigUtils.Config.Http;
        var dns = ConfigUtils.Config.Dns;

        Logs.Info(LanguageHelper.Get("Core.Http.Info5"));
        if (http.DownloadProxy || http.GameProxy || http.LoginProxy)
        {
            Logs.Info(string.Format(LanguageHelper.Get("Core.Http.Info6"),
               http.ProxyIP, http.ProxyPort));
        }

        Source = http.Source;

        DownloadClient?.CancelPendingRequests();
        DownloadClient?.Dispose();

        LoginClient?.CancelPendingRequests();
        LoginClient?.Dispose();

        foreach (var item in _dnsClients)
        {
            item.Dispose();
        }

        _dnsClients.Clear();

        IDnsClient? dnsClient = null;
        WebProxy? proxy = null;

        //代理
        if (!string.IsNullOrWhiteSpace(http.ProxyIP))
        {
            proxy = new WebProxy(http.ProxyIP, http.ProxyPort);
        }
        //Dns
        if (dns.Enable)
        {
            //有GC问题
            //if (dns.DnsType is DnsType.DnsOver or DnsType.DnsOverHttpsWithUdp)
            //{
            //foreach (var item in dns.Dns)
            //{
            //    _dnsClients.Add(new DnsUdpClient(IPAddress.Parse(item)));
            //}
            //}
            if (dns.DnsType is DnsType.DnsOverHttps or DnsType.DnsOverHttpsWithUdp)
            {
                foreach (var item in dns.Https)
                {
                    if (dns.HttpProxy)
                    {
                        _dnsClients.Add(new SelfHttpDnsClient(item, proxy));
                    }
                    else
                    {
                        _dnsClients.Add(new SelfHttpDnsClient(item));
                    }
                }
            }
            if (_dnsClients.Count > 1)
            {
                dnsClient = new DnsRacerClient([.. _dnsClients]);
                _dnsClients.Add(dnsClient);
            }
            else if (_dnsClients.Count > 0)
            {
                dnsClient = _dnsClients[0];
            }
        }

        if (dnsClient != null)
        {
            DownloadClient = new(new DnsDelegatingHandler(dnsClient)
            {
                InnerHandler = new SocketsHttpHandler()
                {
                    Proxy = http.DownloadProxy ? proxy : null
                }
            });
            LoginClient = new(new DnsDelegatingHandler(dnsClient)
            {
                InnerHandler = new SocketsHttpHandler()
                {
                    Proxy = http.LoginProxy ? proxy : null
                }
            });
        }
        else
        {
            DownloadClient = new(new HttpClientHandler()
            {
                Proxy = http.DownloadProxy ? proxy : null
            });
            LoginClient = new(new HttpClientHandler()
            {
                Proxy = http.LoginProxy ? proxy : null
            });
        }

        DownloadClient.DefaultRequestVersion = HttpVersion.Version11;
        DownloadClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        DownloadClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        DownloadClient.DefaultRequestHeaders.UserAgent.Clear();
        DownloadClient.DefaultRequestHeaders.UserAgent
            .Add(new ProductInfoHeaderValue("ColorMC", ColorMCCore.Version));
        DownloadClient.Timeout = TimeSpan.FromSeconds(20);

        LoginClient.DefaultRequestVersion = HttpVersion.Version11;
        LoginClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        LoginClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        LoginClient.DefaultRequestHeaders.UserAgent.Clear();
        LoginClient.DefaultRequestHeaders.UserAgent
            .Add(new ProductInfoHeaderValue("ColorMC", ColorMCCore.Version));
        LoginClient.Timeout = TimeSpan.FromSeconds(20);
    }

    /// <summary>
    /// GET 获取字符串
    /// </summary>
    /// <param name="url">地址</param>
    /// <returns></returns>
    public static async Task<MessageRes> GetStringAsync(string url)
    {
        var data = await DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            return new();
        }

        var data1 = await data.Content.ReadAsStringAsync();
        return new() { State = true, Message = data1 };
    }

    /// <summary>
    /// GET 获取二进制
    /// </summary>
    /// <param name="url">地址</param>
    /// <returns></returns>
    public static async Task<BytesRes> GetBytesAsync(string url)
    {
        var data = await DownloadClient.GetAsync(url);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            return new();
        }

        var data1 = await data.Content.ReadAsByteArrayAsync();
        return new() { State = true, Data = data1 };
    }

    /// <summary>
    /// GET 获取二进制
    /// </summary>
    /// <param name="url">地址</param>
    /// <returns></returns>
    public static async Task<StreamRes> GetStreamAsync(string url)
    {
        var data = await DownloadClient.GetAsync(url);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            return new();
        }

        var data1 = await data.Content.ReadAsStreamAsync();
        return new() { State = true, Stream = data1 };
    }

    /// <summary>
    /// 请求数据
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="arg">参数</param>
    /// <returns>数据</returns>
    public static async Task<string> LoginPostStringAsync(string url, Dictionary<string, string> arg)
    {
        FormUrlEncodedContent content = new(arg);
        using var message = await LoginClient.PostAsync(url, content);

        return await message.Content.ReadAsStringAsync();
    }
    /// <summary>
    /// 请求数据
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="arg">参数</param>
    /// <returns>数据</returns>
    public static async Task<JObject?> LoginPostJsonAsync(string url, object arg)
    {
        var data1 = JsonConvert.SerializeObject(arg);
        var content = new StringContent(data1, MediaTypeHeaderValue.Parse("application/json"));
        using var message = await LoginClient.PostAsync(url, content);
        var data = await message.Content.ReadAsStringAsync();
        return JObject.Parse(data);
    }

    /// <summary>
    /// 请求数据
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="arg">参数</param>
    /// <returns>数据</returns>
    public static async Task<JObject?> LoginPostAsync(string url, Dictionary<string, string> arg)
    {
        var content = new FormUrlEncodedContent(arg);
        using var message = await LoginClient.PostAsync(url, content);
        var data = await message.Content.ReadAsStringAsync();
        return JObject.Parse(data);
    }
}
