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

public static class ColorMCCloudAPI
{
    public const string CheckUrl = $"{ColorMCAPI.BaseUrl}update/{ColorMCCore.TopVersion}/";

    public static async Task<string?> GetNewLog()
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, ColorMCAPI.BaseUrl + "update/log");
            req.Headers.Add("ColorMC", ColorMCCore.Version);
            var data = await WebClient.DownloadClient.SendAsync(req);
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
        var req = new HttpRequestMessage(HttpMethod.Get, ColorMCAPI.BaseUrl + "update/index.json");
        req.Headers.Add("ColorMC", ColorMCCore.Version);
        var data = await WebClient.DownloadClient.SendAsync(req);
        return JObject.Parse(await data.Content.ReadAsStringAsync());
    }

    public static async Task<JObject> GetUpdateSha1()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, CheckUrl + "sha1.json");
        req.Headers.Add("ColorMC", ColorMCCore.Version);
        var data = await WebClient.DownloadClient.SendAsync(req);
        string text = await data.Content.ReadAsStringAsync();
        return JObject.Parse(text);
    }

    public static async Task<JObject?> GetCloudServer(string version)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, ColorMCAPI.BaseUrl + "frplist?version=" + version);
            req.Headers.Add("ColorMC", ColorMCCore.Version);
            var data = await WebClient.DownloadClient.SendAsync(req);
            return JObject.Parse(await data.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error4"), e);
            return null;
        }
    }

    public static async Task<bool> PutCloudServer(string token, string ip, FrpShareModel model)
    {
        HttpRequestMessage httpRequest = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(ColorMCAPI.BaseUrl + "frp"),
        };
        httpRequest.Headers.Add("ColorMC", ColorMCCore.Version);
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(new
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
        }));

        try
        {
            var data = await WebClient.DownloadClient.SendAsync(httpRequest);
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
