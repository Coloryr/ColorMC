using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.LaunchPath;

/// <summary>
/// 版本路径
/// </summary>
public static class VersionPath
{
    public const string Name = "versions";

    public const string Name1 = "forge";
    public const string Name2 = "fabric";
    public const string Name3 = "quilt";
    public const string Name4 = "neoforged";

    private readonly static Dictionary<string, GameArgObj> s_gameArgs = new();
    private readonly static Dictionary<string, ForgeInstallObj> s_forgeInstalls = new();
    private readonly static Dictionary<string, ForgeInstallObj> s_neoForgeInstalls = new();
    private readonly static Dictionary<string, ForgeLaunchObj> s_forgeLaunchs = new();
    private readonly static Dictionary<string, ForgeLaunchObj> s_neoForgeLaunchs = new();
    private readonly static Dictionary<string, FabricLoaderObj> s_fabricLoaders = new();
    private readonly static Dictionary<string, QuiltLoaderObj> s_quiltLoaders = new();

    public static VersionObj? Versions { get; private set; }

    public static string ForgeDir => BaseDir + "/" + Name1;
    public static string FabricDir => BaseDir + "/" + Name2;
    public static string QuiltDir => BaseDir + "/" + Name3;
    public static string NeoForgeDir => BaseDir + "/" + Name4;

    public static string BaseDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        BaseDir = $"{dir}/{Name}";

        Directory.CreateDirectory(BaseDir);

        Directory.CreateDirectory(ForgeDir);
        Directory.CreateDirectory(NeoForgeDir);
        Directory.CreateDirectory(FabricDir);
        Directory.CreateDirectory(QuiltDir);

        try
        {
            ReadVersions();
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Path.Error2"), e);
        }
    }

    /// <summary>
    /// 从在线获取版本信息
    /// </summary>
    /// <returns></returns>
    public static async Task GetFromWeb()
    {
        Versions = await GameAPI.GetVersions();
        if (Versions != null)
        {
            SaveVersions();
            return;
        }
        Versions = await GameAPI.GetVersions(SourceLocal.Offical);
        if (Versions == null)
        {
            Logs.Error(LanguageHelper.Get("Core.Path.Error3"));
        }
        else
        {
            SaveVersions();
        }
    }

    /// <summary>
    /// 是否存在版本信息
    /// </summary>
    /// <returns>结果</returns>
    public static bool Have()
    {
        return Versions != null;
    }

    /// <summary>
    /// 读取版本信息
    /// </summary>
    public static async void ReadVersions()
    {
        string file = BaseDir + "/version.json";
        if (File.Exists(file))
        {
            string data = File.ReadAllText(file);
            Versions = JsonConvert.DeserializeObject<VersionObj>(data);
        }
        else
        {
            await GetFromWeb();
        }
    }

    /// <summary>
    /// 保存版本信息
    /// </summary>
    public static void SaveVersions()
    {
        if (Versions == null)
            return;
        string file = BaseDir + "/version.json";
        File.WriteAllText(file, JsonConvert.SerializeObject(Versions));
    }

    /// <summary>
    /// 添加版本信息
    /// </summary>
    /// <param name="obj">游戏数据</param>
    public static void AddGame(GameArgObj? obj)
    {
        if (obj == null)
        {
            return;
        }
        string file = $"{BaseDir}/{obj.id}.json";
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));
    }

    /// <summary>
    /// 获取版本信息
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns>游戏数据</returns>
    public static GameArgObj? GetGame(string version)
    {
        if (s_gameArgs.TryGetValue(version, out var temp))
        {
            return temp;
        }
        string file = $"{BaseDir}/{version}.json";

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<GameArgObj>(File.ReadAllText(file))!;
        s_gameArgs.Add(version, obj);
        return obj;
    }

    /// <summary>
    /// 更新json
    /// </summary>
    /// <param name="version">游戏版本</param>
    public static async Task CheckUpdate(string version)
    {
        if (Versions == null)
        {
            await GetFromWeb();
        }
        if (Versions == null)
        {
            return;
        }
        var data = Versions.versions.Where(a => a.id == version).FirstOrDefault();
        if (data != null)
        {
            AddGame(await GameAPI.GetGame(data.url));
        }
    }

    /// <summary>
    /// 获取Forge安装数数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>Forge安装数据</returns>
    public static ForgeInstallObj? GetNeoForgeInstallObj(string mc, string version)
    {
        var key = $"{mc}-{version}";
        if (s_forgeInstalls.TryGetValue(key, out var temp))
        {
            return temp;
        }

        string file = $"{BaseDir}/{Name4}/forge-{mc}-{version}-install.json";

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<ForgeInstallObj>(File.ReadAllText(file))!;
        s_forgeInstalls.Add(key, obj);
        return obj;
    }

    /// <summary>
    /// 获取Forge启动数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>启动数据</returns>
    public static ForgeLaunchObj? GetNeoForgeObj(this GameSettingObj obj)
    {
        return GetNeoForgeObj(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 获取Forge启动数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>启动数据</returns>
    public static ForgeLaunchObj? GetNeoForgeObj(string mc, string version)
    {
        var key = $"{mc}-{version}";
        if (s_neoForgeLaunchs.TryGetValue(key, out var temp))
        {
            return temp;
        }

        string file = Path.GetFullPath($"{BaseDir}/{Name4}/forge-{mc}-{version}.json");

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<ForgeLaunchObj>(File.ReadAllText(file))!;
        s_neoForgeLaunchs.Add(key, obj);
        return obj;
    }

    /// <summary>
    /// 获取Forge安装数数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>Forge安装数据</returns>
    public static ForgeInstallObj? GetForgeInstallObj(string mc, string version)
    {
        var key = $"{mc}-{version}";
        string file = $"{BaseDir}/{Name1}/forge-{mc}-{version}-install.json";
        if (s_neoForgeInstalls.TryGetValue(key, out var temp))
        {
            return temp;
        }

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<ForgeInstallObj>(File.ReadAllText(file))!;
        s_neoForgeInstalls.Add(key, obj);
        return obj;
    }

    /// <summary>
    /// 获取Forge启动数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>启动数据</returns>
    public static ForgeLaunchObj? GetForgeObj(this GameSettingObj obj)
    {
        return GetForgeObj(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 获取Forge启动数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>启动数据</returns>
    public static ForgeLaunchObj? GetForgeObj(string mc, string version)
    {
        var key = $"{mc}-{version}";
        if (s_forgeLaunchs.TryGetValue(key, out var temp))
        {
            return temp;
        }
        string file = Path.GetFullPath($"{BaseDir}/{Name1}/forge-{mc}-{version}.json");

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<ForgeLaunchObj>(File.ReadAllText(file))!;
        s_forgeLaunchs.Add(key, obj);
        return obj;
    }

    /// <summary>
    /// 获取Fabric数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>数据</returns>
    public static FabricLoaderObj? GetFabricObj(this GameSettingObj obj)
    {
        return GetFabricObj(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 获取Fabric数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">fabric版本</param>
    /// <returns>数据</returns>
    public static FabricLoaderObj? GetFabricObj(string mc, string version)
    {
        var key = $"{mc}-{version}";
        if (s_fabricLoaders.TryGetValue(key, out var temp))
        {
            return temp;
        }
        string file = $"{BaseDir}/{Name2}/fabric-loader-{mc}-{version}.json";

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<FabricLoaderObj>(File.ReadAllText(file))!;
        s_fabricLoaders.Add(key, obj);
        return obj;
    }

    /// <summary>
    /// 获取Quilt数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>数据</returns>
    public static QuiltLoaderObj? GetQuiltObj(this GameSettingObj obj)
    {
        return GetQuiltObj(obj.Version, obj.LoaderVersion!);
    }

    /// <summary>
    /// 获取Quilt数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">quilt版本</param>
    /// <returns>数据</returns>
    public static QuiltLoaderObj? GetQuiltObj(string mc, string version)
    {
        var key = $"{mc}-{version}";
        if (s_quiltLoaders.TryGetValue(key, out var temp))
        {
            return temp;
        }
        string file = $"{BaseDir}/{Name3}/quilt-loader-{mc}-{version}.json";

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<QuiltLoaderObj>(File.ReadAllText(file))!;
        s_quiltLoaders.Add(key, obj);
        return obj;
    }
}
