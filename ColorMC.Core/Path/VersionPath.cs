using ColorMC.Core.Http;
using ColorMC.Core.Objs.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Path;

public static class VersionPath
{
    private const string Name = "versions";
    private static string BaseDir { get; set; }

    public static void Init(string dir) 
    {
        BaseDir = dir + "/" + Name;

        Directory.CreateDirectory(BaseDir);
    }

    public static VersionObj? ReadVersions() 
    {
        string file = BaseDir + "/version.json";
        if (File.Exists(file))
        {
            string data = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<VersionObj>(data);
        }
        return null;
    }

    public static void SaveVersions(VersionObj obj) 
    {
        string file = BaseDir + "/version.json";
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));
    }
}
