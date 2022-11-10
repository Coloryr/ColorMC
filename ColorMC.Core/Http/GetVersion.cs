using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs.Game;

namespace ColorMC.Core.Http;

public static class GetVersion
{
    public static async Task<VersionObj?> Get(SourceLocal? local = null) 
    {
        try
        {
            var data = await BaseClient.GetString(UrlHelp.Version(local));
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<VersionObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error( "获取版本信息发生错误", e);
            return null;
        }
    }
}
