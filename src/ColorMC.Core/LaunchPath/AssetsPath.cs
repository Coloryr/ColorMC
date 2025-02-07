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
    /// <summary>
    /// 基础路径
    /// </summary>
    public static string BaseDir { get; private set; }

    /// <summary>
    /// 资源文件路径
    /// </summary>
    public static string ObjectsDir { get; private set; }
    /// <summary>
    /// 索引文件路径
    /// </summary>
    public static string IndexDir { get; private set; }
    /// <summary>
    /// 皮肤文件路径
    /// </summary>
    public static string SkinDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行目录</param>
    public static void Init(string dir)
    {
        BaseDir = Path.Combine(dir, Names.NameGameAssetsDir);
        IndexDir = Path.Combine(BaseDir, Names.NameGameIndexDir);
        ObjectsDir = Path.Combine(BaseDir, Names.NameGameObjectDir);
        SkinDir = Path.Combine(BaseDir, Names.NameGameSkinDir);

        Directory.CreateDirectory(BaseDir);
        Directory.CreateDirectory(IndexDir);
        Directory.CreateDirectory(ObjectsDir);
        Directory.CreateDirectory(SkinDir);
    }

    /// <summary>
    /// 添加资源数据
    /// </summary>
    /// <param name="obj">资源数据</param>
    /// <param name="game">游戏数据</param>
    public static void AddIndex(this GameArgObj game, string data)
    {
        string file = Path.Combine(IndexDir, $"{game.Assets}.json");
        PathHelper.WriteText(file, data);
    }

    /// <summary>
    /// 获取资源数据
    /// </summary>
    /// <param name="game">游戏数据</param>
    /// <returns>资源数据</returns>
    public static AssetsObj? GetIndex(this GameArgObj game)
    {
        string file = Path.Combine(IndexDir, $"{game.Assets}.json");
        if (!File.Exists(file))
        {
            return null;
        }

        var obj = JsonConvert.DeserializeObject<AssetsObj>(PathHelper.ReadText(file)!)!;
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
        {
            return;
        }
        var path = obj.GetSkinFile();
        PathHelper.CopyFile(file, path);
    }

    /// <summary>
    /// 获取皮肤文件
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns>皮肤路径</returns>
    public static string GetSkinFile(this LoginObj obj)
    {
        return Path.Combine(SkinDir, $"{obj.UUID}_skin.png");
    }

    /// <summary>
    /// 获取披风文件
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns>披风路径</returns>
    public static string GetCapeFile(this LoginObj obj)
    {
        return Path.Combine(SkinDir, $"{obj.UUID}_cape.png");
    }

    /// <summary>
    /// 获取资源数据
    /// </summary>
    /// <param name="hash">资源名</param>
    /// <returns>数据</returns>
    public static string? ReadAssetsText(string hash)
    {
        return PathHelper.ReadText(Path.Combine(ObjectsDir, $"{hash[0..2]}", hash));
    }
}