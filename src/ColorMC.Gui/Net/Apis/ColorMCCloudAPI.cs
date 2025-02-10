using System;
using System.Net.Http;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.NetFrp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Gui.Net.Apis;

/// <summary>
/// ColorMC相关API
/// </summary>
public static class ColorMCCloudAPI
{
    /// <summary>
    /// 更新检查网址
    /// </summary>
    public const string CheckUrl = $"{ColorMCAPI.BaseUrl}update/{ColorMCCore.TopVersion}/";

    /// <summary>
    /// 获取更新日志
    /// </summary>
    /// <returns>日志内容</returns>
    public static async Task<string?> GetNewLog()
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, ColorMCAPI.BaseUrl + "update/log");
            req.Headers.Add("ColorMC", ColorMCCore.Version);
            var data = await CoreHttpClient.DownloadClient.SendAsync(req);
            return await data.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error2"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取主版本号
    /// </summary>
    /// <returns></returns>
    public static async Task<JObject> GetUpdateIndex()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, ColorMCAPI.BaseUrl + "update/index.json");
        req.Headers.Add("ColorMC", ColorMCCore.Version);
        var data = await CoreHttpClient.DownloadClient.SendAsync(req);
        return JObject.Parse(await data.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// 获取文件Sha1
    /// </summary>
    /// <returns></returns>
    public static async Task<JObject> GetUpdateSha1()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, CheckUrl + "sha1.json");
        req.Headers.Add("ColorMC", ColorMCCore.Version);
        var data = await CoreHttpClient.DownloadClient.SendAsync(req);
        string text = await data.Content.ReadAsStringAsync();
        return JObject.Parse(text);
    }

    /// <summary>
    /// 获取在线服务器
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns></returns>
    public static async Task<JObject?> GetCloudServer(string version)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, ColorMCAPI.BaseUrl + "frplist?version=" + version);
            req.Headers.Add("ColorMC", ColorMCCore.Version);
            var data = await CoreHttpClient.DownloadClient.SendAsync(req);
            return JObject.Parse(await data.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error4"), e);
            return null;
        }
    }

    /// <summary>
    /// 上传在线服务器
    /// </summary>
    /// <param name="token">Minecraft token</param>
    /// <param name="ip">IP地址</param>
    /// <param name="model">显示内容</param>
    /// <returns>是否上传成功</returns>
    public static async Task<bool> PutCloudServer(string token, string ip, FrpShareModel model)
    {
        var httpRequest = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(ColorMCAPI.BaseUrl + "frp"),
            Content = new StringContent(JsonConvert.SerializeObject(new
            {
                token,
                ip,
                custom = new
                {
                    model.Version,
                    model.Loader,
                    model.IsLoader,
                    model.Text
                }
            }))
        };
        httpRequest.Headers.Add("ColorMC", ColorMCCore.Version);

        try
        {
            var data = await CoreHttpClient.DownloadClient.SendAsync(httpRequest);
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
}
