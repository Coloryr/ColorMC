using ColorMC.Core.Game;
using ColorMC.Core.Net.Download;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
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
    private static Dictionary<string, List<GameSettingObj>> GameGroups = new();

    public static string BaseDir { get; private set; }

    public static List<GameSettingObj> Games
    {
        get
        {
            return new(InstallGames.Values);
        }
    }

    public static Dictionary<string, List<GameSettingObj>> Groups
    {
        get
        {
            return new(GameGroups);
        }
    }

    private static void AddToGroup(GameSettingObj obj)
    {
        InstallGames.Add(obj.Name, obj);

        if (string.IsNullOrEmpty(obj.GroupName))
        {
            GameGroups[" "].Add(obj);
        }
        else
        {
            if (GameGroups.TryGetValue(obj.GroupName, out var group))
            {
                group.Add(obj);
            }
            else
            {
                var list = new List<GameSettingObj>
                {
                    obj
                };
                GameGroups.Add(obj.GroupName, list);
            }
        }
    }

    private static void RemoveFromGroup(GameSettingObj obj)
    {
        InstallGames.Remove(obj.Name);

        if (string.IsNullOrEmpty(obj.GroupName))
        {
            GameGroups[" "].Remove(obj);
        }
        else if (GameGroups.TryGetValue(obj.GroupName, out var group))
        {
            group.Remove(obj);
        }
    }

    public static void Init(string dir)
    {
        BaseDir = Path.GetFullPath(dir + "/" + Name);

        Logs.Info(LanguageHelper.GetName("Core.Path.Instances.Load"));

        Directory.CreateDirectory(BaseDir);

        GameGroups.Add(" ", new List<GameSettingObj>());

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
                    AddToGroup(game);
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

    public static async Task<GameSettingObj?> CreateVersion(this GameSettingObj game)
    {
        if (InstallGames.ContainsKey(game.Name))
        {
            if (CoreMain.GameOverwirte == null)
                return null;

            if (await CoreMain.GameOverwirte.Invoke(game) == false)
                return null;

            if (InstallGames.Remove(game.Name, out var temp))
            {
                await Remove(temp);
            }
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
        AddToGroup(game);

        return game;
    }

    public static Task InstallForge(this GameSettingObj obj, string version)
    {
        obj.LoaderVersion = version;
        obj.Loader = Loaders.Forge;
        obj.Save();

        return GameDownload.DownloadForge(obj.Version, version);
    }

    public static Task InstallFabric(this GameSettingObj obj, string version)
    {
        obj.LoaderVersion = version;
        obj.Loader = Loaders.Fabric;
        obj.Save();

        return GameDownload.DownloadFabric(obj.Version, version);
    }

    public static Task InstallQuilt(this GameSettingObj obj, string version)
    {
        obj.LoaderVersion = version;
        obj.Loader = Loaders.Quilt;
        obj.Save();

        return GameDownload.DownloadQuilt(obj.Version, version);
    }

    public static void UninstallLoader(this GameSettingObj obj)
    {
        obj.LoaderVersion = null;
        obj.Loader = Loaders.Normal;
        obj.Save();
    }

    public static async Task<GameSettingObj?> Copy(this GameSettingObj obj, string name)
    {
        var obj1 = await CreateVersion(new()
        {
            Name = name,
            Version = obj.Version,
            ModPack = obj.ModPack,
            Loader = obj.Loader,
            LoaderVersion = obj.LoaderVersion
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

    public static Task Remove(this GameSettingObj obj)
    {
        RemoveFromGroup(obj);
        return PathC.DeleteFiles(obj.GetBaseDir());
    }

    public static async Task<bool> InstallFromCurseForge(CurseForgeObj.Data.LatestFiles data)
    {
        var item = PackDownload.MakeCurseForge(data);

        DownloadManager.Clear();
        DownloadManager.FillAll(new() { item });
        var res1 = await DownloadManager.Start();
        if (!res1)
            return false;

        return await InstallFromZip(item.Local, PackType.CurseForge);
    }

    public static async Task<bool> InstallFromZip(string dir, PackType type)
    {
        GameSettingObj? game = null;
        bool res1111 = false;
        try
        {
            switch (type)
            {
                case PackType.ColorMC:
                    {
                        CoreMain.PackState?.Invoke(CoreRunState.Read);
                        using ZipFile zFile = new(dir);
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
                            break;

                        game = JsonConvert.DeserializeObject<GameSettingObj>
                            (Encoding.UTF8.GetString(stream1.ToArray()));

                        if (game == null)
                            break;

                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile)
                            {
                                using var stream = zFile.GetInputStream(e);
                                string file = Path.GetFullPath(game.GetGameDir() +
                     e.Name.Substring(game.Name.Length));
                                FileInfo info2 = new(file);
                                info2.Directory.Create();
                                using FileStream stream2 = new(file, FileMode.Create,
                                    FileAccess.ReadWrite, FileShare.ReadWrite);
                                await stream.CopyToAsync(stream2);
                            }
                        }

                        CoreMain.PackState?.Invoke(CoreRunState.End);
                        res1111 = true;
                        break;
                    }
                case PackType.CurseForge:
                    CoreMain.PackState?.Invoke(CoreRunState.Read);
                    var res = await PackDownload.DownloadCurseForge(dir);
                    game = res.Game;
                    if (res.State != DownloadState.End)
                    {
                        break;
                    }

                    CoreMain.PackState?.Invoke(CoreRunState.Download);
                    DownloadManager.Clear();
                    DownloadManager.FillAll(res.List!);
                    res1111 = await DownloadManager.Start();

                    CoreMain.PackState?.Invoke(CoreRunState.End);
                    break;
                case PackType.MMC:
                    {
                        CoreMain.PackState?.Invoke(CoreRunState.Read);
                        using ZipFile zFile = new(dir);
                        using var stream1 = new MemoryStream();
                        using var stream2 = new MemoryStream();
                        bool find = false;
                        bool find1 = false;
                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile && !find && e.Name.EndsWith("mmc-pack.json"))
                            {
                                using var stream = zFile.GetInputStream(e);
                                await stream.CopyToAsync(stream1);
                                find = true;
                            }

                            if (e.IsFile && !find1 && e.Name.EndsWith("instance.cfg"))
                            {
                                using var stream = zFile.GetInputStream(e);
                                await stream.CopyToAsync(stream2);
                                find1 = true;
                            }

                            if (find && find1)
                                break;
                        }

                        if (!find || !find1)
                            break;

                        var mmc = JsonConvert.DeserializeObject<MMCObj>
                            (Encoding.UTF8.GetString(stream1.ToArray()));
                        if (mmc == null)
                            break;

                        var mmc1 = Encoding.UTF8.GetString(stream2.ToArray());
                        var list = Options.ReadOptions(mmc1, "=");
                        game = new GameSettingObj
                        {
                            Name = list["name"],
                            Loader = Loaders.Normal
                        };
                        foreach (var item in mmc.components)
                        {
                            if (item.uid == "net.minecraft")
                            {
                                game.Version = item.version;
                            }
                            else if (item.uid == "net.minecraftforge")
                            {
                                game.Loader = Loaders.Forge;
                                game.LoaderVersion = item.version;
                            }
                            else if (item.uid == "net.fabricmc.fabric-loader")
                            {
                                game.Loader = Loaders.Fabric;
                                game.LoaderVersion = item.version;
                            }
                            else if (item.uid == "org.quiltmc.quilt-loader")
                            {
                                game.Loader = Loaders.Quilt;
                                game.LoaderVersion = item.version;
                            }
                        }
                        game.JvmArg = new();
                        game.Window = new();
                        if (list.TryGetValue("JvmArgs", out var item1))
                        {
                            game.JvmArg.JvmArgs = item1;
                        }
                        if (list.TryGetValue("MaxMemAlloc", out item1)
                            && uint.TryParse(item1, out var item2))
                        {
                            game.JvmArg.MaxMemory = item2;
                        }
                        if (list.TryGetValue("MinMemAlloc", out item1)
                             && uint.TryParse(item1, out item2))
                        {
                            game.JvmArg.MaxMemory = item2;
                        }
                        if (list.TryGetValue("MinecraftWinHeight", out item1)
                            && uint.TryParse(item1, out item2))
                        {
                            game.Window.Height = item2;
                        }
                        if (list.TryGetValue("MinecraftWinWidth", out item1)
                            && uint.TryParse(item1, out item2))
                        {
                            game.Window.Width = item2;
                        }
                        if (list.TryGetValue("LaunchMaximized", out item1))
                        {
                            game.Window.FullScreen = item1 == "true";
                        }
                        if (list.TryGetValue("JoinServerOnLaunch", out item1)
                            && item1 == "true")
                        {
                            game.StartServer = new();
                            if (list.TryGetValue("JoinServerOnLaunchAddress", out item1))
                            {
                                game.StartServer.IP = item1;
                                game.StartServer.Port = 0;
                            }
                        }

                        game = await CreateVersion(game);

                        if (game == null)
                            break;

                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile)
                            {
                                using var stream = zFile.GetInputStream(e);
                                string file = Path.GetFullPath(game.GetBaseDir() +
                    e.Name.Substring(game.Name.Length));
                                FileInfo info2 = new(file);
                                info2.Directory.Create();
                                using FileStream stream3 = new(file, FileMode.Create,
                                    FileAccess.ReadWrite, FileShare.ReadWrite);
                                await stream.CopyToAsync(stream3);
                            }
                        }

                        CoreMain.PackState?.Invoke(CoreRunState.End);
                        res1111 = true;
                        break;
                    }
                case PackType.HMCL:
                    {
                        CoreMain.PackState?.Invoke(CoreRunState.Read);
                        using ZipFile zFile = new(dir);
                        using var stream1 = new MemoryStream();
                        using var stream2 = new MemoryStream();
                        bool find = false;
                        bool find1 = false;
                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile && e.Name == "mcbbs.packmeta")
                            {
                                using var stream = zFile.GetInputStream(e);
                                await stream.CopyToAsync(stream1);
                                find = true;
                            }

                            if (e.IsFile && e.Name == "manifest.json")
                            {
                                using var stream = zFile.GetInputStream(e);
                                await stream.CopyToAsync(stream2);
                                find1 = true;
                            }

                            if (find && find1)
                                break;
                        }

                        if (!find)
                            break;

                        var obj = JsonConvert.DeserializeObject<HMCLObj>
                            (Encoding.UTF8.GetString(stream1.ToArray()));

                        if (obj == null)
                            break;

                        game = new GameSettingObj()
                        {
                            Name = obj.name,
                            Loader = Loaders.Normal
                        };

                        foreach (var item in obj.addons)
                        {
                            if (item.id == "game")
                            {
                                game.Version = item.version;
                            }
                            else if (item.id == "forge")
                            {
                                game.Loader = Loaders.Forge;
                                game.LoaderVersion = item.version;
                            }
                            else if (item.id == "fabric")
                            {
                                game.Loader = Loaders.Fabric;
                                game.LoaderVersion = item.version;
                            }
                            else if (item.id == "quilt")
                            {
                                game.Loader = Loaders.Quilt;
                                game.LoaderVersion = item.version;
                            }
                        }

                        if (obj.launchInfo != null)
                        {
                            game.JvmArg = new()
                            {
                                MinMemory = obj.launchInfo.minMemory
                            };
                            string data = "";
                            foreach (var item in obj.launchInfo.launchArgument)
                            {
                                data += item + " ";
                            }
                            if (!string.IsNullOrWhiteSpace(data))
                            {
                                game.JvmArg.GameArgs = data.Trim();
                            }
                            else
                            {
                                game.JvmArg.GameArgs = null;
                            }

                            data = "";
                            foreach (var item in obj.launchInfo.javaArgument)
                            {
                                data += item + " ";
                            }
                            if (!string.IsNullOrWhiteSpace(data))
                            {
                                game.JvmArg.JvmArgs = data.Trim();
                            }
                            else
                            {
                                game.JvmArg.JvmArgs = null;
                            }
                        }

                        game = await CreateVersion(game);

                        if (game == null)
                            break;

                        var obj1 = JsonConvert.DeserializeObject<CurseForgePackObj>
                            (Encoding.UTF8.GetString(stream2.ToArray()));
                        string overrides = "overrides";
                        if (obj1 != null)
                        {
                            overrides = obj1.overrides;
                        }

                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile)
                            {
                                using var stream = zFile.GetInputStream(e);
                                string file = Path.GetFullPath(game.GetGameDir() +
                     overrides.Substring(game.Name.Length));
                                FileInfo info2 = new(file);
                                info2.Directory.Create();
                                using FileStream stream3 = new(file, FileMode.Create,
                                    FileAccess.ReadWrite, FileShare.ReadWrite);
                                await stream.CopyToAsync(stream3);
                            }
                        }

                        CoreMain.PackState?.Invoke(CoreRunState.End);
                        res1111 = true;
                        break;
                    }
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Path.Instances.Load.Error"), e);
        }
        if (!res1111)
        {
            game?.Remove();
        }
        CoreMain.PackState?.Invoke(CoreRunState.End);
        return res1111;
    }
}