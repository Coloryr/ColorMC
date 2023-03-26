using ColorMC.Core.Net.Download;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Text;

namespace ColorMC.Core.LaunchPath;

public static class InstancesPath
{
    private const string Name = "instances";
    private const string Name1 = "game.json";
    private const string Name2 = ".minecraft";
    private const string Name3 = "modinfo.json";
    private const string Name4 = "modpack.json";
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
    private const string Name15 = "config";
    private const string Name16 = "modfileinfo.json";
    private const string Name17 = "logs/latest.log";
    private const string Name18 = "schematics";
    private const string Name19 = "remove";
    private const string Name20 = "backup";
    private const string Name21 = "server.json";
    private const string Name22 = "server.old.json";

    /// <summary>
    /// 游戏实例列表
    /// </summary>
    private static Dictionary<string, GameSettingObj> InstallGames = new();
    /// <summary>
    /// 游戏实例组
    /// </summary>
    private static Dictionary<string, List<GameSettingObj>> GameGroups = new();

    /// <summary>
    /// 基础路径
    /// </summary>
    public static string BaseDir { get; private set; }

    /// <summary>
    /// 获取所有游戏实例
    /// </summary>
    public static List<GameSettingObj> Games
    {
        get
        {
            return new(InstallGames.Values);
        }
    }

    /// <summary>
    /// 获取所有游戏实例组
    /// </summary>
    public static Dictionary<string, List<GameSettingObj>> Groups
    {
        get
        {
            return new(GameGroups);
        }
    }

    public static bool IsNotGame
        => InstallGames.Count == 0;

    private static void AddToGroup(GameSettingObj obj)
    {
        while (string.IsNullOrWhiteSpace(obj.UUID)
            || InstallGames.ContainsKey(obj.UUID))
        {
            obj.UUID = Guid.NewGuid().ToString();
            obj.Save();
        }
        InstallGames.Add(obj.UUID, obj);

        if (string.IsNullOrWhiteSpace(obj.GroupName))
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
                var list = new List<GameSettingObj>()
                {
                    obj
                };
                GameGroups.Add(obj.GroupName, list);
            }
        }
    }

    /// <summary>
    /// 从组中删除游戏实例
    /// </summary>
    /// <param name="obj"></param>
    private static void RemoveFromGroup(GameSettingObj obj)
    {
        InstallGames.Remove(obj.UUID);

        if (string.IsNullOrEmpty(obj.GroupName))
        {
            GameGroups[" "].Remove(obj);
        }
        else if (GameGroups.TryGetValue(obj.GroupName, out var group))
        {
            group.Remove(obj);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        BaseDir = Path.GetFullPath(dir + "/" + Name);

        Logs.Info(LanguageHelper.GetName("Core.Pack.Info2"));

        Directory.CreateDirectory(BaseDir);

        GameGroups.Add(" ", new List<GameSettingObj>());

        var list = Directory.GetDirectories(BaseDir);
        foreach (var item in list)
        {
            var file = Path.GetFullPath(item + "/" + Name1);
            GameSettingObj? game = null;
            if (!File.Exists(file))
            {
                var file1 = Path.GetFullPath(item + "/" + "mmc-pack.json");
                var file2 = Path.GetFullPath(item + "/" + "instance.cfg");
                if (File.Exists(file1) && File.Exists(file2))
                {
                    var mmc = JsonConvert.DeserializeObject<MMCObj>(File.ReadAllText(file1));
                    if (mmc == null)
                        break;

                    var mmc1 = File.ReadAllText(file2);
                    game = GameHelper.MMCToColorMC(mmc, mmc1);
                }
            }
            else
            {
                var data1 = File.ReadAllText(file);
                game = JsonConvert.DeserializeObject<GameSettingObj>(data1);
            }
            if (game != null)
            {
                var path = Path.GetFileName(item);
                if (path != game.DirName)
                {
                    game.DirName = path;
                    game.Save();
                }
                game.ReadCurseForgeMod();
                AddToGroup(game);
            }
        }
    }

    /// <summary>
    /// 获取游戏实例
    /// </summary>
    /// <param name="uuid">实例UUID</param>
    /// <returns>游戏实例</returns>
    public static GameSettingObj? GetGame(string? uuid)
    {
        if (string.IsNullOrWhiteSpace(uuid))
            return null;

        if (InstallGames.TryGetValue(uuid, out var item))
        {
            return item;
        }

        return null;
    }

    /// <summary>
    /// 保存游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void Save(this GameSettingObj obj)
    {
        ConfigSave.AddItem(new()
        {
            Name = $"game-{obj.Name}",
            Local = obj.GetGameJsonFile(),
            Obj = obj
        });
    }

    /// <summary>
    /// 获取游戏实例基础路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetBasePath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}");
    }

    /// <summary>
    /// 获取游戏实例游戏路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetGamePath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}");
    }

    /// <summary>
    /// 获取游戏实例储存文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetGameJsonFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name1}");
    }

    /// <summary>
    /// 获取游戏实例mod信息文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetModJsonFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name3}");
    }

    /// <summary>
    /// 获取游戏实例整合包文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetModPackJsonFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name4}");
    }

    /// <summary>
    /// 获取游戏实例视频选项文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetOptionsFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name5}");
    }

    /// <summary>
    /// 获取游戏实例服务器储存文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetServersFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name6}");
    }

    /// <summary>
    /// 获取游戏实例截图路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetScreenshotsPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name7}/");
    }

    /// <summary>
    /// 获取游戏实例资源包路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetResourcepacksPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name8}/");
    }

    /// <summary>
    /// 获取游戏实例光影包路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetShaderpacksPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name9}/");
    }

    public static string GetUserCacheFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name10}");
    }

    public static string GetUserNameCacheFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name11}");
    }

    /// <summary>
    /// 获取游戏实例图标文件路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetIconFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name12}");
    }

    /// <summary>
    /// 获取游戏实例mod文件路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetModsPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name13}/");
    }

    /// <summary>
    /// 获取游戏实例世界路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetSavesPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name14}/");
    }

    /// <summary>
    /// 获取结构文件路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetSchematicsPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name18}/");
    }

    /// <summary>
    /// 世界备份
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetWorldBackupPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name20}/");
    }

    /// <summary>
    /// 获取游戏实例配置文件路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetConfigPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name15}/");
    }

    /// <summary>
    /// 获取游戏实例mod数据文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetModInfoJsonFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name16}");
    }

    /// <summary>
    /// 获取游戏实例最后日志文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetLogLatestFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name2}/{Name17}");
    }

    /// <summary>
    /// 获取删除世界的文件夹
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetRemoveWorldPath(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name19}/{Name14}");
    }

    public static string GetServerPackFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name21}");
    }

    public static string GetServerPackOldFile(this GameSettingObj obj)
    {
        return Path.GetFullPath($"{BaseDir}/{obj.DirName}/{Name22}");
    }

    /// <summary>
    /// 新建游戏版本
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>结果</returns>
    public static async Task<GameSettingObj?> CreateVersion(this GameSettingObj game)
    {
        if (InstallGames.ContainsKey(game.Name))
        {
            if (ColorMCCore.GameOverwirte == null)
                return null;

            if (await ColorMCCore.GameOverwirte.Invoke(game) == false)
                return null;

            if (InstallGames.Remove(game.Name, out var temp))
            {
                await Remove(temp);
            }
        }

        game.DirName = game.Name;
        game.Mods ??= new();

        var dir = game.GetBasePath();
        if (Directory.Exists(dir))
        {
            return null;
        }

        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(game.GetGamePath());

        game.Save();
        AddToGroup(game);

        return game;
    }

    /// <summary>
    /// 新建游戏组
    /// </summary>
    /// <param name="name">组名字</param>
    /// <returns>结果</returns>
    public static bool AddGroup(string name)
    {
        if (GameGroups.ContainsKey(name))
        {
            return false;
        }

        GameGroups.Add(name, new());

        return true;
    }

    /// <summary>
    /// 移动游戏实例到组
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="now">组名</param>
    public static void MoveGameGroup(this GameSettingObj obj, string? now)
    {
        string group = obj.GroupName;
        if (string.IsNullOrWhiteSpace(group))
        {
            GameGroups[" "].Remove(obj);
        }
        else
        {
            var list = GameGroups[group];
            if (list.Contains(obj))
            {
                list.Remove(obj);
            }

            if (list.Count == 0)
            {
                GameGroups.Remove(group);
            }
        }

        if (string.IsNullOrWhiteSpace(now))
        {
            GameGroups[" "].Add(obj);
        }
        else
        {
            AddGroup(now);
            GameGroups[now].Add(obj);
        }

        obj.GroupName = now!;
        obj.Save();
    }

    /// <summary>
    /// 复制游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>游戏实例</returns>
    public static GameSettingObj CopyObj(this GameSettingObj obj)
    {
        return new()
        {
            Name = obj.Name,
            GroupName = obj.GroupName,
            DirName = obj.DirName,
            Version = obj.Version,
            ModPack = obj.ModPack,
            Loader = obj.Loader,
            LoaderVersion = obj.LoaderVersion,
            JvmArg = obj.JvmArg,
            JvmName = obj.JvmName,
            JvmLocal = obj.JvmLocal,
            Window = obj.Window,
            StartServer = obj.StartServer,
            ProxyHost = obj.ProxyHost,
            Mods = obj.Mods
        };
    }

    /// <summary>
    /// 复制实例
    /// </summary>
    /// <param name="obj">原始实例</param>
    /// <param name="name">新的名字</param>
    /// <returns>复制结果</returns>
    public static async Task<GameSettingObj?> Copy(this GameSettingObj obj, string name)
    {
        var obj1 = await CreateVersion(new()
        {
            Name = name,
            GroupName = obj.GroupName,
            Version = obj.Version,
            ModPack = obj.ModPack,
            Loader = obj.Loader,
            LoaderVersion = obj.LoaderVersion,
            JvmArg = obj.JvmArg,
            JvmName = obj.JvmName,
            JvmLocal = obj.JvmLocal,
            Window = obj.Window,
            StartServer = obj.StartServer,
            ProxyHost = obj.ProxyHost,
            Mods = obj.Mods
        });
        if (obj1 != null)
        {
            await PathC.CopyFiles(GetGamePath(obj), GetGamePath(obj1));
            string file = obj.GetIconFile();
            if (File.Exists(file))
            {
                File.Copy(file, obj1.GetIconFile(), true);
            }
            file = obj.GetModJsonFile();
            if (File.Exists(file))
            {
                File.Copy(file, obj1.GetModJsonFile(), true);
            }
            file = obj.GetModInfoJsonFile();
            if (File.Exists(file))
            {
                File.Copy(file, obj1.GetModInfoJsonFile(), true);
            }

            return obj1;
        }

        return null;
    }

    /// <summary>
    /// 保存游戏实例cfmod数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void SaveModInfo(this GameSettingObj obj)
    {
        if (obj.Mods == null)
            return;

        ConfigSave.AddItem(new()
        {
            Name = $"game-mod-{obj.Name}",
            Local = obj.GetModInfoJsonFile(),
            Obj = obj.Mods
        });
    }

    /// <summary>
    /// 读取游戏实例cfmod数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ReadCurseForgeMod(this GameSettingObj obj)
    {
        string file = obj.GetModInfoJsonFile();
        if (!File.Exists(file))
        {
            obj.Mods = new();
            return;
        }

        var res = JsonConvert.DeserializeObject<Dictionary<string, ModPackInfoObj>>(
            File.ReadAllText(file));
        if (res == null)
        {
            obj.Mods = new();
        }
        else
        {
            obj.Mods = res;
        }

        if (obj.ModPack)
        {
            return;
        }

        var list = PathC.GetAllFile(obj.GetModsPath());
        var remove = new List<string>();
        foreach (var item in obj.Mods)
        {
            bool find = false;
            foreach (var item1 in list)
            {
                if (item.Value.File == item1.Name)
                {
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                remove.Add(item.Key);
            }
        }

        if (remove.Count != 0)
        {
            remove.ForEach(item => obj.Mods.Remove(item));
            obj.SaveModInfo();
        }
    }

    /// <summary>
    /// 删除游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static Task Remove(this GameSettingObj obj)
    {
        RemoveFromGroup(obj);
        return PathC.DeleteFiles(obj.GetBasePath());
    }

    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">导出路径</param>
    /// <param name="filter">过滤文件</param>
    /// <param name="type">压缩包类型</param>
    public static Task Export(this GameSettingObj obj,
        string file, List<string> filter, PackType type)
    {
        switch (type)
        {
            case PackType.ColorMC:
                return ZipUtils.ZipFile(obj.GetBasePath(), file, filter);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 导入整合包
    /// </summary>
    /// <param name="dir">压缩包路径</param>
    /// <param name="type">类型</param>
    public static async Task<(bool, GameSettingObj?)> InstallFromZip(string dir, PackType type, string? name, string? group)
    {
        GameSettingObj? game = null;
        bool res1111 = false;
        try
        {
            switch (type)
            {
                //ColorMC格式
                case PackType.ColorMC:
                    {
                        ColorMCCore.PackState?.Invoke(CoreRunState.Read);
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

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            game.Name = name;
                        }

                        if (!string.IsNullOrWhiteSpace(group))
                        {
                            game.GroupName = group;
                        }

                        if (InstallGames.ContainsKey(game.Name))
                        {
                            break;
                        }

                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile)
                            {
                                using var stream = zFile.GetInputStream(e);
                                var path = game.GetBasePath();
                                string file = Path.GetFullPath(path + '\\' + e.Name);
                                FileInfo info2 = new(file);
                                info2.Directory?.Create();
                                using FileStream stream2 = new(file, FileMode.Create,
                                    FileAccess.ReadWrite, FileShare.ReadWrite);
                                await stream.CopyToAsync(stream2);
                            }
                        }

                        AddToGroup(game);

                        ColorMCCore.PackState?.Invoke(CoreRunState.End);
                        res1111 = true;
                        break;
                    }
                //Curseforge压缩包
                case PackType.CurseForge:
                    ColorMCCore.PackState?.Invoke(CoreRunState.Read);
                    var res = await PackDownload.DownloadCurseForgeModPack(dir, name, group);
                    game = res.Game;
                    if (res.State != GetDownloadState.End)
                    {
                        break;
                    }

                    ColorMCCore.PackState?.Invoke(CoreRunState.Download);
                    res1111 = await DownloadManager.Start(res.List!);

                    ColorMCCore.PackState?.Invoke(CoreRunState.End);
                    break;
                //Curseforge压缩包
                case PackType.Modrinth:
                    ColorMCCore.PackState?.Invoke(CoreRunState.Read);
                    res = await PackDownload.DownloadModrinthModPack(dir, name, group);
                    game = res.Game;
                    if (res.State != GetDownloadState.End)
                    {
                        break;
                    }

                    ColorMCCore.PackState?.Invoke(CoreRunState.Download);
                    res1111 = await DownloadManager.Start(res.List!);

                    ColorMCCore.PackState?.Invoke(CoreRunState.End);
                    break;
                //MMC压缩包
                case PackType.MMC:
                    {
                        ColorMCCore.PackState?.Invoke(CoreRunState.Read);
                        using ZipFile zFile = new(dir);
                        using var stream1 = new MemoryStream();
                        using var stream2 = new MemoryStream();
                        bool find = false;
                        bool find1 = false;
                        string path = "";
                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile && !find && e.Name.EndsWith("mmc-pack.json"))
                            {
                                using var stream = zFile.GetInputStream(e);
                                await stream.CopyToAsync(stream1);
                                path = e.Name[..^Path.GetFileName(e.Name).Length];
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

                        game = GameHelper.MMCToColorMC(mmc, mmc1);
                        game = await CreateVersion(game);

                        if (game == null)
                            break;

                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile)
                            {
                                using var stream = zFile.GetInputStream(e);
                                string file = Path.GetFullPath(game.GetBasePath() + "/" +
                    e.Name[path.Length..]);
                                FileInfo info2 = new(file);
                                info2.Directory?.Create();
                                using FileStream stream3 = new(file, FileMode.Create,
                                    FileAccess.ReadWrite, FileShare.ReadWrite);
                                await stream.CopyToAsync(stream3);
                            }
                        }

                        ColorMCCore.PackState?.Invoke(CoreRunState.End);
                        res1111 = true;
                        break;
                    }
                //HMCL压缩包
                case PackType.HMCL:
                    {
                        ColorMCCore.PackState?.Invoke(CoreRunState.Read);
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

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            game.Name = name;
                        }

                        if (!string.IsNullOrWhiteSpace(group))
                        {
                            game.GroupName = group;
                        }

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
                                string file = Path.GetFullPath(game.GetGamePath() +
                     overrides.Substring(game.Name.Length));
                                FileInfo info2 = new(file);
                                info2.Directory?.Create();
                                using FileStream stream3 = new(file, FileMode.Create,
                                    FileAccess.ReadWrite, FileShare.ReadWrite);
                                await stream.CopyToAsync(stream3);
                            }
                        }

                        ColorMCCore.PackState?.Invoke(CoreRunState.End);
                        res1111 = true;
                        break;
                    }
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Pack.Error2"), e, false);
            Logs.Error(LanguageHelper.GetName("Core.Pack.Error2"), e);
        }
        if (!res1111 && game != null)
        {
            await game.Remove();
        }
        ColorMCCore.PackState?.Invoke(CoreRunState.End);
        return (res1111, game);
    }

    public static async Task<(bool, GameSettingObj?)> InstallFromModrinth(ModrinthVersionObj data, string? name, string? group)
    {
        var file = data.files.FirstOrDefault(a => a.primary);
        if (file == null)
        {
            file = data.files[0];
        }
        var item = new DownloadItemObj()
        {
            Url = file.url,
            Name = file.filename,
            SHA1 = file.hashes.sha1,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + file.filename),
        };

        var res1 = await DownloadManager.Start(new() { item });
        if (!res1)
            return (false, null);

        return await InstallFromZip(item.Local, PackType.Modrinth, name, group);
    }

    /// <summary>
    /// 安装curseforge整合包
    /// </summary>
    /// <param name="data">整合包信息</param>
    /// <returns>结果</returns>
    public static async Task<(bool, GameSettingObj?)> InstallFromCurseForge(CurseForgeObj.Data.LatestFiles data, string? name, string? group)
    {
        data.FixDownloadUrl();

        var item = new DownloadItemObj()
        {
            Url = data.downloadUrl,
            Name = data.fileName,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + data.fileName),
        };

        var res1 = await DownloadManager.Start(new() { item });
        if (!res1)
            return (false, null);

        return await InstallFromZip(item.Local, PackType.CurseForge, name, group);
    }
}