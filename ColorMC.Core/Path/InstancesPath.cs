using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System;

namespace ColorMC.Core.Path;

public static class InstancesPath
{
    private const string Name = "instances";
    private const string Name1 = "game.json";
    private const string Name2 = ".minecraft";

    private static Dictionary<string, GameSettingObj> Games = new();

    public static string BaseDir { get; private set; }

    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;

        Directory.CreateDirectory(BaseDir);

        var list = Directory.GetDirectories(BaseDir);
        foreach (var item in list)
        {
            var data = new DirectoryInfo(item);
            var list1 = data.GetFiles();
            var list2 = list1.Where(a => a.Name == Name1).ToList();
            if (list2.Any())
            {
                var item1 = list2.First();
                var data1 = File.ReadAllText(item1.FullName);
                var game = JsonConvert.DeserializeObject<GameSettingObj>(data1);
                if (game != null)
                {
                    Games.Add(game.Name, game);
                }
            }
        }
    }

    public static List<GameSettingObj> GetGames() 
    {
        return new(Games.Values);
    }

    public static GameSettingObj? GetGame(string name)
    {
        if (Games.TryGetValue(name, out var item))
        {
            return item;
        }

        return null;
    }

    public static GameSettingObj? CreateVersion(string name, string version,
        Loaders loader, LoaderInfo info)
    {
        if (Games.ContainsKey(name))
        {
            return null;
        }

        var dir = BaseDir + "/" + name;
        if (Directory.Exists(dir))
        {
            return null;
        }

        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(dir + "/" + Name2);

        var game = new GameSettingObj()
        {
            Dir = dir,
            Name = name,
            Version = version,
            Loader = loader,
            LoaderInfo = info
        };

        var file = dir + "/" + Name1;
        File.WriteAllText(file, JsonConvert.SerializeObject(game));
        Games.Add(name, game);

        return game;
    }

    public static string GetDir(GameSettingObj obj)
    {
        return obj.Dir + "/" + Name2;
    }

    public static Task InstallForge(GameSettingObj obj, string version) 
    {
        obj.LoaderInfo = new()
        {
            Version = version,
            Name = "forge"
        };
        obj.Loader = Loaders.Forge;

        var file = obj.Dir + "/" + Name1;
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));

        return GameDownload.DownloadForge(obj.Version, version);
    }

    public static Task InstallFabric(GameSettingObj obj, string version)
    {
        obj.LoaderInfo = new()
        {
            Version = version,
            Name = "fabric"
        };
        obj.Loader = Loaders.Fabric;

        var file = obj.Dir + "/" + Name1;
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));

        return GameDownload.DownloadFabric(obj.Version, version);
    }

    public static void Uninstall(GameSettingObj obj)
    {
        obj.LoaderInfo = null;
        obj.Loader = Loaders.Normal;

        var file = obj.Dir + "/" + Name1;
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));
    }
}