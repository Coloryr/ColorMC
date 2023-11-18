using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using Newtonsoft.Json;

namespace ColorMC.Core.LaunchPath;

/// <summary>
/// 游戏资源路径
/// </summary>
public static class AssetsPath
{
    public const string Name = "assets";

    public const string Name1 = "indexes";
    public const string Name2 = "objects";
    public const string Name3 = "skins";

    private readonly static Dictionary<string, AssetsObj> s_assets = new();

    /// <summary>
    /// 基础路径
    /// </summary>
    public static string BaseDir { get; private set; }

    /// <summary>
    /// 资源文件路径
    /// </summary>
    public static string ObjectsDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行目录</param>
    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;
        ObjectsDir = BaseDir + "/" + Name2;

        Directory.CreateDirectory(BaseDir);

        Directory.CreateDirectory($"{BaseDir}/{Name1}");
        Directory.CreateDirectory($"{BaseDir}/{Name2}");
        Directory.CreateDirectory($"{BaseDir}/{Name3}");
    }

    /// <summary>
    /// 添加资源数据
    /// </summary>
    /// <param name="obj">资源数据</param>
    /// <param name="game">游戏数据</param>
    public static void AddIndex(this GameArgObj game, string data)
    {
        string file = Path.GetFullPath($"{BaseDir}/{Name1}/{game.assets}.json");
        PathHelper.WriteText(file, data);
    }

    /// <summary>
    /// 获取资源数据
    /// </summary>
    /// <param name="game">游戏数据</param>
    /// <returns>资源数据</returns>
    public static AssetsObj? GetIndex(this GameArgObj game)
    {
        if (s_assets.TryGetValue(game.assets, out var temp))
        {
            return temp;
        }
        string file = Path.GetFullPath($"{BaseDir}/{Name1}/{game.assets}.json");
        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<AssetsObj>(PathHelper.ReadText(file)!)!;
        s_assets.Add(game.assets, obj);
        return obj;
    }

    /// <summary>
    /// 保存皮肤
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <param name="file">保存路径</param>
    public static void SaveSkin(this LoginObj obj, string? file)
    {
        if (file == null)
            return;
        var path = obj.GetSkinFile();

        if (File.Exists(path))
            PathHelper.Delete(path);
        var info = new FileInfo(path);
        info.Directory?.Create();
        PathHelper.CopyFile(file, path);
    }

    /// <summary>
    /// 获取皮肤文件
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns>皮肤路径</returns>
    public static string GetSkinFile(this LoginObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{Name3}/{obj.UUID}_skin.png");
    }

    /// <summary>
    /// 获取披风文件
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns>披风路径</returns>
    public static string GetCapeFile(this LoginObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{Name3}/{obj.UUID}_cape.png");
    }

    /// <summary>
    /// 获取资源数据
    /// </summary>
    /// <param name="hash">资源名</param>
    /// <returns>数据</returns>
    public static string? ReadAsset(string hash)
    {
        return PathHelper.ReadText($"{BaseDir}/{Name2}/{hash[0..2]}/{hash}");
    }
}