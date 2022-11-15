using ColorMC.Core.Http;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using Newtonsoft.Json;

namespace ColorMC.Core.Path;

public static class VersionPath
{
    private readonly static Dictionary<string, GameArgObj> Version = new();
    private readonly static Dictionary<string, ForgeLaunchObj> Forges = new();
    private readonly static Dictionary<string, ForgeInstallObj> ForgeInstalls = new();
    private readonly static Dictionary<string, FabricLoaderObj> Fabrics = new();
    private readonly static Dictionary<string, QuiltLoaderObj> Quilts = new();
    public static VersionObj? Versions { get; private set; }

    public static string ForgeDir => BaseDir + "/" + Name1;
    public static string FabricDir => BaseDir + "/" + Name2;
    public static string QuiltDir => BaseDir + "/" + Name3;


    private const string Name = "versions";

    private const string Name1 = "forge";
    private const string Name2 = "fabric";
    private const string Name3 = "quilt";

    public static string BaseDir { get; private set; }

    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;

        Directory.CreateDirectory(BaseDir);

        Directory.CreateDirectory(ForgeDir);
        Directory.CreateDirectory(FabricDir);
        Directory.CreateDirectory(QuiltDir);

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

    public static ForgeInstallObj? GetForgeInstallObj(GameSettingObj obj)
    {
        return GetForgeInstallObj(obj.Version, obj.LoaderInfo.Version);
    }

    public static ForgeInstallObj? GetForgeInstallObj(string mc, string version)
    {
        string key = $"{mc}-{version}";
        if (ForgeInstalls.TryGetValue(key, out var value) && value != null)
        {
            return value;
        }

        string file = $"{BaseDir}/{Name1}/forge-{mc}-{version}-install.json";

        if (!File.Exists(file))
            return null;

        try
        {
            var data = File.ReadAllText(file);
            var data1 = JsonConvert.DeserializeObject<ForgeInstallObj>(data)!;
            ForgeInstalls.Add(key, data1);
            return data1;
        }
        catch (Exception e)
        {
            Logs.Error("读取forge安装信息错误", e);
            return null;
        }
    }

    public static ForgeLaunchObj? GetForgeObj(GameSettingObj obj)
    {
        return GetForgeObj(obj.Version, obj.LoaderInfo.Version);
    }

    public static ForgeLaunchObj? GetForgeObj(string mc, string version)
    {
        string key = $"{mc}-{version}";
        if (Forges.TryGetValue(key, out var value) && value != null)
        {
            return value;
        }

        string file = $"{BaseDir}/{Name1}/forge-{mc}-{version}.json";

        if (!File.Exists(file))
            return null;

        try
        {
            var data = File.ReadAllText(file);
            var data1 = JsonConvert.DeserializeObject<ForgeLaunchObj>(data)!;
            Forges.Add(key, data1);
            return data1;
        }
        catch (Exception e)
        {
            Logs.Error("读取forge信息错误", e);
            return null;
        }
    }

    public static FabricLoaderObj? GetFabricObj(GameSettingObj obj)
    {
        return GetFabricObj(obj.Version, obj.LoaderInfo.Version);
    }

    public static FabricLoaderObj? GetFabricObj(string mc, string version)
    {
        string key = $"{mc}-{version}";
        if (Fabrics.TryGetValue(key, out var value) && value != null)
        {
            return value;
        }

        string file = $"{BaseDir}/{Name2}/fabric-loader-{version}-{mc}.json";

        if (!File.Exists(file))
            return null;

        try
        {
            var data = File.ReadAllText(file);
            var data1 = JsonConvert.DeserializeObject<FabricLoaderObj>(data)!;
            Fabrics.Add(key, data1);
            return data1;
        }
        catch (Exception e)
        {
            Logs.Error("读取fabric信息错误", e);
            return null;
        }
    }

    public static QuiltLoaderObj? GetQuiltObj(GameSettingObj obj)
    {
        return GetQuiltObj(obj.Version, obj.LoaderInfo.Version);
    }

    public static QuiltLoaderObj? GetQuiltObj(string mc, string version)
    {
        string key = $"{mc}-{version}";
        if (Quilts.TryGetValue(key, out var value) && value != null)
        {
            return value;
        }

        string file = $"{BaseDir}/{Name3}/quilt-loader-{version}-{mc}.json";

        if (!File.Exists(file))
            return null;

        try
        {
            var data = File.ReadAllText(file);
            var data1 = JsonConvert.DeserializeObject<QuiltLoaderObj>(data)!;
            Quilts.Add(key, data1);
            return data1;
        }
        catch (Exception e)
        {
            Logs.Error("读取fabric信息错误", e);
            return null;
        }
    }
}
