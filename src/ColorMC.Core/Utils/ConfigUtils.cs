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
                ColorMCCore.OnError?.Invoke(text, e, true);
                Logs.Error(text, e);
            }
        }
        else
        {
            ColorMCCore.NewStart = true;
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

        Save();

        return true;
    }

    /// <summary>
    /// 保存
    /// </summary>
    public static void Save()
    {
        Logs.Info(LanguageHelper.GetName("Core.Config.Info2"));
        ConfigSave.AddItem(new()
        {
            Name = "config.json",
            Local = Name,
            Obj = Config
        });
    }

    private static ConfigObj MakeDefaultConfig()
    {
        return new()
        {
            Version = ColorMCCore.Version,
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
            CheckFile = true,
            CheckUpdate = true,
            AutoDownload = true
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

public record ConfigSaveObj
{
    public string Name;
    public object Obj;
    public string Local;
}

public static class ConfigSave
{
    private static readonly Dictionary<string, ConfigSaveObj> SaveQue = new();

    private static readonly object Lock = new();

    private static Thread thread;
    private static bool run;

    public static void Init()
    {
        ColorMCCore.Stop += Stop;

        thread = new(Run)
        {
            Name = "ColorMC-Save"
        };
        run = true;
        thread.Start();
    }

    private static void Stop()
    {
        run = false;
        thread.Join();

        lock (Lock)
        {
            foreach (var item in SaveQue.Values)
            {
                File.WriteAllText(item.Local,
                    JsonConvert.SerializeObject(item.Obj, Formatting.Indented));
            }

            SaveQue.Clear();
        }
    }

    private static void Run()
    {
        while (run)
        {
            Thread.Sleep(1000);
            if (!SaveQue.Any())
                continue;

            lock (Lock)
            {
                foreach (var item in SaveQue.Values)
                {
                    if (new FileInfo(item.Local)?.Directory?.Exists == true)
                    {
                        try
                        {
                            File.WriteAllText(item.Local,
                                JsonConvert.SerializeObject(item.Obj, Formatting.Indented));
                        }
                        catch (Exception e)
                        {
                            Logs.Error("Save Error", e);
                        }
                    }
                }
                SaveQue.Clear();
            }
        }
    }

    public static void AddItem(ConfigSaveObj obj)
    {
        lock (Lock)
        {
            if (!SaveQue.ContainsKey(obj.Name))
            {
                SaveQue.Add(obj.Name, obj);
            }
        }
    }
}