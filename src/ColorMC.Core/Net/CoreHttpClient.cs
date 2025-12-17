using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json;
using Ae.Dns.Client;
using Ae.Dns.Protocol;
using ColorMC.Core.Config;
using ColorMC.Core.Objs;

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
        var http = ConfigLoad.Config.Http;
        var dns = ConfigLoad.Config.Dns;

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
        if (dns?.Https != null && dns.Enable)
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
            if (_dnsClients.Count > 1)
            {
                dnsClient = new DnsRacerClient([.. _dnsClients]);
                _dnsClients.Add(dnsClient);
            }
            else if (_dnsClients.Count == 1)
            {
                dnsClient = _dnsClients[0];
            }
        }

        if (dnsClient != null)
        {
            _downloadClient = new(new DnsDelegatingHandler(dnsClient)
            {
                InnerHandler = new HttpClientHandler()
                {
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    Proxy = http.DownloadProxy ? proxy : null
                }
            });
            _loginClient = new(new DnsDelegatingHandler(dnsClient)
            {
                InnerHandler = new HttpClientHandler()
                {
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    Proxy = http.LoginProxy ? proxy : null
                }
            });
        }
        else
        {
            _downloadClient = new(new HttpClientHandler()
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                Proxy = http.DownloadProxy ? proxy : null
            });
            _loginClient = new(new HttpClientHandler()
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
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

    /// <summary>
    /// 进行一次Get请求
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> GetAsync(Uri url)
    {
        return _downloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    }

    /// <summary>
    /// 进行一次Get请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> GetAsync(string url, CancellationToken token)
    {
        return _downloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
    }

    /// <summary>
    /// 进行一次GET请求<br/>
    /// 指定ranges
    /// </summary>
    /// <param name="url"></param>
    /// <param name="pos"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> GetRangesAsync(string url, long pos, CancellationToken token)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Range = new RangeHeaderValue(pos, null);
        return _downloadClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);
    }

    /// <summary>
    /// 进行一次Get请求
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static Task<Stream?> GetStreamAsync(string url)
    {
        return GetStreamAsync(url, CancellationToken.None);
    }

    /// <summary>
    /// 进行一次Get请求
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task<Stream?> GetStreamAsync(string url, CancellationToken token)
    {
        var res = await _downloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
        if (res.StatusCode != HttpStatusCode.OK)
        {
            res.Dispose();
            return null;
        }
        return await res.Content.ReadAsStreamAsync(token);
    }

    /// <summary>
    /// 使用登录Client进行一次Get请求
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> LoginGetAsync(string url)
    {
        return _loginClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    }

    /// <summary>
    /// GET 获取字符串
    /// </summary>
    /// <param name="url">地址</param>
    /// <returns></returns>
    public static async Task<string?> GetStringAsync(string url, CancellationToken token = default)
    {
        using var data = await _downloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await data.Content.ReadAsStringAsync(token);
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
    public static async Task<Stream> LoginPostStreamAsync(string url, Dictionary<string, string> arg, CancellationToken token)
    {
        var content = new FormUrlEncodedContent(arg);
        var message = await _loginClient.PostAsync(url, content, token);
        return await message.Content.ReadAsStreamAsync(token);
    }
    /// <summary>
    /// 请求数据
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="arg">参数</param>
    /// <returns>数据</returns>
    public static async Task<JsonDocument?> LoginPostJsonAsync(string url, string arg, CancellationToken token)
    {
        var content = new StringContent(arg, MediaTypeHeaderValue.Parse("application/json"));
        using var message = await _loginClient.PostAsync(url, content, token);
        using var data = await message.Content.ReadAsStreamAsync(token);
        return await JsonDocument.ParseAsync(data, cancellationToken: token);
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken token)
    {
        return _downloadClient.SendAsync(req, token);
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> SendLoginAsync(HttpRequestMessage req, CancellationToken token)
    {
        return _loginClient.SendAsync(req, token);
    }
}
