using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Config;
using ColorMC.Core.Utils;

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
    internal static void Init()
    {
        s_local = Path.Combine(ColorMCCore.BaseDir, Names.NameConfigFile);

        Load(s_local);
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="quit">出错后直接返回</param>
    /// <returns>是否加载成功</returns>
    public static bool Load(string local, bool quit = false)
    {
        Logs.Info(LanguageHelper.Get("Core.Config.Info1"));

        using var data = PathHelper.OpenRead(local);
        ConfigObj? obj = null;
        if (data == null)
        {
            if (!quit)
            {
                ColorMCCore.NewStart = true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            try
            {
                obj = JsonUtils.ToObj(data, JsonType.ConfigObj);
            }
            catch (Exception e)
            {
                ColorMCCore.OnError(LanguageHelper.Get("Core.Config.Error1"), e, true);
            }
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
            obj.Dns ??= MakeDnsConfig();
            obj.Dns.Dns ??= [];
            obj.Dns.Https ??= [];

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
    /// 保存配置文件
    /// </summary>
    public static void Save()
    {
        Logs.Info(LanguageHelper.Get("Core.Config.Info2"));
        ConfigSave.AddItem(ConfigSaveObj.Build(Names.NameConfigFile, s_local, Config, JsonType.ConfigObj));
    }

    /// <summary>
    /// 立即保存配置文件
    /// </summary>
    public static void SaveNow()
    {
        Logs.Info(LanguageHelper.Get("Core.Config.Info2"));
        File.WriteAllText(s_local, JsonUtils.ToString(Config, JsonType.ConfigObj));
    }

    /// <summary>
    /// 创建新的配置文件
    /// </summary>
    /// <returns>配置内容</returns>
    public static ConfigObj MakeDefaultConfig()
    {
        return new()
        {
            Version = ColorMCCore.Version,
            JavaList = [],
            Http = MakeHttpConfig(),
            DefaultJvmArg = MakeJvmArgConfig(),
            Window = MakeWindowSettingConfig(),
            GameCheck = MakeGameCheckConfig(),
            Dns = MakeDnsConfig()
        };
    }

    /// <summary>
    /// 创建新的配置文件
    /// </summary>
    /// <returns>配置内容</returns>
    public static DnsObj MakeDnsConfig()
    {
        return new()
        {
            Dns = ["1.1.1.1", "1.0.0.1", "8.8.8.8", "8.8.4.4"],
            Https = ["https://223.5.5.5/dns-query"]
        };
    }

    /// <summary>
    /// 创建新的配置文件
    /// </summary>
    /// <returns>配置内容</returns>
    public static HttpObj MakeHttpConfig()
    {
        return new()
        {
            Source = SourceLocal.Offical,
            DownloadThread = 5,
            ProxyIP = "127.0.0.1",
            ProxyPort = 1080,
            CheckFile = true,
            AutoDownload = true,
        };
    }

    /// <summary>
    /// 创建新的配置文件
    /// </summary>
    /// <returns>配置内容</returns>
    public static RunArgObj MakeJvmArgConfig()
    {
        return new()
        {
            GC = GCType.G1GC,
            MaxMemory = 4096,
            MinMemory = 512,
        };
    }

    /// <summary>
    /// 创建新的配置文件
    /// </summary>
    /// <returns>配置内容</returns>
    public static WindowSettingObj MakeWindowSettingConfig()
    {
        return new()
        {
            Height = 720,
            Width = 1280
        };
    }

    /// <summary>
    /// 创建新的配置文件
    /// </summary>
    /// <returns>配置内容</returns>
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
