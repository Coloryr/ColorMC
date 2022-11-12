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

    private static Dictionary<string, GameSetting> Games = new();

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
                var game = JsonConvert.DeserializeObject<GameSetting>(data1);
                if (game != null)
                {
                    Games.Add(game.Name, game);
                }
            }
        }
    }

    public static List<GameSetting> GetGames() 
    {
        return new(Games.Values);
    }

    public static GameSetting? GetGame(string name)
    {
        if (Games.TryGetValue(name, out var item))
        {
            return item;
        }

        return null;
    }

    public static GameSetting? CreateVersion(string name, string version,
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

        var game = new GameSetting()
        {
            Dir = dir,
            Name = name,
            Version = version,
            JvmArgs = "",
            Loader = loader,
            LoaderInfo = info
        };

        var file = dir + "/" + Name1;
        File.WriteAllText(file, JsonConvert.SerializeObject(game));
        Games.Add(name, game);

        return game;
    }

    public static string GetDir(GameSetting obj)
    {
        return obj.Dir + "/" + Name2;
    }

    public static Task InstallForge(GameSetting obj, string version) 
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

    public static Task InstallFabric(GameSetting obj, string version)
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

    public static void Uninstall(GameSetting obj)
    {
        obj.LoaderInfo = null;
        obj.Loader = Loaders.Normal;

        var file = obj.Dir + "/" + Name1;
        File.WriteAllText(file, JsonConvert.SerializeObject(obj));
    }

    public static async Task<List<DownloadItem>?> CheckGameFile(GameSetting obj) 
    {
        var list = new List<DownloadItem>();
        var game = VersionPath.GetGame(obj.Version);
        if (game == null)
            return null;

        string file =  $"{VersionPath.BaseDir}/{obj.Version}.jar";
        if (!File.Exists(file))
        {
            list.Add(new()
            {
                Url = game.downloads.client.url,
                SHA1 = game.downloads.client.sha1,
                Local = file,
                Name = $"{obj.Version}.jar"
            });
        }
        else
        {
            using FileStream stream2 = new(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            stream2.Seek(0, SeekOrigin.Begin);
            string sha1 = Sha1.GenSha1(stream2);
            if (sha1 != game.downloads.client.sha1)
            {
                list.Add(new()
                {
                    Url = game.downloads.client.url,
                    SHA1 = game.downloads.client.sha1,
                    Local = file,
                    Name = $"{obj.Version}.jar"
                });
            }
        }

        var assets = AssetsPath.GetIndex(obj.Version);
        if (assets == null)
            return null;

        var list1 = AssetsPath.Check(assets);
        foreach (var item in list1)
        {
            list.Add(new()
            {
                Overwrite = true,
                Url = UrlHelp.DownloadAssets(item, BaseClient.Source),
                SHA1 = item,
                Local = $"{AssetsPath.ObjectsDir}/{item[..2]}/{item}",
                Name = item
            });
        }

        var list2 = LibrariesPath.Check(game);
        foreach (var item in list2)
        {
            list.Add(new()
            {
                Overwrite = true,
                Url = UrlHelp.DownloadLibraries(item.downloads.artifact.url, BaseClient.Source),
                SHA1 = item.downloads.artifact.sha1,
                Local = $"{LibrariesPath.BaseDir}/{item.downloads.artifact.path}",
                Name = item.name
            });
        }

        if (obj.Loader == Loaders.Forge)
        {
            var list3 = LibrariesPath.CheckForge(obj);
            if (list3 == null)
            {
                if (CoreMain.LostModLoader?.Invoke(obj.LoaderInfo) == true)
                {
                    await GameDownload.DownloadForge(obj.Version, obj.LoaderInfo.Version);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                foreach (var item in list3)
                {
                    if (item.name.StartsWith("net.minecraftforge:forge:"))
                    {
                        list.Add(new()
                        {
                            Url = UrlHelp.DownloadForgeJar(obj.Version, obj.LoaderInfo.Version, BaseClient.Source),
                            Name = item.name,
                            Local = $"{LibrariesPath.BaseDir}/net/minecraftforge/forge/" +
                                PathC.MakeForgeName(obj.Version, obj.LoaderInfo.Version),
                            SHA1 = item.downloads.artifact.sha1
                        });
                    }
                    else
                    {
                        list.Add(new()
                        {
                            Url = UrlHelp.DownloadForgeLib(item.downloads.artifact.url,
                                BaseClient.Source),
                            Name = item.name,
                            Local = $"{LibrariesPath.BaseDir}/{item.downloads.artifact.path}",
                            SHA1 = item.downloads.artifact.sha1
                        });
                    }
                }
            }
        }
        else if (obj.Loader == Loaders.Fabric)
        {
            var list3 = LibrariesPath.CheckFabric(obj);
            if (list3 == null)
            {
                if (CoreMain.LostModLoader?.Invoke(obj.LoaderInfo) == true)
                {
                    await GameDownload.DownloadFabric(obj.Version, obj.LoaderInfo.Version);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                foreach (var item in list3)
                {
                    var name = PathC.ToName(item.name);
                    list.Add(new()
                    {
                        Url = UrlHelp.DownloadFabric(BaseClient.Source) + name.Item1,
                        Name = name.Item2,
                        Local = $"{LibrariesPath.BaseDir}/{name.Item1}"
                    });
                }
            }
        }

        return list;
    }
}