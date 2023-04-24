using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.LaunchPath;

/// <summary>
/// 版本
/// </summary>
public static class VersionPath
{
    public static VersionObj? Versions { get; private set; }

    public static string ForgeDir => BaseDir + "/" + Name1;
    public static string FabricDir => BaseDir + "/" + Name2;
    public static string QuiltDir => BaseDir + "/" + Name3;


    private const string Name = "versions";

    private const string Name1 = "forge";
    private const string Name2 = "fabric";
    private const string Name3 = "quilt";

    public static string BaseDir { get; private set; } = "";

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;

        Logs.Info(LanguageHelper.GetName("Core.Path.Info2"));

        Directory.CreateDirectory(BaseDir);

        Directory.CreateDirectory(ForgeDir);
        Directory.CreateDirectory(FabricDir);
        Directory.CreateDirectory(QuiltDir);

        try
        {
            ReadVersions();
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Path.Error2"), e);
        }
    }

    /// <summary>
    /// 从在线获取版本信息
    /// </summary>
    /// <returns></returns>
    public static async Task GetFromWeb()
    {
        Versions = await GameJsonObj.GetVersions();
        if (Versions != null)
        {
            SaveVersions();
            return;
        }
        Versions = await GameJsonObj.GetVersions(SourceLocal.Offical);
        if (Versions == null)
        {
            Logs.Warn(LanguageHelper.GetName("Core.Path.Error3"));
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
            return;
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
        string file = $"{BaseDir}/{version}.json";

        if (!File.Exists(file))
            return null;

        return JsonConvert.DeserializeObject<GameArgObj>(File.ReadAllText(file));
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
            AddGame(await GameJsonObj.GetGame(data.url));
        }
    }

    /// <summary>
    /// 获取Forge安装数数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>Forge安装数据</returns>
    public static ForgeInstallObj? GetForgeInstallObj(string mc, string version)
    {
        string file = $"{BaseDir}/{Name1}/forge-{mc}-{version}-install.json";

        if (!File.Exists(file))
            return null;

        return JsonConvert.DeserializeObject<ForgeInstallObj>(File.ReadAllText(file));
    }

    /// <summary>
    /// 获取Forge启动数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>启动数据</returns>
    public static ForgeLaunchObj? GetForgeObj(this GameSettingObj obj)
    {
        return GetForgeObj(obj.Version, obj.LoaderVersion);
    }

    /// <summary>
    /// 获取Forge启动数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">forge版本</param>
    /// <returns>启动数据</returns>
    public static ForgeLaunchObj? GetForgeObj(string mc, string? version)
    {
        if (version == null)
            return null;

        string file = Path.GetFullPath($"{BaseDir}/{Name1}/forge-{mc}-{version}.json");

        if (!File.Exists(file))
            return null;

        return JsonConvert.DeserializeObject<ForgeLaunchObj>(File.ReadAllText(file));
    }

    /// <summary>
    /// 获取Fabric数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>数据</returns>
    public static FabricLoaderObj? GetFabricObj(this GameSettingObj obj)
    {
        return GetFabricObj(obj.Version, obj.LoaderVersion);
    }

    /// <summary>
    /// 获取Fabric数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">fabric版本</param>
    /// <returns>数据</returns>
    public static FabricLoaderObj? GetFabricObj(string mc, string? version)
    {
        if (version == null)
            return null;
        string file = $"{BaseDir}/{Name2}/fabric-loader-{version}-{mc}.json";

        if (!File.Exists(file))
            return null;

        return JsonConvert.DeserializeObject<FabricLoaderObj>(File.ReadAllText(file));
    }

    /// <summary>
    /// 获取Quilt数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>数据</returns>
    public static QuiltLoaderObj? GetQuiltObj(this GameSettingObj obj)
    {
        return GetQuiltObj(obj.Version, obj.LoaderVersion);
    }

    /// <summary>
    /// 获取Quilt数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">quilt版本</param>
    /// <returns>数据</returns>
    public static QuiltLoaderObj? GetQuiltObj(string mc, string? version)
    {
        if (version == null)
            return null;

        string file = $"{BaseDir}/{Name3}/quilt-loader-{version}-{mc}.json";

        if (!File.Exists(file))
            return null;

        return JsonConvert.DeserializeObject<QuiltLoaderObj>(File.ReadAllText(file));
    }
}
