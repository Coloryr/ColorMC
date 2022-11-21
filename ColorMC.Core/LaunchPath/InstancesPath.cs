using ColorMC.Core.Http.Download;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.LaunchPath;

public static class InstancesPath
{
    private const string Name = "instances";
    private const string Name1 = "game.json";
    private const string Name2 = ".minecraft";
    private const string Name3 = "modinfo.json";
    private const string Name4 = "manifest.json";
    private const string Name5 = "options.txt";
    private const string Name6 = "servers.dat";
    private const string Name7 = "screenshots";
    private const string Name8 = "resourcepacks";
    private const string Name9 = "shaderpacks";
    private const string Name10 = "usercache.json";
    private const string Name11 = "usernamecache.json";
    private const string Name12 = "icon.png";
    private const string Name13 = "mods";
    private const string Name14 = "saves";

    private static Dictionary<string, GameSettingObj> Games = new();

    public static string BaseDir { get; private set; }

    public static void Init(string dir)
    {
        BaseDir = Path.GetFullPath(dir + "/" + Name);

        Logs.Info($"正在读取游戏对象信息");

        Directory.CreateDirectory(BaseDir);

        var list = Directory.GetDirectories(BaseDir);
        foreach (var item in list)
        {
            var data = new DirectoryInfo(item);
            var list1 = data.GetFiles();
            var list2 = list1.Where(a => a.Name == Name1).ToList();
            if (list2.Any())
            {
                var item1 = list2.First();
                var data1 = File.ReadAllText(item1.FullName);
                var game = JsonConvert.DeserializeObject<GameSettingObj>(data1);
                if (game != null)
                {
                    Games.Add(game.Name, game);
                }
            }
        }
    }

    public static List<GameSettingObj> GetGames()
    {
        return new(Games.Values);
    }

    public static GameSettingObj? GetGame(string name)
    {
        if (Games.TryGetValue(name, out var item))
        {
            return item;
        }

        return null;
    }

    public static void Save(this GameSettingObj obj)
    {
        File.WriteAllText(obj.GetGameJsonPath(), JsonConvert.SerializeObject(obj));
    }

    public static string GetBaseDir(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}");
    }

    public static string GetGameDir(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}");
    }

    public static string GetGameJsonPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name1}");
    }

    public static string GetModJsonFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name3}");
    }

    public static string GetModInfoJsonFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name4}");
    }

    public static string GetOptionsFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name5}");
    }

    public static string GetServersFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name6}");
    }

    public static string GetScreenshotsPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name7}");
    }

    public static string GetResourcepacksPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name8}");
    }

    public static string GetShaderpacksPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name9}");
    }

    public static string GetUserCacheFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name10}");
    }

    public static string GetUserNameCacheFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name11}");
    }

    public static string GetIconFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name12}");
    }

    public static string GetModsPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name13}");
    }

    public static string GetSavesPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name14}");
    }

    public static GameSettingObj? CreateVersion(string name, string version,
        bool pack, Loaders loader, LoaderInfoObj info)
    {
        if (Games.ContainsKey(name))
        {
            return null;
        }

        var game = new GameSettingObj()
        {
            DirName = name,
            Name = name,
            Version = version,
            Loader = loader,
            LoaderInfo = info,
            ModPack = pack
        };

        var dir = game.GetBaseDir();
        if (Directory.Exists(dir))
        {
            return null;
        }

        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(game.GetGameDir());

        game.Save();
        Games.Add(name, game);

        return game;
    }

    public static Task InstallForge(this GameSettingObj obj, string version)
    {
        obj.LoaderInfo = new()
        {
            Version = version,
            Name = "forge"
        };
        obj.Loader = Loaders.Forge;
        obj.Save();

        return GameDownload.DownloadForge(obj.Version, version);
    }

    public static Task InstallFabric(this GameSettingObj obj, string version)
    {
        obj.LoaderInfo = new()
        {
            Version = version,
            Name = "fabric"
        };
        obj.Loader = Loaders.Fabric;
        obj.Save();

        return GameDownload.DownloadFabric(obj.Version, version);
    }

    public static Task InstallQuilt(this GameSettingObj obj, string version)
    {
        obj.LoaderInfo = new()
        {
            Version = version,
            Name = "quilt"
        };
        obj.Loader = Loaders.Quilt;
        obj.Save();

        return GameDownload.DownloadQuilt(obj.Version, version);
    }

    public static void Uninstall(this GameSettingObj obj)
    {
        obj.LoaderInfo = null;
        obj.Loader = Loaders.Normal;
        obj.Save();
    }

    public static async Task<GameSettingObj?> Copy(this GameSettingObj obj, string name)
    {
        var obj1 = CreateVersion(name, obj.Version, obj.ModPack, obj.Loader, obj.LoaderInfo);
        if (obj1 != null)
        {
            await PathC.CopyFiles(GetGameDir(obj), GetGameDir(obj1));
            if (obj.ModPack)
            {
                File.Copy(obj.GetModJsonFile(), obj1.GetModJsonFile(), true);
                File.Copy(obj.GetModInfoJsonFile(), obj1.GetModInfoJsonFile(), true);
            }

            return obj1;
        }

        return null;
    }

    public static void Remove(this GameSettingObj obj)
    {
        PathC.DeleteFiles(obj.GetBaseDir());
    }
}