using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ae.Dns.Client;
using Ae.Dns.Protocol;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

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
    private static HttpClient _downloadClient;
    /// <summary>
    /// 登录用http客户端
    /// </summary>
    private static HttpClient _loginClient;
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

        _downloadClient?.CancelPendingRequests();
        _downloadClient?.Dispose();

        _loginClient?.CancelPendingRequests();
        _loginClient?.Dispose();

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
            _downloadClient = new(new DnsDelegatingHandler(dnsClient)
            {
                InnerHandler = new SocketsHttpHandler()
                {
                    Proxy = http.DownloadProxy ? proxy : null
                }
            });
            _loginClient = new(new DnsDelegatingHandler(dnsClient)
            {
                InnerHandler = new SocketsHttpHandler()
                {
                    Proxy = http.LoginProxy ? proxy : null
                }
            });
        }
        else
        {
            _downloadClient = new(new HttpClientHandler()
            {
                Proxy = http.DownloadProxy ? proxy : null
            });
            _loginClient = new(new HttpClientHandler()
            {
                Proxy = http.LoginProxy ? proxy : null
            });
        }

        _downloadClient.DefaultRequestVersion = HttpVersion.Version11;
        _downloadClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        _downloadClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        _downloadClient.DefaultRequestHeaders.UserAgent.Clear();
        _downloadClient.DefaultRequestHeaders.UserAgent
            .Add(new ProductInfoHeaderValue("ColorMC", ColorMCCore.Version));
        _downloadClient.Timeout = TimeSpan.FromSeconds(20);

        _loginClient.DefaultRequestVersion = HttpVersion.Version11;
        _loginClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        _loginClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        _loginClient.DefaultRequestHeaders.UserAgent.Clear();
        _loginClient.DefaultRequestHeaders.UserAgent
            .Add(new ProductInfoHeaderValue("ColorMC", ColorMCCore.Version));
        _loginClient.Timeout = TimeSpan.FromSeconds(20);
    }

    /// <summary>
    /// 进行一次Get请求
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> GetAsync(string url)
    { 
        return _downloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    }

    public static Task<HttpResponseMessage> GetAsync(Uri url)
    {
        return _downloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    }

    public static Task<HttpResponseMessage> GetAsync(string url, CancellationToken token)
    {
        return _downloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
    }

    public static async Task<Stream?> GetStreamAsync(string url)
    {
        var res = await _downloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (res.StatusCode != HttpStatusCode.OK)
        {
            res.Dispose();
            return null;
        }
        return await res.Content.ReadAsStreamAsync();
    }

    public static Task<HttpResponseMessage> LoginGetAsync(string url)
    {
        return _loginClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    }

    /// <summary>
    /// GET 获取字符串
    /// </summary>
    /// <param name="url">地址</param>
    /// <returns></returns>
    public static async Task<MessageRes> GetStringAsync(string url)
    {
        using var data = await _downloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
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
        using var data = await _downloadClient.GetAsync(url);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            return new();
        }

        var data1 = await data.Content.ReadAsByteArrayAsync();
        return new() { State = true, Data = data1 };
    }

    /// <summary>
    /// 请求数据
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="arg">参数</param>
    /// <returns>数据</returns>
    public static async Task<Stream> LoginPostStreamAsync(string url, Dictionary<string, string> arg)
    {
        var content = new FormUrlEncodedContent(arg);
        var message = await _loginClient.PostAsync(url, content);
        return await message.Content.ReadAsStreamAsync();
    }
    /// <summary>
    /// 请求数据
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="arg">参数</param>
    /// <returns>数据</returns>
    public static async Task<JsonDocument?> LoginPostJsonAsync(string url, string arg)
    {
        var content = new StringContent(arg, MediaTypeHeaderValue.Parse("application/json"));
        using var message = await _loginClient.PostAsync(url, content);
        using var data = await message.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(data);
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest)
    {
        return _downloadClient.SendAsync(httpRequest);
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> SendLoginAsync(HttpRequestMessage httpRequest)
    {
        return _loginClient.SendAsync(httpRequest);
    }
}
