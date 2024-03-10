using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Java;
using ColorMC.Core.Objs.McMod;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// ColorMC网络API
/// </summary>
public static class ColorMCAPI
{
    //public const string BaseUrl = $"http://localhost:8080/";
    public const string BaseUrl = $"https://mc1.coloryr.com:8081/";
    public const string CheckUrl = $"{BaseUrl}update/{ColorMCCore.TopVersion}/";

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
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(temp),
            };
            httpRequest.Headers.Add("ColorMC", ColorMCCore.Version);
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(new { type, ids }));

            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
                return null;
            var obj = JObject.Parse(data1);
            if (obj.TryGetValue("res", out var res) && ((int)res) != 100)
            {
                return null;
            }

            var data2 = obj.GetValue("data")!;
            return data2.ToObject<Dictionary<string, McModSearchItemObj>>();
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
    public static Task<Dictionary<string, McModSearchItemObj>?> GetMcModFromName(string name, int page)
    {
        return GetList(2, [name, page.ToString()]);
    }

    public static async Task<string?> GetNewLog()
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "update/log");
            req.Headers.Add("ColorMC", ColorMCCore.Version);
            var data = await BaseClient.DownloadClient.SendAsync(req);
            return await data.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error2"), e);
            return null;
        }
    }

    public static async Task<JObject> GetUpdateIndex()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "update/index.json");
        req.Headers.Add("ColorMC", ColorMCCore.Version);
        var data = await BaseClient.DownloadClient.SendAsync(req);
        return JObject.Parse(await data.Content.ReadAsStringAsync());
    }

    public static async Task<JObject> GetUpdateSha1()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, CheckUrl + "sha1.json");
        req.Headers.Add("ColorMC", ColorMCCore.Version);
        var data = await BaseClient.DownloadClient.SendAsync(req);
        string text = await data.Content.ReadAsStringAsync();
        return JObject.Parse(text);
    }

    public static async Task<JObject?> GetCloudServer()
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "frplist");
            req.Headers.Add("ColorMC", ColorMCCore.Version);
            var data = await BaseClient.DownloadClient.SendAsync(req);
            return JObject.Parse(await data.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error4"), e);
            return null;
        }
    }

    public static async Task<bool> PutCloudServer(string token, string ip)
    {
        HttpRequestMessage httpRequest = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(BaseUrl + "frp"),
        };
        httpRequest.Headers.Add("ColorMC", ColorMCCore.Version);
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(new { token, ip }));

        try
        {
            var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
            {
                return false;
            }
            var obj = JObject.Parse(data1);
            if (obj.TryGetValue("res", out var res) && ((int)res) != 100)
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error3"), e);
            return false;
        }
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="version">版本</param>
    /// <param name="os">系统</param>
    /// <returns></returns>
    public static async Task<PojavLauncherTeamObj?> GetJavaList()
    {
        string url = BaseUrl + "update/java.json";

        HttpRequestMessage httpRequest = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
        };
        httpRequest.Headers.Add("ColorMC", ColorMCCore.Version);

        var data = await BaseClient.DownloadClient.SendAsync(httpRequest);
        var str = await data.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<PojavLauncherTeamObj>(str);
    }
}
