using ColorMC.Core.Objs.Frp;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Net.Apis;

public static class SakuraFrpAPI
{
    public const string Url = "https://api.natfrp.com/v4/";

    public static async Task<SakuraFrpUserObj?> GetUserInfo(string key)
    {
        try
        {
            var data = await BaseClient.LoginClient.GetStringAsync($"{Url}user/info?token={key}");

            return JsonConvert.DeserializeObject<SakuraFrpUserObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }

    public static async Task<List<SakuraFrpChannelObj>?> GetChannel(string key)
    {
        try
        {
            var data = await BaseClient.LoginClient.GetStringAsync($"{Url}tunnels?token={key}");

            return JsonConvert.DeserializeObject<List<SakuraFrpChannelObj>>(data);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }

    public static async Task<SakuraFrpDownloadObj?> GetDownload()
    {
        try
        {
            var data = await BaseClient.LoginClient.GetStringAsync($"{Url}system/clients");

            return JsonConvert.DeserializeObject<SakuraFrpDownloadObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }

    public static async Task<string?> GetChannelConfig(string key, int id)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { query = id }),
                MediaTypeHeaderValue.Parse("application/json"));

            var data = await BaseClient.LoginClient.PostAsync($"{Url}tunnel/config?token={key}", content);
            var str = await data.Content.ReadAsStringAsync();
            if (str.StartsWith("{"))
            {
                return null;
            }

            return str;
        }
        catch (Exception e)
        {
            Logs.Error("frp", e);
        }

        return null;
    }
}
