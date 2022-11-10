using ColorMC.Core.Objs.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Http;

public static class GetGame
{
    public static async Task<GameArgObj?> Get(string url)
    {
        try
        {
            var data = await BaseClient.GetString(url);
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<GameArgObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("获取游戏信息发生错误", e);
            return null;
        }
    }
}
