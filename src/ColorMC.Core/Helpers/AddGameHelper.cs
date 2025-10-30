using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Utils;
using SharpCompress.Archives.Zip;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 添加游戏实例操作
/// </summary>
public static class AddGameHelper
{
    /// <summary>
    /// 导入文件夹
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>导入结果</returns>
    public static async Task<GameRes> AddGameFolder(AddGameFolderArg arg)
    {
        if (string.IsNullOrWhiteSpace(arg.Local))
        {
            throw new ArgumentNullException(nameof(arg.Local));
        }

        GameSettingObj? game = null;

        bool isfind = false;
        bool ismmc = false;

        //是否为MMC实例
        if (GameHelper.IsMMCVersion(arg.Local))
        {
            try
            {
                using var stream = PathHelper.OpenRead(Path.Combine(arg.Local, Names.NameMMCJsonFile));
                var mmc = JsonUtils.ToObj(stream, JsonType.MMCObj);
                if (mmc != null)
                {
                    using var stream1 = PathHelper.OpenRead(Path.Combine(arg.Local, Names.NameMMCCfgFile));
                    var mmc1 = GameOptions.ReadOptions(stream1, "=");
                    var res = mmc.ToColorMC(mmc1);
                    game = res.Game;
                    game.Icon = res.Icon + Names.NamePngExt;
                    ismmc = true;
                    isfind = true;
                }
            }
            catch
            {

            }
        }

        if (!isfind)
        {
            //是否为官启版本
            var files = Directory.GetFiles(arg.Local);
            foreach (var item in files)
            {
                if (!item.EndsWith(Names.NameJsonExt))
                {
                    continue;
                }

                try
                {
                    using var stream = PathHelper.OpenRead(item);
                    var obj1 = JsonUtils.ToObj(stream, JsonType.OfficialObj);
                    if (obj1 != null && obj1.Id != null)
                    {
                        game = obj1.ToColorMC();
                        break;
                    }
                }
                catch
                {

                }
            }
        }

        game ??= new GameSettingObj()
        {
            Version = (await GameHelper.GetGameVersionsAsync(GameType.Release))[0],
            Loader = Loaders.Normal,
            GroupName = arg.Group
        };

        //没有名字使用输入名字，已有名字同时有输入名字则覆盖
        if (string.IsNullOrWhiteSpace(game.Name))
        {
            game.Name = arg.Name ?? throw new ArgumentNullException(nameof(arg.Name));
        }
        else if (!string.IsNullOrWhiteSpace(arg.Name))
        {
            game.Name = arg.Name;
        }

        //创建游戏实例
        game = await InstancesPath.CreateGameAsync(new CreateGameArg
        {
            Game = game,
            Request = arg.Request,
            Overwirte = arg.Overwirte
        });
        if (game == null)
        {
            return new();
        }

        //复制游戏文件
        await game.CopyFileAsync(new()
        {
            Local = arg.Local,
            Unselect = arg.Unselect,
            IsDir = ismmc,
            State = arg.State
        });

        return new()
        {
            State = true,
            Game = game
        };
    }

    /// <summary>
    /// 导入Modrinth整合包
    /// </summary>
    /// <param name="arg">导入参数</param>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> ModrinthAsync(InstallZipArg arg, Stream st)
    {
        arg.Update2?.Invoke(CoreRunState.Read);
        using var zFile = ZipArchive.Open(st);
        //获取主信息
        ModrinthPackObj? info = null;
        if (zFile.Entries.FirstOrDefault(item => item.Key == Names.NameModrinthFile) is { } ent)
        {
            try
            {
                using var stream = ent.OpenEntryStream();
                info = JsonUtils.ToObj(stream, JsonType.ModrinthPackObj);
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Error49"), e);
                return new();
            }
        }
        else
        {
            return new();
        }

        if (info == null)
        {
            return new();
        }

        arg.Update2?.Invoke(CoreRunState.Init);

        //获取版本数据
        Loaders loaders = Loaders.Normal;
        string loaderversion = "";
        if (info.Dependencies.TryGetValue(Names.NameForgeKey, out var version))
        {
            loaders = Loaders.Forge;
            loaderversion = version;
        }
        else if (info.Dependencies.TryGetValue(Names.NameNeoForgeKey, out version))
        {
            loaders = Loaders.NeoForge;
            loaderversion = version;
        }
        else if (info.Dependencies.TryGetValue(Names.NameFabricLoaderKey, out version))
        {
            loaders = Loaders.Fabric;
            loaderversion = version;
        }
        else if (info.Dependencies.TryGetValue(Names.NameQuiltLoaderKey, out version))
        {
            loaders = Loaders.Quilt;
            loaderversion = version;
        }
        if (string.IsNullOrWhiteSpace(arg.Name))
        {
            arg.Name = $"{info.Name}-{info.VersionId}";
        }

        var gameversion = info.Dependencies[Names.NameMinecraftKey];
        if (VersionPath.CheckUpdateAsync(gameversion) == null)
        {
            await VersionPath.GetFromWebAsync();
            if (VersionPath.CheckUpdateAsync(gameversion) == null)
            {
                return new();
            }
        }

        //创建游戏实例
        var game = await InstancesPath.CreateGameAsync(new CreateGameArg
        {
            Game = new GameSettingObj()
            {
                GroupName = arg.Group,
                Name = arg.Name,
                Version = gameversion,
                ModPack = true,
                ModPackType = SourceType.Modrinth,
                Loader = loaders,
                LoaderVersion = loaderversion
            },
            Request = arg.Request,
            Overwirte = arg.Overwirte
        });

        if (game == null)
        {
            return new GameRes { Game = game };
        }

        int length = Names.NameOverrideDir.Length;
        string dir = Names.NameOverrideDir;

        //解压文件
        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                continue;
            }
            if (e.Key!.StartsWith(dir))
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(game.GetGamePath() + e.Key[length..]);
                file = PathHelper.ReplacePathName(file);
                await PathHelper.WriteBytesAsync(file, stream);
            }
            else
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(game.GetBasePath() + "/" + e.Key);
                file = PathHelper.ReplacePathName(file);
                await PathHelper.WriteBytesAsync(file, stream);
            }
        }

        arg.Update2?.Invoke(CoreRunState.GetInfo);

        //获取Mod信息

        var list = ModrinthHelper.GetModrinthModInfo(new GetModrinthModInfoArg
        {
            Game = game,
            Info = info,
            Update = arg.Update
        });

        game.SaveModInfo();

        arg.Update2?.Invoke(CoreRunState.Download);

        await DownloadManager.StartAsync([.. list]);

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 导入CurseForge整合包
    /// </summary>
    /// <param name="arg">导入参数</param>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> CurseForgeAsync(InstallZipArg arg, Stream st)
    {
        arg.Update2?.Invoke(CoreRunState.Read);
        using var zFile = ZipArchive.Open(st);

        //获取主信息
        CurseForgePackObj? info = null;
        if (zFile.Entries.FirstOrDefault(item => item.Key == Names.NameManifestFile) is { } ent)
        {
            try
            {
                using var stream = ent.OpenEntryStream();
                info = JsonUtils.ToObj(stream, JsonType.CurseForgePackObj);
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Error49"), e);
                return new GameRes();
            }
        }
        else
        {
            return new GameRes();
        }

        if (info == null)
        {
            return new GameRes();
        }

        arg.Update2?.Invoke(CoreRunState.Init);

        //获取版本数据
        Loaders loaders = Loaders.Normal;
        string loaderversion = "";
        foreach (var item in info.Minecraft.ModLoaders)
        {
            if (item.Id.StartsWith(Names.NameForgeKey))
            {
                loaders = Loaders.Forge;
                loaderversion = item.Id.Replace(Names.NameForgeKey + "-", "");
            }
            else if (item.Id.StartsWith(Names.NameNeoForgeKey))
            {
                loaders = Loaders.NeoForge;
                loaderversion = item.Id.Replace(Names.NameNeoForgeKey + "-", "");
            }
            else if (item.Id.StartsWith(Names.NameFabricKey))
            {
                loaders = Loaders.Fabric;
                loaderversion = item.Id.Replace(Names.NameFabricKey + "-", "");
            }
            else if (item.Id.StartsWith(Names.NameQuiltKey))
            {
                loaders = Loaders.Quilt;
                loaderversion = item.Id.Replace(Names.NameQuiltKey + "-", "");
            }
        }

        if (loaderversion.StartsWith(info.Minecraft.Version + "-")
            && loaderversion.Length > info.Minecraft.Version.Length + 1)
        {
            loaderversion = loaderversion[(info.Minecraft.Version.Length + 1)..];
        }

        if (string.IsNullOrWhiteSpace(arg.Name))
        {
            arg.Name = $"{info.Name}-{info.Version}";
        }

        var gameversion = info.Minecraft.Version;
        if (VersionPath.CheckUpdateAsync(gameversion) == null)
        {
            await VersionPath.GetFromWebAsync();
            if (VersionPath.CheckUpdateAsync(gameversion) == null)
            {
                return new GameRes();
            }
        }

        //创建游戏实例
        var game = new GameSettingObj()
        {
            GroupName = arg.Group,
            Name = arg.Name,
            Version = gameversion,
            ModPack = true,
            Loader = loaders,
            ModPackType = SourceType.CurseForge,
            LoaderVersion = loaderversion
        };

        game = await InstancesPath.CreateGameAsync(new CreateGameArg
        {
            Game = game,
            Request = arg.Request,
            Overwirte = arg.Overwirte
        });

        if (game == null)
        {
            return new GameRes();
        }

        //解压文件
        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                continue;
            }

            if (e.Key!.StartsWith(info.Overrides + "/"))
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(game.GetGamePath() + e.Key[info.Overrides.Length..]);
                file = PathHelper.ReplacePathName(file);
                await PathHelper.WriteBytesAsync(file, stream);
            }
            else
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(game.GetBasePath() + "/" + e.Key);
                file = PathHelper.ReplacePathName(file);
                await PathHelper.WriteBytesAsync(file, stream);
            }
        }

        arg.Update2?.Invoke(CoreRunState.GetInfo);

        //获取Mod信息
        var list = await CurseForgeHelper.GetModPackInfoAsync(new GetCurseForgeModInfoArg
        {
            Game = game,
            Info = info,
            Update = arg.Update
        });
        if (!list.State)
        {
            return new GameRes { Game = game };
        }

        game.SaveModInfo();

        arg.Update2?.Invoke(CoreRunState.Download);

        await DownloadManager.StartAsync([.. list.List!]);

        arg.Update2?.Invoke(CoreRunState.DownloadDone);

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 导入ColorMC压缩包
    /// </summary>
    /// <param name="arg">导入参数</param>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> ColorMCAsync(InstallZipArg arg, Stream st)
    {
        arg.Update2?.Invoke(CoreRunState.Read);
        //直接解压
        using var zFile = ZipArchive.Open(st);
        GameSettingObj? game = null;
        if (zFile.Entries.FirstOrDefault(item => item.Key == Names.NameGameFile) is { } item)
        {
            using var stream = item.OpenEntryStream();
            game = JsonUtils.ToObj(stream, JsonType.GameSettingObj);
        }

        if (game == null)
        {
            return new GameRes();
        }

        //导入实例
        game = await InstancesPath.CreateGameAsync(new CreateGameArg
        {
            Game = game,
            Request = arg.Request,
            Overwirte = arg.Overwirte
        });

        if (game == null)
        {
            return new GameRes();
        }

        //复制文件
        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                continue;
            }
            using var stream = e.OpenEntryStream();
            string file = Path.GetFullPath(game.GetBasePath() + "/" + e.Key);
            await PathHelper.WriteBytesAsync(file, stream);
        }

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 导入MMC压缩包
    /// </summary>
    /// <param name="arg">导入参数</param>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> MMCAsync(InstallZipArg arg, Stream st)
    {
        arg.Update2?.Invoke(CoreRunState.Read);
        using var zFile = ZipArchive.Open(st);
        string path = "";
        MMCObj? mmc = null;
        Dictionary<string, string>? mmc1 = null;
        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                continue;
            }
            if (mmc == null && e.Key!.EndsWith(Names.NameMMCJsonFile))
            {
                path = e.Key.Replace(Path.GetFileName(e.Key), "");
                using var stream = e.OpenEntryStream();
                mmc = JsonUtils.ToObj(stream, JsonType.MMCObj);
            }
            else if (mmc1 == null && e.Key!.EndsWith(Names.NameMMCCfgFile))
            {
                using var stream1 = e.OpenEntryStream();
                mmc1 = GameOptions.ReadOptions(stream1, "=");
            }

            if (mmc != null && mmc1 != null)
            {
                break;
            }
        }

        if (mmc == null || mmc1 == null)
        {
            return new GameRes();
        }

        var res = mmc.ToColorMC(mmc1);
        var game = res.Game;

        if (!string.IsNullOrWhiteSpace(arg.Name))
        {
            game.Name = arg.Name;
        }
        if (!string.IsNullOrWhiteSpace(arg.Group))
        {
            game.GroupName = arg.Group;
        }
        if (string.IsNullOrWhiteSpace(game.Name))
        {
            game.Name = new FileInfo(arg.Dir).Name;
        }
        if (!string.IsNullOrWhiteSpace(res.Icon))
        {
            game.Icon = res.Icon + ".png";
        }
        game = await InstancesPath.CreateGameAsync(new CreateGameArg
        {
            Game = game,
            Request = arg.Request,
            Overwirte = arg.Overwirte
        });

        if (game == null)
        {
            return new GameRes();
        }

        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                continue;
            }
            if (e.Key!.StartsWith(path))
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath($"{game.GetBasePath()}/{e.Key[path.Length..]}");
                await PathHelper.WriteBytesAsync(file, stream);
            }
        }

        game.ReadCustomJson();
        if (game.CustomJson.Count > 0)
        {
            game.CustomLoader ??= new();
            game.CustomLoader.CustomJson = true;
        }

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 导入HMCL压缩包
    /// </summary>
    /// <param name="arg">导入参数</param>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> HMCLAsync(InstallZipArg arg, Stream st)
    {
        arg.Update2?.Invoke(CoreRunState.Read);
        using var zFile = ZipArchive.Open(st);
        HMCLObj? obj = null;
        CurseForgePackObj? obj1 = null;
        if (zFile.Entries.FirstOrDefault(item => item.Key == Names.NameHMCLFile) is { } item)
        {
            using var stream = item.OpenEntryStream();
            obj = JsonUtils.ToObj(stream, JsonType.HMCLObj);
        }
        if (zFile.Entries.FirstOrDefault(item => item.Key == Names.NameManifestFile) is { } item1)
        {
            using var stream = item1.OpenEntryStream();
            obj1 = JsonUtils.ToObj(stream, JsonType.CurseForgePackObj);
        }

        if (obj == null)
        {
            return new GameRes();
        }

        var game = obj.ToColorMC();
        if (!string.IsNullOrWhiteSpace(arg.Name))
        {
            game.Name = arg.Name;
        }
        if (!string.IsNullOrWhiteSpace(arg.Group))
        {
            game.GroupName = arg.Group;
        }

        game = await InstancesPath.CreateGameAsync(new CreateGameArg
        {
            Game = game,
            Request = arg.Request,
            Overwirte = arg.Overwirte
        });

        if (game == null)
        {
            return new GameRes();
        }

        string overrides = Names.NameOverrideDir;

        if (obj1 != null)
        {
            overrides = obj1.Overrides;
        }

        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                continue;
            }
            if (e.Key!.StartsWith(overrides))
            {
                string file = Path.GetFullPath(game.GetGamePath() + e.Key[overrides.Length..]);
                if (e.Key.EndsWith(Names.NameIconFile))
                {
                    file = game.GetIconFile();
                }
                using var stream = e.OpenEntryStream();
                await PathHelper.WriteBytesAsync(file, stream);
            }
        }

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 直接解压压缩包
    /// </summary>
    /// <param name="arg">导入参数</param>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> UnzipAsync(InstallZipArg arg, Stream st)
    {
        arg.Update2?.Invoke(CoreRunState.Read);
        if (string.IsNullOrWhiteSpace(arg.Name))
        {
            arg.Name = Path.GetFileName(arg.Dir);
        }

        arg.Update2?.Invoke(CoreRunState.Start);

        var game = await InstancesPath.CreateGameAsync(new CreateGameArg
        {
            Game = new()
            {
                GroupName = arg.Group,
                Name = arg.Name!
            },
            Request = arg.Request,
            Overwirte = arg.Overwirte
        });

        if (game == null)
        {
            return new GameRes();
        }

        await new ZipProcess(zipUpdate: arg.Zip).UnzipAsync(game.GetGamePath(), arg.Dir, st!);

        //尝试解析版本号
        var files = Directory.GetFiles(game!.GetGamePath());
        foreach (var item in files)
        {
            if (!item.EndsWith(".json"))
            {
                continue;
            }

            try
            {
                using var stream = PathHelper.OpenRead(item);
                var obj1 = JsonUtils.ToObj(stream, JsonType.OfficialObj);
                if (obj1 == null || obj1.Id == null)
                {
                    continue;
                }
                var game1 = obj1.ToColorMC();
                if (game1 == null)
                {
                    continue;
                }

                game.Version = game1.Version;
                game.Loader = game1.Loader;
                game.LoaderVersion = game1.LoaderVersion;
                game.Save();
                break;
            }
            catch
            {

            }
        }

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 导入整合包
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>导入结果</returns>
    public static async Task<GameRes> InstallZip(InstallZipArg arg)
    {
        GameRes? res1 = null;
        Stream? st = null;
        try
        {
            //如果是http则下载
            if (arg.Dir.StartsWith("http"))
            {
                var file = Path.Combine(DownloadManager.DownloadDir, FuntionUtils.NewUUID());
                var res = await DownloadManager.StartAsync([new FileItemObj()
                {
                    Url = arg.Dir,
                    Overwrite = true,
                    Local = file,
                    Name = LanguageHelper.Get("Core.Info22")
                }]);
                if (!res)
                {
                    return new GameRes();
                }
                st = PathHelper.OpenRead(file);
            }
            else
            {
                st = PathHelper.OpenRead(arg.Dir);
            }
            if (st == null)
            {
                return new();
            }
            res1 = arg.Type switch
            {
                PackType.ColorMC => await ColorMCAsync(arg, st),
                PackType.CurseForge => await CurseForgeAsync(arg, st),
                PackType.Modrinth => await ModrinthAsync(arg, st),
                PackType.MMC => await MMCAsync(arg, st),
                PackType.HMCL => await HMCLAsync(arg, st),
                PackType.ZipPack => await UnzipAsync(arg, st),
                _ => null
            };

            arg.Update2?.Invoke(CoreRunState.End);

            return res1 ?? new GameRes();
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(LanguageHelper.Get("Core.Error50"), e, false);
        }
        finally
        {
            st?.Close();
            st?.Dispose();
        }
        if (res1?.State == false && res1?.Game is { } game)
        {
            await game.RemoveAsync();
        }
        arg.Update2?.Invoke(CoreRunState.End);
        return new GameRes();
    }

    /// <summary>
    /// 下载并安装Modrinth整合包
    /// </summary>
    /// <param name="arg">整合包信息</param>
    /// <returns>导入结果</returns>
    public static async Task<GameRes> InstallModrinth(DownloadModrinthArg arg)
    {
        var file = arg.Data.Files.FirstOrDefault(a => a.Primary) ?? arg.Data.Files[0];
        var item = arg.Data.MakeDownloadObj(Path.Combine(DownloadManager.DownloadDir, file.Filename));

        var res1 = await DownloadManager.StartAsync([item]);
        if (!res1)
        {
            return new GameRes();
        }

        var res2 = await InstallZip(new InstallZipArg
        {
            Dir = item.Local,
            Type = PackType.Modrinth,
            Name = arg.Name,
            Group = arg.Group,
            Zip = arg.Zip,
            Request = arg.Request,
            Overwirte = arg.Overwirte,
            Update = arg.Update,
            Update2 = arg.Update2
        });
        if (res2.State)
        {
            res2.Game!.PID = arg.Data.ProjectId;
            res2.Game.FID = arg.Data.Id;
            res2.Game.Save();

            if (arg.IconUrl != null)
            {
                await res2.Game.SetGameIconFromUrlAsync(arg.IconUrl);
            }
        }

        return res2;
    }

    /// <summary>
    /// 下载并安装curseforge整合包
    /// </summary>
    /// <param name="arg">整合包信息</param>
    /// <returns>导入结果</returns>
    public static async Task<GameRes> InstallCurseForge(DownloadCurseForgeArg arg)
    {
        var item = arg.Data.MakeDownloadObj(Path.Combine(DownloadManager.DownloadDir, arg.Data.FileName));

        var res1 = await DownloadManager.StartAsync([item]);
        if (!res1)
        {
            return new GameRes();
        }

        var res2 = await InstallZip(new InstallZipArg
        {
            Dir = item.Local,
            Type = PackType.CurseForge,
            Name = arg.Name,
            Group = arg.Group,
            Zip = arg.Zip,
            Request = arg.Request,
            Overwirte = arg.Overwirte,
            Update = arg.Update,
            Update2 = arg.Update2
        });
        if (res2.State)
        {
            res2.Game!.PID = arg.Data.ModId.ToString();
            res2.Game.FID = arg.Data.Id.ToString();
            res2.Game.Save();

            if (arg.IconUrl != null)
            {
                await res2.Game.SetGameIconFromUrlAsync(arg.IconUrl);
            }
        }

        return res2;
    }
}
