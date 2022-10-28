using ColorMC.Core.Path;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ColorMC.Core.Config;

public static class ConfigUtils
{
    public static ConfigObj Config { get; set; }

    public static string Dir;

    private static string Name;

    public static void Init(string dir)
    {
        Dir = dir;
        Name = dir + "config.json";

        try
        {
            Load(Name);

            if (Config == null)
            {
                Config = MakeDefaultConfig();
            }
            Save();
        }
        catch (Exception e)
        {
            CoreMain.OnError?.Invoke("配置文件读取错误", e, true);
        }
    }

    public static void Load(string name)
    {
        if (File.Exists(name))
        {
            Config = JsonConvert.DeserializeObject<ConfigObj>(File.ReadAllText(name));

            if (Config == null)
            {
                throw new Exception("配置为空");
            }

            if (Config.JavaList == null)
            {
                Config.JavaList = new();
            }
            JvmPath.AddList(Config.JavaList);

            Save();
        }
    }

    public static void Save()
    {
        File.WriteAllText(Name, JsonConvert.SerializeObject(Config));
    }

    private static ConfigObj MakeDefaultConfig()
    {
        return new()
        {
            Version = CoreMain.Version,
            MCPath = "./.minectaft",
            JavaList = new(),
            GameList = new(),
        };
    }
}
