using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
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

        Load(Name);
    }

    public static bool Load(string name, bool quit = false)
    {
        Logs.Info(LanguageHelper.GetName("Core.Config.Load"));
        if (File.Exists(name))
        {
            try
            {
                Config = JsonConvert.DeserializeObject<ConfigObj>(File.ReadAllText(name))!;
            }
            catch (Exception e)
            {
                string text = LanguageHelper.GetName("Core.Config.Load.Error");
                CoreMain.OnError?.Invoke(text, e, true);
                Logs.Error(text, e);
            }
        }
        else
        {
            CoreMain.NewStart?.Invoke();
        }

        if (Config == null)
        {
            if (quit)
            {
                return false;
            }
            Logs.Warn(LanguageHelper.GetName("Core.Config.Empty"));

            Config = MakeDefaultConfig();
            Save();
        }
        else
        {
            if (Config.JavaList == null)
            {
                if (quit)
                {
                    return false;
                }
                Config.JavaList = new();
            }
            if (Config.Http == null)
            {
                if (quit)
                {
                    return false;
                }
                Config.Http = MakeHttpConfig();
            }
            if (Config.DefaultJvmArg == null)
            {
                if (quit)
                {
                    return false;
                }
                Config.DefaultJvmArg = MakeJvmArgConfig();
            }
            if (Config.Window == null)
            {
                if (quit)
                {
                    return false;
                }
                Config.Window = MakeWindowSettingConfig();
            }
        }

        BaseClient.Init();
        JvmPath.AddList(Config.JavaList);
        LanguageHelper.Change(Config.Language);
        BaseClient.Source = Config.Http.Source;

        Save();

        return true;
    }

    public static void Save()
    {
        Logs.Info(LanguageHelper.GetName("Core.Config.Save"));
        File.WriteAllText(Name, JsonConvert.SerializeObject(Config, Formatting.Indented));
    }

    private static ConfigObj MakeDefaultConfig()
    {
        return new()
        {
            Version = CoreMain.Version,
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
            ProxyIP = "127.0.0.1",
            ProxyPort = 1080,
            CheckFile = true
        };
    }

    public static JvmArgObj MakeJvmArgConfig()
    {
        return new()
        {
            JvmArgs = null,
            GC = GCType.G1GC,
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
        obj2.JvmArgs = obj1.JvmArgs;
        obj2.GCArgument = obj1.GCArgument;
        obj2.GC = obj1.GC;
        obj2.JavaAgent = obj1.JavaAgent;
        obj2.MaxMemory = obj1.MaxMemory;
        obj2.MinMemory = obj1.MinMemory;
    }
}
