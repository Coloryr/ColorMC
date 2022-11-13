using ColorMC.Core.Http;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.Path;

public static class VersionPath
{
    private readonly static Dictionary<string, GameArgObj> Version = new();
    public static VersionObj? Versions { get; private set; }

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
        }
        catch (Exception e)
        {
            Logs.Error("读取版本信息错误", e);
        }
        try
        {
            Versions = GetFromWeb().Result;
            if (Version == null)
            {
                Versions = ReadVersions();
            }
            else
            {
                SaveVersions(Versions);
            }
        }
        catch (Exception e)
        {
            Logs.Error("获取版本信息错误", e);
        }
    }

    public static async Task<VersionObj?> GetFromWeb()
    {
        var res = await Get.GetVersions();
        if (res == null)
        {
            res = await Get.GetVersions(SourceLocal.Offical);
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
        return Versions?.versions.Where(a => a.id == version).Any() == true;
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

    public static void SaveVersions(VersionObj? obj)
    {
        if (obj == null)
            return;
        string file = BaseDir + "/version.json";
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));
    }

    public static void AddGame(GameArgObj? obj)
    {
        if (obj == null)
            return;
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

    /// <summary>
    /// 更新json
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static async Task CheckUpdate(string version)
    {
        if (Versions == null)
            return;

        var data = Versions.versions.Where(a => a.id == version).FirstOrDefault();
        if (data != null)
        {
            AddGame(await Get.GetGame(data.url));
        }
    }

    public static ForgeInstallObj? GetForgeObj(GameSettingObj obj)
    {
        return GetForgeObj(obj.Version, obj.LoaderInfo.Version);
    }

    public static ForgeInstallObj? GetForgeObj(string mc, string version)
    {
        string file =  $"{BaseDir}/{Name1}/forge-{mc}-{version}.json";
        var data = File.ReadAllText(file);

        try
        {
            return JsonConvert.DeserializeObject<ForgeInstallObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("读取forge信息错误", e);
            return null;
        }
    }

    public static FabricLoaderObj? GetFabricObj(string mc, string version)
    {
        string file = $"{BaseDir}/{Name2}/fabric-loader-{version}-{mc}.json";
        var data = File.ReadAllText(file);

        try
        {
            return JsonConvert.DeserializeObject<FabricLoaderObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error("读取fabric信息错误", e);
            return null;
        }
    }
}
