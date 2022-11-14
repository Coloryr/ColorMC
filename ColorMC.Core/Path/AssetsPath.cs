using ColorMC.Core.Objs.Game;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using ColorMC.Core.Http;

namespace ColorMC.Core.Path;

public static class AssetsPath
{
    private readonly static Dictionary<string, AssetsObj> Assets = new();

    private const string Name = "assets";

    private const string Name1 = "indexes";
    private const string Name2 = "objects";
    private const string Name3 = "skins";

    public static string BaseDir { get; private set; }

    public static string ObjectsDir { get; private set; }

    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;
        ObjectsDir = BaseDir + "/" + Name2;

        Directory.CreateDirectory(BaseDir);

        Directory.CreateDirectory($"{BaseDir}/{Name1}");
        Directory.CreateDirectory($"{BaseDir}/{Name2}");
        Directory.CreateDirectory($"{BaseDir}/{Name3}");

        DirectoryInfo info = new($"{BaseDir}/{Name1}");
        foreach (var item in info.GetFiles())
        {
            var data = File.ReadAllText(item.FullName);

            var obj = JsonConvert.DeserializeObject<AssetsObj>(data);
            Assets.Add(item.Name.Replace(".json", ""), obj);
        }
    }

    public static void AddIndex(AssetsObj? obj, GameArgObj game)
    {
        if (obj == null)
            return;
        string file = $"{BaseDir}/{Name1}/{game.assets}.json";
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));
        if (Assets.ContainsKey(game.assets))
        {
            Assets[game.assets] = obj;
        }
        else
        {
            Assets.Add(game.assets, obj);
        }
    }

    public static AssetsObj? GetIndex(GameArgObj game)
    {
        if (Assets.TryGetValue(game.assets, out var item))
        {
            return item;
        }

        return null;
    }

    public static List<string> Check(AssetsObj obj) 
    {
        var list = new List<string>();
        foreach (var item in obj.objects)
        {
            string file = $"{ObjectsDir}/{item.Value.hash[..2]}/{item.Value.hash}";
            if (!File.Exists(file))
            {
                list.Add(item.Value.hash);
                continue;
            }
            using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            var sha1 = Sha1.GenSha1(stream);
            if (item.Value.hash != sha1)
            {
                list.Add(item.Value.hash);
            }
        }

        return list;
    }

    /// <summary>
    /// ¸üÐÂjson
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static async Task CheckUpdate(string version) 
    {
        var item = VersionPath.GetGame(version);
        if (item == null)
        {
            return;
        }

        var obj = await Get.GetAssets(item.assetIndex.url);
        if (obj == null)
            return;
        AddIndex(obj, item);
        Check(obj);
    }
}