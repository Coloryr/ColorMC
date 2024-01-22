using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.Config;

/// <summary>
/// 配置文件
/// </summary>
public static class ConfigUtils
{
    /// <summary>
    /// 配置文件
    /// </summary>
    public static ConfigObj Config { get; set; }
    /// <summary>
    /// 配置文件路径
    /// </summary>
    private static string s_local;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        s_local = dir + "config.json";

        Load(s_local);
    }

    /// <summary>
    /// 加载
    /// </summary>
    public static bool Load(string local, bool quit = false)
    {
        Logs.Info(LanguageHelper.Get("Core.Config.Info1"));

        var data = PathHelper.ReadText(local);
        ConfigObj? obj = null;
        if (data != null)
        {
            try
            {
                obj = JsonConvert.DeserializeObject<ConfigObj>(data);
            }
            catch (Exception e)
            {
                ColorMCCore.Error(LanguageHelper.Get("Core.Config.Error1"), e, true);
            }
        }
        else if (!quit)
        {
            ColorMCCore.NewStart = true;
        }
        else
        {
            return false;
        }

        if (obj == null)
        {
            if (quit)
            {
                return false;
            }
            Logs.Warn(LanguageHelper.Get("Core.Config.Warn1"));

            Config = MakeDefaultConfig();
        }
        else
        {
            obj.JavaList ??= [];
            obj.Http ??= MakeHttpConfig();
            obj.DefaultJvmArg ??= MakeJvmArgConfig();
            obj.Window ??= MakeWindowSettingConfig();
            obj.GameCheck ??= MakeGameCheckConfig();

            Config = obj;
        }

        if (Config.Version != ColorMCCore.Version)
        {
            ColorMCCore.NewStart = true;
            Config.Version = ColorMCCore.Version;
        }

        LanguageHelper.Change(Config.Language);

        Save();

        return true;
    }

    /// <summary>
    /// 保存
    /// </summary>
    public static void Save()
    {
        Logs.Info(LanguageHelper.Get("Core.Config.Info2"));
        ConfigSave.AddItem(new()
        {
            Name = "config.json",
            Local = s_local,
            Obj = Config
        });
    }

    public static ConfigObj MakeDefaultConfig()
    {
        return new()
        {
            Version = ColorMCCore.Version,
            JavaList = [],
            Http = MakeHttpConfig(),
            DefaultJvmArg = MakeJvmArgConfig(),
            Window = MakeWindowSettingConfig(),
            GameCheck = MakeGameCheckConfig()
        };
    }

    public static HttpObj MakeHttpConfig()
    {
        return new()
        {
            Source = SourceLocal.Offical,
            DownloadThread = 5,
            ProxyIP = "127.0.0.1",
            ProxyPort = 1080,
            CheckFile = true,
            CheckUpdate = true,
            AutoDownload = true,
        };
    }

    public static RunArgObj MakeJvmArgConfig()
    {
        return new()
        {
            GC = GCType.G1GC,
            MaxMemory = 4096,
            MinMemory = 512,
        };
    }

    public static WindowSettingObj MakeWindowSettingConfig()
    {
        return new()
        {
            Height = 720,
            Width = 1280
        };
    }

    public static GameCheckObj MakeGameCheckConfig()
    {
        return new()
        {
            CheckCore = true,
            CheckAssets = true,
            CheckLib = true,
            CheckMod = true,
            CheckCoreSha1 = true,
            CheckAssetsSha1 = true,
            CheckLibSha1 = true,
            CheckModSha1 = true,
        };
    }
}
