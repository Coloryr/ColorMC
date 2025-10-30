using System.Net;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ColorMC;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// ColorMC网络API
/// </summary>
public static class ColorMCAPI
{
    //public const string BaseUrl = $"http://localhost:80/";
    public const string BaseUrl = $"https://api.coloryr.com:8081/";
    public const string BaseWebUrl = $"https://www.coloryr.com/";

    private static readonly HttpClient _client = new()
    {
        Timeout = Timeout.InfiniteTimeSpan
    };

    /// <summary>
    /// 发送http请求
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
    {
        return _client.SendAsync(message);
    }

    public static async Task<Stream?> GetStreamAsync(string url)
    {
        var res = await _client.GetAsync(url);
        if (res.StatusCode != HttpStatusCode.OK)
        {
            res.Dispose();
            return null;
        }
        return await res.Content.ReadAsStreamAsync();
    }

    public static Task<string> GetStringAsync(string url)
    {
        return _client.GetStringAsync(url);
    }

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="ids">数据</param>
    /// <returns></returns>
    private static async Task<Dictionary<string, McModSearchItemObj>?> GetListAsync(int type, List<string> ids, int mcmod_type)
    {
        try
        {
            string temp = $"{BaseUrl}findmod";
            var obj = new McModSearchObj()
            {
                Type = type,
                Ids = ids,
                McmodType = mcmod_type
            };
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(temp),
                Content = new StringContent(JsonUtils.ToString(obj, JsonType.McModSearchObj))
            };

            using var data = await _client.SendAsync(httpRequest);
            using var data1 = await data.Content.ReadAsStreamAsync();
            var obj1 = JsonUtils.ToObj(data1, JsonType.McModSearchResObj);
            if (obj1 == null || obj1.Res != 100)
            {
                return null;
            }

            return obj1.Data;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Error44"), e);
            return null;
        }
    }

    /// <summary>
    /// 从CF的modid获取mcmod数据
    /// </summary>
    /// <param name="ids">id列表</param>
    /// <returns>数据</returns>
    public static Task<Dictionary<string, McModSearchItemObj>?> GetMcModFromCFAsync(List<string> ids, int mcmod_type)
    {
        return GetListAsync(0, ids, mcmod_type);
    }

    /// <summary>
    /// 从Mo的modid获取mcmod数据
    /// </summary>
    /// <param name="ids">id列表</param>
    /// <returns>数据</returns>
    public static Task<Dictionary<string, McModSearchItemObj>?> GetMcModFromMOAsync(List<string> ids, int mcmod_type)
    {
        return GetListAsync(1, ids, mcmod_type);
    }

    /// <summary>
    /// 从名字获取mcmod数据
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="page">页数</param>
    /// <returns>数据</returns>
    public static Task<Dictionary<string, McModSearchItemObj>?> GetMcMod(string name, int page, Loaders loader, string version, string modtype, int sort)
    {
        return GetListAsync(2, [name, page.ToString(), modtype, version, ((int)loader).ToString(), sort.ToString()], 0);
    }

    /// <summary>
    /// 获取McMod分组列表
    /// </summary>
    /// <returns></returns>
    public static async Task<McModTypsObj?> GetMcModGroupAsync()
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "getmcmodgroup");
            using var data = await _client.SendAsync(req);
            using var data1 = await data.Content.ReadAsStreamAsync();
            var obj = JsonUtils.ToObj(data1, JsonType.McModTypsResObj);
            if (obj == null || obj.Res != 100)
            {
                return null;
            }

            return obj.Data;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Error45"), e);
            return null;
        }
    }
}
