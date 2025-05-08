using System.Text.Json.Nodes;
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
    public const string BaseUrl = $"https://mc1.coloryr.com:8081/";
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

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="ids">数据</param>
    /// <returns></returns>
    private static async Task<Dictionary<string, McModSearchItemObj>?> GetList(int type, List<string> ids)
    {
        try
        {
            string temp = $"{BaseUrl}findmod";
            var obj = new McModSearchObj()
            {
                Type = type,
                Ids = ids
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
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error1"), e);
            return null;
        }
    }

    /// <summary>
    /// 从CF的modid获取mcmod数据
    /// </summary>
    /// <param name="ids">id列表</param>
    /// <returns>数据</returns>
    public static Task<Dictionary<string, McModSearchItemObj>?> GetMcModFromCF(List<string> ids)
    {
        return GetList(0, ids);
    }

    /// <summary>
    /// 从Mo的modid获取mcmod数据
    /// </summary>
    /// <param name="ids">id列表</param>
    /// <returns>数据</returns>
    public static Task<Dictionary<string, McModSearchItemObj>?> GetMcModFromMO(List<string> ids)
    {
        return GetList(1, ids);
    }

    /// <summary>
    /// 从名字获取mcmod数据
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="page">页数</param>
    /// <returns>数据</returns>
    public static Task<Dictionary<string, McModSearchItemObj>?> GetMcMod(string name, int page, Loaders loader, string version, string modtype, int sort)
    {
        return GetList(2, [name, page.ToString(), modtype, version, ((int)loader).ToString(), sort.ToString()]);
    }

    /// <summary>
    /// 获取McMod分组列表
    /// </summary>
    /// <returns></returns>
    public static async Task<McModTypsObj?> GetMcModGroup()
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
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error2"), e);
            return null;
        }
    }
#if Phone
    /// <summary>
    /// 获取列表
    /// </summary>
    /// <returns></returns>
    public static async Task<AndroidJavaObj?> GetJavaList()
    {
        string url = BaseUrl + "update/java.json";
        var str = await CoreHttpClient.DownloadClient.GetStringAsync(url);

        return JsonConvert.DeserializeObject<AndroidJavaObj>(str);
    }
#endif
}
