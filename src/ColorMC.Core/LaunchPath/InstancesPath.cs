using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Config;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.LaunchPath;

/// <summary>
/// 游戏实例路径
/// </summary>
public static class InstancesPath
{
    /// <summary>
    /// 禁用文件夹监视
    /// </summary>
    public static bool DisableWatcher { private get; set; }

    /// <summary>
    /// 获取所有游戏实例
    /// </summary>
    public static List<GameSettingObj> Games => [.. s_installGames.Values];

    /// <summary>
    /// 获取所有游戏实例组
    /// </summary>
    public static Dictionary<string, List<GameSettingObj>> Groups => new(s_gameGroups);

    /// <summary>
    /// 是否没有游戏实例
    /// </summary>
    public static bool IsNotGame => s_installGames.Count == 0;

    private static FileSystemWatcher s_systemWatcher;

    private static string s_baseDir;
    private static bool s_init;
    private static bool s_change;
    private static int s_delay;

    private static readonly object s_lock = new();

    /// <summary>
    /// 游戏实例列表
    /// </summary>
    private static readonly Dictionary<string, GameSettingObj> s_installGames = [];
    /// <summary>
    /// 游戏实例组
    /// </summary>
    private static readonly Dictionary<string, List<GameSettingObj>> s_gameGroups = [];

    /// <summary>
    /// 添加游戏实例到组
    /// </summary>
    /// <param name="obj">游戏实例</param>
    private static void AddToGroup(this GameSettingObj obj)
    {
        while (string.IsNullOrWhiteSpace(obj.UUID)
            || s_installGames.ContainsKey(obj.UUID))
        {
            obj.UUID = FuntionUtils.NewUUID();
            obj.Save();
        }
        s_installGames.Add(obj.UUID, obj);

        if (string.IsNullOrWhiteSpace(obj.GroupName))
        {
            s_gameGroups[Names.NameDefaultGroup].Add(obj);
        }
        else
        {
            if (s_gameGroups.TryGetValue(obj.GroupName, out var group))
            {
                group.Add(obj);
            }
            else
            {
                var list = new List<GameSettingObj>()
                {
                    obj
                };
                s_gameGroups.Add(obj.GroupName, list);
            }
        }

        if (!s_init)
        {
            StartChange();
        }
    }

    /// <summary>
    /// 从组中删除游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    private static void RemoveFromGroup(this GameSettingObj obj)
    {
        s_installGames.Remove(obj.UUID);

        if (string.IsNullOrEmpty(obj.GroupName) || obj.GroupName is Names.NameDefaultGroup)
        {
            s_gameGroups[Names.NameDefaultGroup].Remove(obj);
        }
        else if (s_gameGroups.TryGetValue(obj.GroupName, out var group))
        {
            group.Remove(obj);

            if (group.Count == 0)
            {
                s_gameGroups.Remove(obj.GroupName);
            }
        }

        if (!s_init)
        {
            StartChange();
        }
    }

    /// <summary>
    /// 游戏实例数量修改任务
    /// </summary>
    private static void StartChange()
    {
        lock (s_lock)
        {
            s_delay = 500;
            if (!s_change)
            {
                s_change = true;
                Task.Run(() =>
                {
                    while (s_delay != 0)
                    {
                        Thread.Sleep(s_delay);
                        lock (s_lock)
                        {
                            s_delay = 0;
                        }
                    }
                    ColorMCCore.OnInstanceChange();
                    s_change = false;
                });
            }
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        s_init = true;

        s_baseDir = Path.Combine(dir, Names.NameInstanceDir);

        Directory.CreateDirectory(s_baseDir);

        s_gameGroups.Add(Names.NameDefaultGroup, []);

        var list = Directory.GetDirectories(s_baseDir);
        foreach (var item in list)
        {
            LoadInstance(item);
        }

        s_systemWatcher = new FileSystemWatcher(s_baseDir);
        s_systemWatcher.BeginInit();
        s_systemWatcher.EnableRaisingEvents = true;
        s_systemWatcher.IncludeSubdirectories = false;
        s_systemWatcher.Created += SystemWatcher_Created;
        s_systemWatcher.Deleted += SystemWatcher_Deleted;
        s_systemWatcher.EndInit();

        s_init = false;
    }

    /// <summary>
    /// 加载游戏实例
    /// </summary>
    /// <param name="dir">游戏路径</param>
    private static void LoadInstance(string dir)
    {
        var file = Path.Combine(dir, Names.NameGameFile);
        if (!File.Exists(file))
        {
            return;
        }

        try
        {
            using var stream = PathHelper.OpenRead(file)!;
            var game = JsonUtils.ToObj(stream, JsonType.GameSettingObj);
            if (game != null)
            {
                var path = Path.GetFileName(dir);
                if (path != game.DirName)
                {
                    game.DirName = path;
                    game.Save();
                }
                game.ReadModInfo();
                game.ReadLaunchData();
                game.ReadCustomJson();
                game.AddToGroup();
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Game.Error19"), e);
        }
    }

    private static void SystemWatcher_Deleted(object sender, FileSystemEventArgs e)
    {
        if (DisableWatcher)
        {
            return;
        }

        var local = e.FullPath;

        var obj = s_installGames.Values.FirstOrDefault(item => item.DirName == e.Name);
        obj?.RemoveFromGroup();
    }

    private static void SystemWatcher_Created(object sender, FileSystemEventArgs e)
    {
        if (DisableWatcher)
        {
            return;
        }

        var local = e.FullPath;
        if (Directory.Exists(local))
        {
            var obj = s_installGames.Values.FirstOrDefault(item => item.DirName == e.Name);
            if (obj != null)
            {
                return;
            }

            LoadInstance(local);
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

        if (s_installGames.TryGetValue(uuid, out var item))
        {
            return item;
        }

        return null;
    }

    /// <summary>
    /// 从游戏名字取游戏实例
    /// </summary>
    /// <param name="name">游戏名字</param>
    /// <returns>游戏实例</returns>
    public static GameSettingObj? GetGameByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return s_installGames.Values.FirstOrDefault(a => a.Name == name);
    }

    /// <summary>
    /// 是否存在该名字的实例
    /// </summary>
    /// <param name="name">实例名字</param>
    /// <returns>是否存在</returns>
    public static bool HaveGameWithName(string name)
    {
        return s_installGames.Values.Any(item => item.Name == name);
    }

    /// <summary>
    /// 保存游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void Save(this GameSettingObj obj)
    {
        ConfigSave.AddItem(ConfigSaveObj.Build($"game-{obj.UUID}", 
            obj.GetGameJsonFile(), obj, JsonType.GameSettingObj));
    }

    /// <summary>
    /// 获取游戏实例基础路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetBasePath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName);
    }

    /// <summary>
    /// 获取游戏实例游戏路径 .minecraft
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetGamePath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir);
    }

    /// <summary>
    /// 获取游戏实例储存文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetGameJsonFile(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameFile);
    }

    /// <summary>
    /// 获取游戏实例整合包信息文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetModPackJsonFile(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameModPackFile);
    }

    /// <summary>
    /// 获取游戏实例视频选项文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetOptionsFile(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameOptionFile);
    }

    /// <summary>
    /// 获取游戏实例服务器储存文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetServersFile(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameGameServerFile);
    }

    /// <summary>
    /// 获取游戏实例截图路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetScreenshotsPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameGameScreenShotDir);
    }

    /// <summary>
    /// 获取游戏实例资源包路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetResourcepacksPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameGameResourcepackDir);
    }

    /// <summary>
    /// 获取游戏实例光影包路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetShaderpacksPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameGameShaderpackDir);
    }

    /// <summary>
    /// 获取游戏实例图标文件路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetIconFile(this GameSettingObj obj)
    {
        if (string.IsNullOrWhiteSpace(obj.Icon))
        {
            obj.Icon = Names.NameIconFile;
            obj.Save();
        }
        return Path.Combine(s_baseDir, obj.DirName, obj.Icon);
    }

    /// <summary>
    /// 获取游戏实例mod文件路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetModsPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameGameModDir);
    }

    /// <summary>
    /// 获取游戏实例世界路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetSavesPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameGameSavesDir);
    }

    /// <summary>
    /// 获取结构文件路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetSchematicsPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameGameSchematicsDir);
    }

    /// <summary>
    /// 世界备份
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetWorldBackupPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameBackupDir);
    }

    /// <summary>
    /// 获取游戏实例配置文件路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetConfigPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameGameConfigDir);
    }

    /// <summary>
    /// 获取游戏实例mod数据文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetModInfoJsonFile(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameModInfoFile);
    }

    /// <summary>
    /// 获取游戏实例最后日志路径
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetLogPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameGameLogDir);
    }

    /// <summary>
    /// 获取删除世界的文件夹
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>路径</returns>
    public static string GetRemoveWorldPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameRemoveDir, Names.NameGameSavesDir);
    }

    /// <summary>
    /// 获取服务器实例文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetServerPackFile(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameServerFile);
    }

    /// <summary>
    /// 获取旧服务器实例文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>文件路径</returns>
    public static string GetServerPackOldFile(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameServerOldFile);
    }

    /// <summary>
    /// 获取启动数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static string GetLaunchFile(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameLaunchCountFile);
    }

    /// <summary>
    /// 获取安全Log4j文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static string GetLog4jFile(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameLog4jFile);
    }

    /// <summary>
    /// 游戏临时路径
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetGameTempPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameTempDir);
    }

    /// <summary>
    /// 游戏缓存路径
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetGameCachePath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameCacheDir);
    }

    /// <summary>
    /// 游戏附加json路径
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetGameJsonPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameJsonDir);
    }

    /// <summary>
    /// 游戏附加json路径
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetGameLibPath(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameLibDir);
    }

    /// <summary>
    /// 获取自定义加载器路径
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetGameLoaderFile(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameLoaderFile);
    }

    /// <summary>
    /// 获取崩溃日志路径
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetGameCrashReports(this GameSettingObj obj)
    {
        return Path.Combine(s_baseDir, obj.DirName, Names.NameGameDir, Names.NameGameCrashLogDir);
    }

    /// <summary>
    /// 新建游戏版本
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>结果</returns>
    public static async Task<GameSettingObj?> CreateGame(CreateGameArg arg)
    {
        try
        {
            DisableWatcher = true;

            var game = arg.Game;
            //如果已经存在了相同名字的实例
            if (HaveGameWithName(game.Name))
            {
                if (arg.Overwirte == null || await arg.Overwirte(game) == false)
                {
                    if (arg.Request != null && await arg.Request(LanguageHelper.Get("Core.Game.Error20")) == false)
                    {
                        return null;
                    }
                    //替换名字
                    int a = 1;
                    string name;
                    do
                    {
                        name = game.Name + $"({a++})";
                    }
                    while (HaveGameWithName(name));

                    game.Name = name;
                }
            }

            if (string.IsNullOrWhiteSpace(game.Name))
            {
                throw new ArgumentException("Name can't be empty");
            }

            //删除旧的文件
            var value = s_installGames.Values.FirstOrDefault(item => item.DirName == game.Name);
            if (value != null
                && s_installGames.Remove(value.UUID, out var temp)
                && !await Remove(temp))
            {
                return null;
            }

            game.DirName = PathHelper.ReplaceFileName(game.Name);

            var dir = game.GetBasePath();
            await PathHelper.MoveToTrash(dir);

            try
            {
                Directory.CreateDirectory(dir);
                Directory.CreateDirectory(game.GetGamePath());
            }
            catch (Exception e)
            {
                Logs.Error(string.Format(LanguageHelper.Get("Core.Game.Error15"), game.Name), e);
                return null;
            }

            game.Mods ??= [];
            game.CustomJson ??= [];
            game.LaunchData ??= new()
            {
                AddTime = DateTime.Now,
                LastPlay = new()
            };

            game.Save();
            game.SaveLaunchData();
            game.AddToGroup();

            return game;
        }
        finally
        {
            DisableWatcher = false;
        }
    }

    /// <summary>
    /// 重读配置
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>重载后的实例</returns>
    public static GameSettingObj Reload(this GameSettingObj game)
    {
        var stream = PathHelper.OpenRead(game.GetGameJsonFile());
        var obj = JsonUtils.ToObj(stream, JsonType.GameSettingObj);
        if (obj != null)
        {
            game.RemoveFromGroup();
            obj.Name = game.Name;
            obj.UUID = game.UUID;
            obj.DirName = game.DirName;
            obj.ReadModInfo(false);
            obj.ReadLaunchData();
            obj.ReadCustomJson();
            obj.AddToGroup();
            obj.Save();
        }
        return obj ?? game;
    }

    /// <summary>
    /// 新建游戏组
    /// </summary>
    /// <param name="name">组名字</param>
    /// <returns>结果</returns>
    public static bool AddGroup(string name)
    {
        if (s_gameGroups.ContainsKey(name))
        {
            return false;
        }

        s_gameGroups.Add(name, []);

        return true;
    }

    /// <summary>
    /// 移动游戏实例到组
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="now">组名</param>
    public static void MoveGameGroup(this GameSettingObj obj, string? now)
    {
        var group = obj.GroupName;
        if (string.IsNullOrWhiteSpace(group))
        {
            s_gameGroups[Names.NameDefaultGroup].Remove(obj);
        }
        else
        {
            var list = s_gameGroups[group];
            list.Remove(obj);

            if (list.Count == 0)
            {
                s_gameGroups.Remove(group);
            }
        }

        if (string.IsNullOrWhiteSpace(now))
        {
            s_gameGroups[Names.NameDefaultGroup].Add(obj);
        }
        else
        {
            AddGroup(now);
            s_gameGroups[now].Add(obj);
        }

        obj.GroupName = now!;
        obj.Save();
    }

    /// <summary>
    /// 复制游戏实例存储
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
            JvmArg = obj.JvmArg?.Copy(),
            JvmName = obj.JvmName,
            JvmLocal = obj.JvmLocal,
            Window = obj.Window?.Copy(),
            StartServer = obj.StartServer?.Copy(),
            ProxyHost = obj.ProxyHost?.Copy(),
            AdvanceJvm = obj.AdvanceJvm?.Copy(),
            ServerUrl = obj.ServerUrl,
            Mods = new(obj.Mods),
            FID = obj.FID,
            PID = obj.PID,
            ModPackType = obj.ModPackType,
            GameType = obj.GameType,
            Icon = obj.Icon
        };
    }

    /// <summary>
    /// 复制游戏实例存储
    /// </summary>
    /// <param name="obj">目标游戏实例</param>
    /// <param name="now">复制到的游戏实例</param>
    public static void CopyObj(this GameSettingObj obj, GameSettingObj now)
    {
        now.Version = obj.Version;
        now.Loader = obj.Loader;
        now.LoaderVersion = obj.LoaderVersion;

        if (obj.JvmArg != null)
        {
            now.JvmArg = obj.JvmArg;
        }
        if (obj.Window != null)
        {
            now.Window = obj.Window;
        }
        if (obj.StartServer != null)
        {
            now.StartServer = obj.StartServer;
        }
        if (obj.AdvanceJvm != null)
        {
            now.AdvanceJvm = obj.AdvanceJvm;
        }

        now.Save();
    }

    /// <summary>
    /// 复制实例
    /// </summary>
    /// <param name="obj">原始实例</param>
    /// <param name="arg">复制参数</param>
    /// <returns>复制的实例</returns>
    public static async Task<GameSettingObj?> Copy(this GameSettingObj obj, CopyGameArg arg)
    {
        var obj1 = obj.CopyObj();
        obj1.Name = arg.Game;
        obj1 = await CreateGame(new CreateGameArg
        {
            Game = obj1,
            Request = arg.Request,
            Overwirte = arg.Overwirte
        });
        if (obj1 != null)
        {
            await PathHelper.CopyDirAsync(GetGamePath(obj), GetGamePath(obj1));
            string file = obj.GetIconFile();
            if (File.Exists(file))
            {
                PathHelper.CopyFile(file, obj1.GetIconFile());
            }
            file = obj.GetModPackJsonFile();
            if (File.Exists(file))
            {
                PathHelper.CopyFile(file, obj1.GetModPackJsonFile());
            }
            file = obj.GetModInfoJsonFile();
            if (File.Exists(file))
            {
                PathHelper.CopyFile(file, obj1.GetModInfoJsonFile());
            }

            return obj1;
        }

        return null;
    }

    /// <summary>
    /// 保存游戏实例mod数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void SaveModInfo(this GameSettingObj obj)
    {
        if (obj.Mods == null)
            return;

        ConfigSave.AddItem(ConfigSaveObj.Build($"game-mod-{obj.Name}", obj.GetModInfoJsonFile(), obj.Mods, JsonType.DictionaryStringModInfoObj));
    }

    /// <summary>
    /// 保存游戏启动数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void SaveLaunchData(this GameSettingObj obj)
    {
        if (obj.LaunchData == null)
            return;

        ConfigSave.AddItem(ConfigSaveObj.Build($"game-launch-{obj.Name}", obj.GetLaunchFile(), obj.LaunchData, JsonType.LaunchDataObj));
    }

    /// <summary>
    /// 读取游戏实例mod数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="delete">是否删除不存在的mod数据</param>
    public static void ReadModInfo(this GameSettingObj obj, bool delete = true)
    {
        string file = obj.GetModInfoJsonFile();
        if (!File.Exists(file))
        {
            obj.Mods = [];
            return;
        }

        try
        {
            using var stream = PathHelper.OpenRead(file);
            var res = JsonUtils.ToObj(stream, JsonType.DictionaryStringModInfoObj);
            if (res == null)
            {
                obj.Mods = [];
            }
            else
            {
                obj.Mods = res;
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Game.Error8"), e);
            obj.Mods = [];
            return;
        }

        if (obj.ModPack || !delete)
        {
            return;
        }

        var list = PathHelper.GetAllFiles(obj.GetModsPath());
        var remove = new List<string>();
        foreach (var item in obj.Mods)
        {
            var name = item.Value.File
                .Replace(Names.NameJarExt, "")
                .Replace(Names.NameZipExt, "");
            bool find = false;
            foreach (var item1 in list)
            {
                if (item1.Name.Contains(name))
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
    /// 读取自定义游戏启动配置
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ReadCustomJson(this GameSettingObj obj)
    {
        obj.CustomJson ??= [];
        obj.CustomJson.Clear();

        var dir = obj.GetGameJsonPath();
        if (!Directory.Exists(dir))
        {
            return;
        }

        foreach (var item in Directory.GetFiles(dir))
        {
            try
            {
                using var stream = PathHelper.OpenRead(item);
                var obj1 = JsonUtils.ToObj(stream, JsonType.CustomGameArgObj);
                if (obj1 == null)
                {
                    continue;
                }
                obj1.File = item;
                obj1.Name ??= obj1.Id;
                obj.CustomJson.Add(obj1);
            }
            catch
            {

            }
        }

        obj.CustomJson.Sort(CustomGameArgObjComparer.Instance);
    }

    /// <summary>
    /// 读取游戏运行时间
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ReadLaunchData(this GameSettingObj obj)
    {
        string file = obj.GetLaunchFile();
        if (!File.Exists(file))
        {
            obj.LaunchData = new()
            {
                LastPlay = new()
            };
            return;
        }

        try
        {
            using var stream = PathHelper.OpenRead(file);
            var res = JsonUtils.ToObj(stream, JsonType.LaunchDataObj);
            obj.LaunchData = res ?? new() { LastPlay = new() };
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Game.Error9"), e);
            obj.LaunchData = new()
            {
                LastPlay = new()
            };
        }
    }

    /// <summary>
    /// 删除游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="request">操作请求</param>
    /// <returns>是否成功删除</returns>
    public static async Task<bool> Remove(this GameSettingObj obj)
    {
        obj.RemoveFromGroup();
        return await PathHelper.MoveToTrash(obj.GetBasePath());
    }

    /// <summary>
    /// 复制游戏文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="arg">复制参数</param>
    public static async Task CopyFileAsync(this GameSettingObj obj,
        CopyGameFileArg arg)
    {
        arg.Local = Path.GetFullPath(arg.Local);
        var list = PathHelper.GetAllFiles(arg.Local);
        if (arg.Unselect != null)
        {
            list.RemoveAll(item => arg.Unselect.Contains(item.FullName));
        }
        int basel = arg.Local.Length;
        var local1 = arg.Dir ? obj.GetBasePath() : obj.GetGamePath();
        await Task.Run(() =>
        {
            int index = 0;
            foreach (var item in list)
            {
                var path = item.FullName[basel..];
                var info = new FileInfo(Path.GetFullPath(local1 + "/" + path));
                info.Directory?.Create();
                arg.State?.Invoke(info.FullName, index, list.Count);
                PathHelper.CopyFile(item.FullName, info.FullName);
                index++;
            }
        });
    }

    /// <summary>
    /// 获取自定义加载器名字
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>加载器名字</returns>
    public static async Task<string?> GetGameLoaderInfoAsync(this GameSettingObj obj)
    {
        var file = obj.GetGameLoaderFile();
        if (File.Exists(file))
        {
            var res = await GameDownloadHelper.DecodeLoaderJarAsync(obj);
            if (res == null)
            {
                return null;
            }
            var name = res.Name;

            if (obj.GetCustomLoaderObj() == null)
            {
                return null;
            }

            return name;
        }

        return null;
    }

    /// <summary>
    /// 设置游戏实例自定义加载器
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="path">自定义加载器路径</param>
    /// <returns>数据</returns>
    public static async Task<MessageRes> SetGameLoader(this GameSettingObj obj, string path)
    {
        if (!File.Exists(path))
        {
            return new() { Message = LanguageHelper.Get("Core.Game.Error16") };
        }

        var list = await GameDownloadHelper.DecodeLoaderJarAsync(obj, path);

        if (list == null)
        {
            return new() { Message = LanguageHelper.Get("Core.Game.Error17") };
        }

        var local = obj.GetGameLoaderFile();
        PathHelper.Delete(local);

        PathHelper.CopyFile(path, local);

        return new() { State = true };
    }

    /// <summary>
    /// 设置游戏实例图标
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="url">网址</param>
    /// <returns>是否成功设置</returns>
    public static async Task<bool> SetGameIconFromUrlAsync(this GameSettingObj obj, string url)
    {
        try
        {
            var data = await CoreHttpClient.GetBytesAsync(url);
            if (data.State)
            {
                PathHelper.WriteBytes(obj.GetIconFile(), data.Data!);
                ColorMCCore.OnInstanceIconChange(obj);
                return true;
            }
        }
        catch (Exception e)
        {
            Logs.Error("error set icon", e);
        }

        return false;
    }

    /// <summary>
    /// 设置游戏实例图标
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件地址</param>
    /// <returns>是否成功设置</returns>
    public static bool SetGameIconFromFile(this GameSettingObj obj, string file)
    {
        if (!File.Exists(file))
        {
            return false;
        }

        PathHelper.CopyFile(file, obj.GetIconFile());
        ColorMCCore.OnInstanceIconChange(obj);

        return true;
    }

    /// <summary>
    /// 设置游戏实例图标
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="data">图片数据</param>
    public static void SetGameIconFromBytes(this GameSettingObj obj, byte[] data)
    {
        PathHelper.WriteBytes(obj.GetIconFile(), data);
        ColorMCCore.OnInstanceIconChange(obj);
    }

    /// <summary>
    /// 设置游戏实例图标
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="data">图片数据</param>
    public static void SetGameIconFromStream(this GameSettingObj obj, byte[] data)
    {
        PathHelper.WriteBytes(obj.GetIconFile(), data);
        ColorMCCore.OnInstanceIconChange(obj);
    }
}