using ColorMC.Core.Objs.Frp;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
}
