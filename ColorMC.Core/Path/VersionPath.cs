using ColorMC.Core.Http;
using ColorMC.Core.Objs.Game;
using Newtonsoft.Json;

namespace ColorMC.Core.Path;

public static class VersionPath
{
    private readonly static Dictionary<string, GameArgObj> Version = new();
    public static VersionObj Versions { get; private set; }

    public static string ForgeDir => BaseDir + "/" + Name1;
    public static string FabricDir => BaseDir + "/" + Name2;


    private const string Name = "versions";

    private const string Name1 = "forge";
    private const string Name2 = "fabric";

    public static string BaseDir { get; private set; }

    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;

        Directory.CreateDirectory(BaseDir);

        Directory.CreateDirectory($"{BaseDir}/{Name1}");
        Directory.CreateDirectory($"{BaseDir}/{Name2}");

        DirectoryInfo info = new(BaseDir);

        try
        {
            foreach (var item in info.GetFiles())
            {
                if (!item.Name.EndsWith(".json"))
                    continue;

                if (item.Name == "version.json")
                    continue;

                var data = File.ReadAllText(item.FullName);

                var obj = JsonConvert.DeserializeObject<GameArgObj>(data);
                if (obj != null)
                {
                    Version.Add(obj.id, obj);
                }
            }

            var res = ReadVersions();
            if (res == null)
            {
                res = GetFromWeb().Result;
                if (res == null)
                {
                    Logs.Error("版本信息为空");
                    return;
                }
                SaveVersions(res);
            }

            Versions = res;
        }
        catch (Exception e)
        {
            Logs.Error("读取版本信息错误", e);
        }
    }

    public static async Task<VersionObj?> GetFromWeb()
    {
        var res = await Get.GetVersion();
        if (res == null)
        {
            res = await Get.GetVersion(SourceLocal.Offical);
            if (res == null)
            {
                Logs.Warn("获取版本信息错误");
                return null;
            }
        }

        return res;
    }

    public static bool Have(string version)
    {
        return Versions.versions.Where(a => a.id == version).Any();
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

    public static void AddGame(GameArgObj obj)
    {
        string file = BaseDir + "/" + obj.id + ".json";
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));
        if (Version.ContainsKey(obj.id))
        {
            Version[obj.id] = obj;
        }
        else
        {
            Version.Add(obj.id, obj);
        }
    }

    public static GameArgObj? GetGame(string version)
    {
        if (Version.TryGetValue(version, out var item))
        {
            return item;
        }

        return null;
    }
}
