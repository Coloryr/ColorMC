using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Config;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.OptiFine;
using ColorMC.Core.Utils;

namespace ColorMC.Core.LaunchPath;

/// <summary>
/// 版本路径
/// </summary>
public static class VersionPath
{
    //版本缓存
    private static readonly Dictionary<string, GameArgObj> s_gameArgs = [];
    private static readonly Dictionary<string, ForgeInstallObj> s_forgeInstalls = [];
    private static readonly Dictionary<string, ForgeInstallObj> s_neoForgeInstalls = [];
    private static readonly Dictionary<string, ForgeLaunchObj> s_forgeLaunchs = [];
    private static readonly Dictionary<string, ForgeLaunchObj> s_neoForgeLaunchs = [];
    private static readonly Dictionary<string, FabricLoaderObj> s_fabricLoaders = [];
    private static readonly Dictionary<string, QuiltLoaderObj> s_quiltLoaders = [];
    private static readonly Dictionary<string, CustomLoaderObj> s_customLoader = [];
    private static readonly Dictionary<string, OptifineObj> s_optifineLoader = [];

    private static VersionObj? _version;
    private static string s_optifineFile;

    /// <summary>
    /// 版本缓存路径
    /// </summary>
    public static string BaseDir { get; private set; }

    //加载器缓存路径
    public static string ForgeDir { get; private set; }
    public static string FabricDir { get; private set; }
    public static string QuiltDir { get; private set; }
    public static string NeoForgeDir { get; private set; }

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

    public static VersionObj? GetVersions()
    {
        return _version;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        BaseDir = Path.Combine(dir, Names.NameVersionDir);
        ForgeDir = Path.Combine(BaseDir, Names.NameForgeKey);
        NeoForgeDir = Path.Combine(BaseDir, Names.NameNeoForgedKey);
        FabricDir = Path.Combine(BaseDir, Names.NameFabricKey);
        QuiltDir = Path.Combine(BaseDir, Names.NameQuiltKey);

        Directory.CreateDirectory(BaseDir);

        Directory.CreateDirectory(ForgeDir);
        Directory.CreateDirectory(NeoForgeDir);
        Directory.CreateDirectory(FabricDir);
        Directory.CreateDirectory(QuiltDir);

        s_optifineFile = Path.Combine(BaseDir, Names.NameOptifineFile);

        LoadOptifine();
    }

    /// <summary>
    /// 读取高清修复信息
    /// </summary>
    private static void LoadOptifine()
    {
        if (File.Exists(s_optifineFile))
        {
            s_optifineLoader.Clear();
            try
            {
                using var stream = PathHelper.OpenRead(s_optifineFile);
                var obj = JsonUtils.ToObj(stream, JsonType.DictionaryStringOptifineObj);
                if (obj != null)
                {
                    foreach (var item in obj)
                    {
                        s_optifineLoader.Add(item.Key, item.Value);
                    }
                }
            }
            catch
            {

            }
        }
    }

    /// <summary>
    /// 保存高清修复信息
    /// </summary>
    private static void SaveOptifine()
    {
        ConfigSave.AddItem(ConfigSaveObj.Build(Names.NameOptifineFile, s_optifineFile, s_optifineLoader, JsonType.DictionaryStringOptifineObj));
    }

    /// <summary>
    /// 从在线获取版本信息
    /// </summary>
    /// <returns></returns>
    public static async Task GetFromWebAsync()
    {
        var res = await GameAPI.GetVersions();
        if (res != null)
        {
            _version = res.Version;
            SaveVersions(res.Text);
            return;
        }
        res = await GameAPI.GetVersions(SourceLocal.Offical);
        if (res == null)
        {
            Logs.Error(LanguageHelper.Get("Core.Path.Error3"));
        }
        else
        {
            _version = res.Version;
            SaveVersions(res.Text);
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
    private static async Task ReadVersionsAsync()
    {
        string file = Path.Combine(BaseDir, Names.NameVersionFile);
        if (File.Exists(file))
        {
            using var stream = PathHelper.OpenRead(file);
            _version = JsonUtils.ToObj(stream, JsonType.VersionObj);
        }
        else
        {
            await GetFromWebAsync();
        }
    }

    /// <summary>
    /// 保存版本信息
    /// </summary>
    public static void SaveVersions(MemoryStream data)
    {
        var file = Path.Combine(BaseDir, Names.NameVersionFile);
        PathHelper.WriteBytes(file, data);
        data.Dispose();
    }

    /// <summary>
    /// 添加版本信息
    /// </summary>
    /// <param name="obj">游戏数据</param>
    public static async Task<GameArgObj?> AddGameAsync(VersionObj.VersionsObj obj)
    {
        var url = UrlHelper.DownloadSourceChange(obj.Url, CoreHttpClient.Source);
        var res = await GameAPI.GetGame(url);
        if (res == null)
        {
            return null;
        }
        PathHelper.WriteBytes(Path.Combine(BaseDir, $"{obj.Id}.json"), res.Text);
        res.Text.Dispose();
        return res.Arg;
    }

    /// <summary>
    /// 保存Fabric-Loader信息
    /// </summary>
    /// <param name="obj">信息</param>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">加载器版本</param>
    public static void AddGame(FabricLoaderObj obj, Stream array, string mc, string version)
    {
        PathHelper.WriteBytes(Path.Combine(FabricDir, $"{obj.Id}.json"), array);

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
    public static void AddGame(ForgeLaunchObj obj, Stream array, string mc, string version, bool neo)
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
        PathHelper.WriteBytes(Path.Combine(neo ? NeoForgeDir : ForgeDir, $"{name}.json"), array);

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
    public static void AddGame(ForgeInstallObj obj, Stream array, string mc, string version, bool neo)
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

        PathHelper.WriteBytes(Path.Combine(neo ? NeoForgeDir : ForgeDir, $"{name}.json"), array);

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
    public static void AddGame(QuiltLoaderObj obj, Stream data, string mc, string version)
    {
        PathHelper.WriteBytes(Path.Combine(QuiltDir, $"{obj.Id}.json"), data);

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
    /// 添加高清修复信息
    /// </summary>
    /// <param name="obj">高清修复信息</param>
    public static void AddOptifine(OptifineObj obj)
    {
        if (!s_optifineLoader.TryAdd(obj.Version, obj))
        {
            s_optifineLoader[obj.Version] = obj;
        }
        SaveOptifine();
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
        string file = Path.Combine(BaseDir, $"{version}.json");

        if (!File.Exists(file))
        {
            return null;
        }

        using var stream = PathHelper.OpenRead(file);
        var obj = JsonUtils.ToObj(stream, JsonType.GameArgObj);
        if (obj == null)
        {
            return null;
        }
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
        var data = ver.Versions.Where(a => a.Id == version).FirstOrDefault();
        if (data != null)
        {
            var file = Path.Combine(BaseDir, $"{version}.json");
            var temp = PathHelper.OpenRead(file);
            if (temp == null)
            {
                return null;
            }
            var sha1 = await HashHelper.GenSha1Async(temp);
            temp.Close();
            if (sha1 != data.Sha1)
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
            ? Path.Combine(NeoForgeDir, $"neoforge-{version}-install.json")
            : Path.Combine(NeoForgeDir, $"forge-{mc}-{version}-install.json");

        if (!File.Exists(file))
        {
            return null;
        }

        using var stream = PathHelper.OpenRead(file);
        var obj = JsonUtils.ToObj(stream, JsonType.ForgeInstallObj);
        if (obj == null)
        {
            return null;
        }
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
            ? Path.Combine(NeoForgeDir, $"neoforge-{version}.json")
            : Path.Combine(NeoForgeDir, $"forge-{mc}-{version}.json");

        if (!File.Exists(file))
        {
            return null;
        }

        using var stream = PathHelper.OpenRead(file);
        var obj = JsonUtils.ToObj(stream, JsonType.ForgeLaunchObj);
        if (obj == null)
        {
            return null;
        }
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
        string file = Path.Combine(ForgeDir, $"forge-{mc}-{version}-install.json");
        if (s_neoForgeInstalls.TryGetValue(key, out var temp))
        {
            return temp;
        }

        if (!File.Exists(file))
        {
            return null;
        }

        using var stream = PathHelper.OpenRead(file);
        var obj = JsonUtils.ToObj(stream, JsonType.ForgeInstallObj);
        if (obj == null)
        {
            return null;
        }
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
        string file = Path.Combine(ForgeDir, $"forge-{mc}-{version}.json");

        if (!File.Exists(file))
        {
            return null;
        }

        using var stream = PathHelper.OpenRead(file);
        var obj = JsonUtils.ToObj(stream, JsonType.ForgeLaunchObj);
        if (obj == null)
        {
            return null;
        }
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
        string file = Path.Combine(FabricDir, $"fabric-loader-{version}-{mc}.json");

        if (!File.Exists(file))
        {
            return null;
        }

        using var stream = PathHelper.OpenRead(file);
        var obj = JsonUtils.ToObj(stream, JsonType.FabricLoaderObj);
        if (obj == null)
        {
            return null;
        }
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
        string file = Path.Combine(QuiltDir, $"quilt-loader-{version}-{mc}.json");

        if (!File.Exists(file))
        {
            return null;
        }

        using var stream = PathHelper.OpenRead(file);
        var obj = JsonUtils.ToObj(stream, JsonType.QuiltLoaderObj);
        if (obj == null)
        {
            return null;
        }
        s_quiltLoaders.Add(key, obj);
        return obj;
    }

    /// <summary>
    /// 获取自定义加载器数据
    /// </summary>
    /// <param name="uuid">游戏实例</param>
    /// <returns></returns>
    public static CustomLoaderObj? GetCustomLoaderObj(this GameSettingObj obj)
    {
        if (s_customLoader.TryGetValue(obj.UUID, out var temp))
        {
            return temp;
        }

        return null;
    }

    /// <summary>
    /// 获取高清修复信息
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns></returns>
    public static OptifineObj? GetOptifine(string version)
    {
        if (s_optifineLoader.TryGetValue(version, out var temp))
        {
            return temp;
        }

        return null;
    }

    /// <summary>
    /// 获取高清修复信息
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static OptifineObj? GetOptifine(this GameSettingObj obj)
    {
        if (obj.LoaderVersion == null || obj.Loader != Loaders.OptiFine)
        {
            return null;
        }

        return GetOptifine(obj.LoaderVersion);
    }
}