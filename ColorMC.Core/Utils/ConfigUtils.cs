using ColorMC.Core.Http;
using ColorMC.Core.Objs;
using ColorMC.Core.LaunchPath;
using Newtonsoft.Json;

namespace ColorMC.Core.Utils;

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
            Config = JsonConvert.DeserializeObject<ConfigObj>(File.ReadAllText(name))!;

            if (Config == null)
            {
                throw new Exception("配置为空");
            }

            if (Config.JavaList == null)
            {
                Config.JavaList = new();
            }
            if (Config.Http == null)
            {
                Config.Http = MakeHttpConfig();
            }
            if (Config.DefaultJvmArg == null)
            {
                Config.DefaultJvmArg = MakeJvmArgConfig();
            }
            if (Config.Window == null)
            {
                Config.Window = MakeWindowSettingConfig();
            }

            JvmPath.AddList(Config.JavaList);
            BaseClient.Source = Config.Http.Source;

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
            Http = MakeHttpConfig(),
            DefaultJvmArg = MakeJvmArgConfig(),
            Window = MakeWindowSettingConfig()
        };
    }

    private static HttpObj MakeHttpConfig()
    {
        return new()
        {
            Source = SourceLocal.Offical,
            DownloadThread = 5,
            Proxy = false,
            ProxyIP = "127.0.0.1",
            ProxyPort = 1080
        };
    }

    public static JvmArgObj MakeJvmArgConfig()
    {
        return new()
        {
            AdvencedJvmArguments = null,
            GC = JvmArgObj.GCType.G1GC,
            GCArgument = null,
            JavaAgent = null,
            MaxMemory = 4096,
            MinMemory = 512,
        };
    }

    public static WindowSettingObj MakeWindowSettingConfig()
    {
        return new()
        {
            Height = 720,
            Width = 1280,
            FullScreen = false
        };
    }

    public static void CopyTo(this JvmArgObj obj1, JvmArgObj obj2)
    {
        obj2.AdvencedJvmArguments = obj1.AdvencedJvmArguments;
        obj2.GCArgument = obj1.GCArgument;
        obj2.GC = obj1.GC;
        obj2.JavaAgent = obj1.JavaAgent;
        obj2.MaxMemory = obj1.MaxMemory;
        obj2.MinMemory = obj1.MinMemory;
    }
}
