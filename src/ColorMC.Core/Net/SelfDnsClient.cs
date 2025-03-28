﻿using System.Net;
using Ae.Dns.Client;
using Ae.Dns.Protocol;

namespace ColorMC.Core.Net;

/// <summary>
/// DNS over https客户端
/// </summary>
public class SelfHttpDnsClient : IDnsClient
{
    private readonly DnsHttpClient _client;
    private readonly HttpClient _http;

    public SelfHttpDnsClient(string url)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(url)
        };

        _client = new DnsHttpClient(_http);
    }

    public SelfHttpDnsClient(string url, IWebProxy? proxy)
    {
        _http = new HttpClient(new HttpClientHandler()
        {
            Proxy = proxy
        })
        {
            BaseAddress = new Uri(url)
        };

        _client = new DnsHttpClient(_http);
    }

    public Task<DnsMessage> Query(DnsMessage query, CancellationToken token = default)
    {
        return _client.Query(query, token);
    }

    public void Dispose()
    {
        _client.Dispose();
        _http.Dispose();
    }
}