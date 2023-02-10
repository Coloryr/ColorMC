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

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        Dir = dir;
        Name = dir + "config.json";

        Load(Name);
    }

    /// <summary>
    /// 加载
    /// </summary>
    public static bool Load(string name, bool quit = false)
    {
        Logs.Info(LanguageHelper.GetName("Core.Config.Info1"));
        if (File.Exists(name))
        {
            try
            {
                Config = JsonConvert.DeserializeObject<ConfigObj>(File.ReadAllText(name))!;
            }
            catch (Exception e)
            {
                string text = LanguageHelper.GetName("Core.Config.Error1");
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
            Logs.Warn(LanguageHelper.GetName("Core.Config.Warn1"));

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
            if (Config.GameCheck == null)
            {
                if (quit)
                {
                    return false;
                }
                Config.GameCheck = MakeGameCheckConfig();
            }
        }

        BaseClient.Init();
        JvmPath.AddList(Config.JavaList);
        LanguageHelper.Change(Config.Language);
        BaseClient.Source = Config.Http.Source;

        Save();

        return true;
    }

    /// <summary>
    /// 保存
    /// </summary>
    public static void Save()
    {
        Logs.Info(LanguageHelper.GetName("Core.Config.Info2"));
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
            Window = MakeWindowSettingConfig(),
            GameCheck = MakeGameCheckConfig()
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

    private static JvmArgObj MakeJvmArgConfig()
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

    private static WindowSettingObj MakeWindowSettingConfig()
    {
        return new()
        {
            Height = 720,
            Width = 1280,
            FullScreen = false
        };
    }

    private static GameCheckObj MakeGameCheckConfig()
    {
        return new()
        {
            CheckCore = true,
            CheckAssets = true,
            CheckLib = true,
            CheckMod = true
        };
    }
}
