using ColorMC.Core.Net;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.LaunchPath;

public static class AssetsPath
{
    private readonly static Dictionary<string, AssetsObj?> Assets = new();

    public const string Name = "assets";

    public const string Name1 = "indexes";
    public const string Name2 = "objects";
    public const string Name3 = "skins";

    public static string BaseDir { get; private set; }

    public static string ObjectsDir { get; private set; }

    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;
        ObjectsDir = BaseDir + "/" + Name2;

        Logs.Info(LanguageHelper.GetName("Core.Path.Assets.Load"));

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

    public static List<(string Name, string Hash)> Check(AssetsObj obj)
    {
        var list = new List<(string, string)>();
        foreach (var item in obj.objects)
        {
            string file = $"{ObjectsDir}/{item.Value.hash[..2]}/{item.Value.hash}";
            if (!File.Exists(file))
            {
                list.Add((item.Key, item.Value.hash));
                continue;
            }
            using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            var sha1 = Funtcions.GenSha1(stream);
            if (item.Value.hash != sha1)
            {
                list.Add((item.Key, item.Value.hash));
            }
        }

        return list;
    }

    /// <summary>
    /// ????json
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
    }
}