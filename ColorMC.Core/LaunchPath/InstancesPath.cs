using ColorMC.Core.Http.Download;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace ColorMC.Core.LaunchPath;

public enum PackType
{ 
    ColorMC, CurseForge, MMC, HMCL
}

public static class InstancesPath
{
    private const string Name = "instances";
    private const string Name1 = "game.json";
    private const string Name2 = ".minecraft";
    private const string Name3 = "modinfo.json";
    private const string Name4 = "manifest.json";
    private const string Name5 = "options.txt";
    private const string Name6 = "servers.dat";
    private const string Name7 = "screenshots";
    private const string Name8 = "resourcepacks";
    private const string Name9 = "shaderpacks";
    private const string Name10 = "usercache.json";
    private const string Name11 = "usernamecache.json";
    private const string Name12 = "icon.png";
    private const string Name13 = "mods";
    private const string Name14 = "saves";

    private static Dictionary<string, GameSettingObj> InstallGames = new();

    public static string BaseDir { get; private set; }

    public static List<GameSettingObj> Games
    {
        get
        {
            return new(InstallGames.Values);
        }
    }

    public static void Init(string dir)
    {
        BaseDir = Path.GetFullPath(dir + "/" + Name);

        Logs.Info($"正在读取游戏对象信息");

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
                    InstallGames.Add(game.Name, game);
                }
            }
        }
    }

    public static GameSettingObj? GetGame(string name)
    {
        if (InstallGames.TryGetValue(name, out var item))
        {
            return item;
        }

        return null;
    }

    public static void Save(this GameSettingObj obj)
    {
        File.WriteAllText(obj.GetGameJsonPath(), JsonConvert.SerializeObject(obj));
    }

    public static string GetBaseDir(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}");
    }

    public static string GetGameDir(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}");
    }

    public static string GetGameJsonPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name1}");
    }

    public static string GetModJsonFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name3}");
    }

    public static string GetModInfoJsonFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name4}");
    }

    public static string GetOptionsFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name5}");
    }

    public static string GetServersFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name6}");
    }

    public static string GetScreenshotsPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name7}");
    }

    public static string GetResourcepacksPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name8}");
    }

    public static string GetShaderpacksPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name9}");
    }

    public static string GetUserCacheFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name10}");
    }

    public static string GetUserNameCacheFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name11}");
    }

    public static string GetIconFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name12}");
    }

    public static string GetModsPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name13}");
    }

    public static string GetSavesPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name14}");
    }

    public static GameSettingObj? CreateVersion(GameSettingObj game)
    {
        if (InstallGames.ContainsKey(game.Name))
        {
            return null;
        }

        game.DirName = game.Name;

        var dir = game.GetBaseDir();
        if (Directory.Exists(dir))
        {
            return null;
        }

        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(game.GetGameDir());

        game.Save();
        InstallGames.Add(game.Name, game);

        return game;
    }

    public static Task InstallForge(this GameSettingObj obj, string version)
    {
        obj.LoaderInfo = new()
        {
            Version = version,
            Name = "forge"
        };
        obj.Loader = Loaders.Forge;
        obj.Save();

        return GameDownload.DownloadForge(obj.Version, version);
    }

    public static Task InstallFabric(this GameSettingObj obj, string version)
    {
        obj.LoaderInfo = new()
        {
            Version = version,
            Name = "fabric"
        };
        obj.Loader = Loaders.Fabric;
        obj.Save();

        return GameDownload.DownloadFabric(obj.Version, version);
    }

    public static Task InstallQuilt(this GameSettingObj obj, string version)
    {
        obj.LoaderInfo = new()
        {
            Version = version,
            Name = "quilt"
        };
        obj.Loader = Loaders.Quilt;
        obj.Save();

        return GameDownload.DownloadQuilt(obj.Version, version);
    }

    public static void Uninstall(this GameSettingObj obj)
    {
        obj.LoaderInfo = null;
        obj.Loader = Loaders.Normal;
        obj.Save();
    }

    public static async Task<GameSettingObj?> Copy(this GameSettingObj obj, string name)
    {
        var obj1 = CreateVersion(new()
        { 
            Name = name,
            Version = obj.Version,
            ModPack = obj.ModPack,
            Loader = obj.Loader,
            LoaderInfo = obj.LoaderInfo
        });
        if (obj1 != null)
        {
            await PathC.CopyFiles(GetGameDir(obj), GetGameDir(obj1));
            if (obj.ModPack)
            {
                File.Copy(obj.GetModJsonFile(), obj1.GetModJsonFile(), true);
                File.Copy(obj.GetModInfoJsonFile(), obj1.GetModInfoJsonFile(), true);
            }

            return obj1;
        }

        return null;
    }

    public static void Remove(this GameSettingObj obj)
    {
        PathC.DeleteFiles(obj.GetBaseDir());
    }

    public static async Task<bool> LoadFromZip(string dir, PackType type)
    {
        try
        {
            using ZipFile zFile = new(dir);

            switch (type)
            {
                case PackType.ColorMC:
                    {
                        using var stream1 = new MemoryStream();
                        bool find = false;
                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile && e.Name == "game.json")
                            {
                                using var stream = zFile.GetInputStream(e);
                                await stream.CopyToAsync(stream1);
                                find = true;
                                break;
                            }
                        }

                        if (!find)
                            return false;

                        var game = JsonConvert.DeserializeObject<GameSettingObj>
                            (Encoding.UTF8.GetString(stream1.ToArray()));

                        if (game == null)
                            return false;

                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile)
                            {
                                using var stream = zFile.GetInputStream(e);
                                string file = Path.GetFullPath(game.GetGameDir());
                                FileInfo info2 = new(file);
                                info2.Directory.Create();
                                using FileStream stream2 = new(file, FileMode.Create,
                                    FileAccess.ReadWrite, FileShare.ReadWrite);
                                await stream.CopyToAsync(stream2);
                            }
                        }

                        return true;
                    }
                case PackType.CurseForge:
                    var res = await PackDownload.DownloadCurseForge(dir);
                    if (res.State != DownloadState.End)
                    {
                        return false;
                    }

                    DownloadManager.Clear();
                    DownloadManager.FillAll(res.List!);
                    var res1 = await DownloadManager.Start();

                    break;
            }
        }
        catch (Exception e)
        {
            Logs.Error("导入压缩包错误", e);
        }

        return false;
    }
}