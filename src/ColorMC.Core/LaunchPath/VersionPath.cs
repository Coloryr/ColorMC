using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
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

    //版本缓存
    private readonly static Dictionary<string, GameArgObj> s_gameArgs = [];
    private readonly static Dictionary<string, ForgeInstallObj> s_forgeInstalls = [];
    private readonly static Dictionary<string, ForgeInstallObj> s_neoForgeInstalls = [];
    private readonly static Dictionary<string, ForgeLaunchObj> s_forgeLaunchs = [];
    private readonly static Dictionary<string, ForgeLaunchObj> s_neoForgeLaunchs = [];
    private readonly static Dictionary<string, FabricLoaderObj> s_fabricLoaders = [];
    private readonly static Dictionary<string, QuiltLoaderObj> s_quiltLoaders = [];
    private readonly static Dictionary<string, CustomLoaderObj> s_customLoader = [];

    private static VersionObj? _version;

    public static string BaseDir { get; private set; }

    private static string ForgeDir => BaseDir + "/" + Name1;
    private static string FabricDir => BaseDir + "/" + Name2;
    private static string QuiltDir => BaseDir + "/" + Name3;
    private static string NeoForgeDir => BaseDir + "/" + Name4;

    /// <summary>
    /// 获取游戏版本列表
    /// </summary>
    /// <returns></returns>
    public static async Task<VersionObj?> GetVersionsAsync()
    {
        try
        {
            if (_version == null)
            {
                await ReadVersionsAsync();
            }
            return _version;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Path.Error2"), e);
            return null;
        }
    }

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
    }

    /// <summary>
    /// 从在线获取版本信息
    /// </summary>
    /// <returns></returns>
    public static async Task GetFromWebAsync()
    {
        (_version, var data) = await GameAPI.GetVersions();
        if (_version != null)
        {
            SaveVersions(data!);
            return;
        }
        (_version, data) = await GameAPI.GetVersions(SourceLocal.Offical);
        if (_version == null)
        {
            Logs.Error(LanguageHelper.Get("Core.Path.Error3"));
        }
        else
        {
            SaveVersions(data!);
        }
    }

    /// <summary>
    /// 是否存在版本信息
    /// </summary>
    /// <returns>结果</returns>
    public static async Task<bool> IsHaveVersionInfoAsync()
    {
        return await GetVersionsAsync() != null;
    }

    /// <summary>
    /// 读取版本信息
    /// </summary>
    public static async Task ReadVersionsAsync()
    {
        string file = BaseDir + "/version.json";
        if (File.Exists(file))
        {
            string data = PathHelper.ReadText(file)!;
            _version = JsonConvert.DeserializeObject<VersionObj>(data);
        }
        else
        {
            await GetFromWebAsync();
        }
    }

    /// <summary>
    /// 保存版本信息
    /// </summary>
    public static void SaveVersions(string data)
    {
        string file = BaseDir + "/version.json";
        File.WriteAllText(file, data);
    }

    /// <summary>
    /// 添加版本信息
    /// </summary>
    /// <param name="obj">游戏数据</param>
    public static async Task<GameArgObj?> AddGameAsync(VersionObj.Versions obj)
    {
        var url = UrlHelper.Download(obj.url, BaseClient.Source);
        (var obj1, var data) = await GameAPI.GetGame(url);
        if (obj1 == null)
        {
            return null;
        }
        PathHelper.WriteBytes($"{BaseDir}/{obj.id}.json", data!);
        return obj1;
    }

    /// <summary>
    /// 保存Fabric-Loader信息
    /// </summary>
    /// <param name="obj">信息</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">加载器版本</param>
    public static void AddGame(FabricLoaderObj obj, string array, string mc, string version)
    {
        PathHelper.WriteText(Path.GetFullPath($"{FabricDir}/{obj.id}.json"), array);

        var key = $"{mc}-{version}";
        if (!s_fabricLoaders.TryAdd(key, obj))
        {
            s_fabricLoaders[key] = obj;
        }
    }

    /// <summary>
    /// 添加Forge启动信息
    /// </summary>
    /// <param name="obj">信息</param>
    /// <param name="array">文本</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">加载器版本</param>
    /// <param name="neo">是否为NeoForge</param>
    public static void AddGame(ForgeLaunchObj obj, byte[] array, string mc, string version, bool neo)
    {
        var v2222 = CheckHelpers.IsGameVersion1202(mc);
        string name;
        if (neo && v2222)
        {
            name = $"neoforge-{version}";
        }
        else
        {
            name = $"forge-{mc}-{version}";
        }
        PathHelper.WriteBytes($"{(neo ? NeoForgeDir : ForgeDir)}/{name}.json", array);

        var key = $"{mc}-{version}";
        if (neo)
        {
            if (!s_neoForgeLaunchs.TryAdd(key, obj))
            {
                s_neoForgeLaunchs[key] = obj;
            }
        }
        else
        {
            if (!s_forgeLaunchs.TryAdd(key, obj))
            {
                s_forgeLaunchs[key] = obj;
            }
        }
    }

    /// <summary>
    /// 添加Forge安装信息
    /// </summary>
    /// <param name="obj">信息</param>
    /// <param name="array">文本</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">加载器版本</param>
    /// <param name="neo">是否为NeoForge</param>
    public static void AddGame(ForgeInstallObj obj, byte[] array, string mc, string version, bool neo)
    {
        var v2222 = CheckHelpers.IsGameVersion1202(mc);
        string name;
        if (neo && v2222)
        {
            name = $"neoforge-{version}-install";
        }
        else
        {
            name = $"forge-{mc}-{version}-install";
        }

        PathHelper.WriteBytes($"{(neo ? NeoForgeDir : ForgeDir)}/{name}.json", array);

        var key = $"{mc}-{version}";
        if (neo)
        {
            if (!s_neoForgeInstalls.TryAdd(key, obj))
            {
                s_neoForgeInstalls[key] = obj;
            }
        }
        else
        {
            if (!s_forgeInstalls.TryAdd(key, obj))
            {
                s_forgeInstalls[key] = obj;
            }
        }
    }

    /// <summary>
    /// 添加Quilt信息
    /// </summary>
    /// <param name="obj">Quilt加载器数据</param>
    /// <param name="data">数据内容</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">加载器版本</param>
    public static void AddGame(QuiltLoaderObj obj, string data, string mc, string version)
    {
        PathHelper.WriteText(Path.GetFullPath($"{QuiltDir}/{obj.id}.json"), data);

        var key = $"{mc}-{version}";
        if (!s_quiltLoaders.TryAdd(key, obj))
        {
            s_quiltLoaders[key] = obj;
        }
    }

    /// <summary>
    /// 添加自定义加载器信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="uuid"></param>
    public static void AddGame(CustomLoaderObj obj, string uuid)
    {
        if (!s_customLoader.TryAdd(uuid, obj))
        {
            s_customLoader[uuid] = obj;
        }
    }

    /// <summary>
    /// 获取版本信息
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns>游戏数据</returns>
    public static GameArgObj? GetVersion(string version)
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

        var obj = JsonConvert.DeserializeObject<GameArgObj>(PathHelper.ReadText(file)!)!;
        s_gameArgs.Add(version, obj);
        return obj;
    }

    /// <summary>
    /// 更新json
    /// </summary>
    /// <param name="version">游戏版本</param>
    public static async Task<GameArgObj?> CheckUpdateAsync(string version)
    {
        var ver = await GetVersionsAsync();
        if (ver == null)
        {
            return null;
        }
        var data = ver.versions.Where(a => a.id == version).FirstOrDefault();
        if (data != null)
        {
            var file = $"{BaseDir}/{version}.json";
            var temp = PathHelper.OpenRead(file);
            if (temp == null)
            {
                return null;
            }
            var sha1 = await HashHelper.GenSha1Async(temp);
            temp.Close();
            if (sha1 != data.sha1)
            {
                return await AddGameAsync(data);
            }

            return GetVersion(version);
        }

        return null;
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
        if (s_neoForgeInstalls.TryGetValue(key, out var temp))
        {
            return temp;
        }

        var v2222 = CheckHelpers.IsGameVersion1202(mc);
        string file = v2222
            ? $"{BaseDir}/{Name4}/neoforge-{version}-install.json"
            : $"{BaseDir}/{Name4}/forge-{mc}-{version}-install.json";

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<ForgeInstallObj>(PathHelper.ReadText(file)!)!;
        s_neoForgeInstalls.Add(key, obj);
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

        string file = CheckHelpers.IsGameVersion1202(mc)
            ? Path.GetFullPath($"{BaseDir}/{Name4}/neoforge-{version}.json")
            : Path.GetFullPath($"{BaseDir}/{Name4}/forge-{mc}-{version}.json");

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<ForgeLaunchObj>(PathHelper.ReadText(file)!)!;
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

        var obj = JsonConvert.DeserializeObject<ForgeInstallObj>(PathHelper.ReadText(file)!)!;
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

        var obj = JsonConvert.DeserializeObject<ForgeLaunchObj>(PathHelper.ReadText(file)!)!;
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
        string file = $"{BaseDir}/{Name2}/fabric-loader-{version}-{mc}.json";

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<FabricLoaderObj>(PathHelper.ReadText(file)!)!;
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
        string file = $"{BaseDir}/{Name3}/quilt-loader-{version}-{mc}.json";

        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<QuiltLoaderObj>(PathHelper.ReadText(file)!)!;
        s_quiltLoaders.Add(key, obj);
        return obj;
    }

    /// <summary>
    /// 获取自定义加载器数据
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns></returns>
    public static CustomLoaderObj? GetCustomLoaderObj(string uuid)
    {
        if (s_customLoader.TryGetValue(uuid, out var temp))
        {
            return temp;
        }

        return null;
    }
}