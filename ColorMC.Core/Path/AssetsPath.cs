using ColorMC.Core.Objs.Game;
using Newtonsoft.Json;
using System;

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

    public static void AddIndex(AssetsObj obj, string version) 
    {
        string file = $"{BaseDir}/{Name1}/{version}.json";
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));
        if (Assets.ContainsKey(version))
        {
            Assets[version] = obj;
        }
        else
        {
            Assets.Add(version, obj);
        }
    }

    public static AssetsObj? GetIndex(string version) 
    {
        if (Assets.TryGetValue(version, out var item))
        {
            return item;
        }

        return null;
    }
}