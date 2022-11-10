using ColorMC.Core.Config;
using ColorMC.Core.Http;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Path;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Utils;

public static class VersionCheck
{
    private static VersionObj Versions = new();

    public static void Init() 
    {
        try
        {
            var res = VersionPath.ReadVersions();
            if (res == null)
            {
                GetFromWeb();
            }
            else
            {
                Versions = res;
            }
        }
        catch (Exception e)
        {
            Logs.Error("读取版本信息错误", e);
        }
    }

    public static async void GetFromWeb() 
    {
        var res = await GetVersion.Get();
        if (res == null)
        {
            res = await GetVersion.Get(SourceLocal.Offical);
            if (res == null)
            {
                Logs.Warn("读取版本信息错误");
                return;
            }
        }

        Versions = res;

        VersionPath.SaveVersions(res);
    }

    public static bool Have(string version) 
    {
        return Versions.versions.Where(a => a.id == version).Any();
    }
}
