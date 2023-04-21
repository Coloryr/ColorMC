using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace ColorMC.Core.LaunchPath;

public static class AssetsPath
{
    public const string Name = "assets";

    public const string Name1 = "indexes";
    public const string Name2 = "objects";
    public const string Name3 = "skins";

    /// <summary>
    /// 基础路径
    /// </summary>
    public static string BaseDir { get; private set; } = "";

    /// <summary>
    /// 资源文件路径
    /// </summary>
    public static string ObjectsDir { get; private set; } = "";

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行目录</param>
    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;
        ObjectsDir = BaseDir + "/" + Name2;

        Logs.Info(LanguageHelper.GetName("Core.Pack.Info1"));

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
    public static void AddIndex(this GameArgObj game, AssetsObj? obj)
    {
        if (obj == null)
            return;

        string file = Path.GetFullPath($"{BaseDir}/{Name1}/{game.assets}.json");
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));
    }

    /// <summary>
    /// 获取资源数据
    /// </summary>
    /// <param name="game">游戏数据</param>
    /// <returns></returns>
    public static AssetsObj? GetIndex(this GameArgObj game)
    {
        string file = Path.GetFullPath($"{BaseDir}/{Name1}/{game.assets}.json");
        if (!File.Exists(file))
            return null;

        var data = File.ReadAllText(file);
        return JsonConvert.DeserializeObject<AssetsObj>(data);
    }

    /// <summary>
    /// 检查丢失的资源
    /// </summary>
    /// <param name="obj">资源数据</param>
    /// <returns>丢失列表</returns>
    public static async Task<ConcurrentBag<(string Name, string Hash)>> Check(this AssetsObj obj)
    {
        var list1 = new ConcurrentBag<string>();
        var list = new ConcurrentBag<(string, string)>();
        await Parallel.ForEachAsync(obj.objects, async (item, cancel) =>
        {
            if (list1.Contains(item.Value.hash))
                return;

            string file = $"{ObjectsDir}/{item.Value.hash[..2]}/{item.Value.hash}";
            if (!File.Exists(file))
            {
                list.Add((item.Key, item.Value.hash));
                return;
            }
            using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            var sha1 = await Funtcions.GenSha1Async(stream);
            if (item.Value.hash != sha1)
            {
                list.Add((item.Key, item.Value.hash));
                list1.Add(item.Value.hash);
            }

            return;
        });

        return list;
    }

    /// <summary>
    /// 检查资源文件
    /// </summary>
    /// <param name="version">游戏版本</param>
    public static async Task Check(string version)
    {
        var item = VersionPath.GetGame(version);
        if (item == null)
        {
            return;
        }

        var obj = await GameJsonObj.GetAssets(item.assetIndex.url);
        if (obj == null)
            return;
        item.AddIndex(obj);
    }

    /// <summary>
    /// 保存皮肤
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <param name="file">保存路径</param>
    public static void SaveSkin(this LoginObj obj, string file)
    {
        var path = obj.GetSkinFile();

        if (File.Exists(path))
            File.Delete(path);
        var info = new FileInfo(path);
        info.Directory?.Create();
        File.Copy(file, path);
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
}