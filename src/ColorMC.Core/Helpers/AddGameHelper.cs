﻿using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
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
            Version = (await GameHelper.GetGameVersions(GameType.Release))[0],
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
        game = await InstancesPath.CreateGame(new CreateGameArg
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
    /// 导入整合包
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>导入结果</returns>
    public static async Task<GameRes> InstallZip(InstallZipArg arg)
    {
        GameSettingObj? game = null;
        bool import = false;
        Stream? st = null;
        try
        {
            //如果是http则下载
            if (arg.Dir.StartsWith("http"))
            {
                var file = Path.Combine(DownloadManager.DownloadDir, FuntionUtils.NewUUID());
                var res = await DownloadManager.StartAsync([new()
                {
                    Url = arg.Dir,
                    Overwrite = true,
                    Local = file,
                    Name = LanguageHelper.Get("Core.Game.Info3")
                }]);
                if (!res)
                {
                    return new();
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

            //导入ColorMC压缩包
            async Task<bool> ColorMC()
            {
                arg.Update2?.Invoke(CoreRunState.Read);
                //直接解压
                using var zFile = ZipArchive.Open(st);
                if (zFile.Entries.FirstOrDefault(item => item.Key == Names.NameGameFile) is { } item)
                {
                    using var stream = item.OpenEntryStream();
                    game = JsonUtils.ToObj(stream, JsonType.GameSettingObj);
                }

                if (game == null)
                {
                    return false;
                }

                //导入实例
                game = await InstancesPath.CreateGame(new CreateGameArg
                {
                    Game = game,
                    Request = arg.Request,
                    Overwirte = arg.Overwirte
                });

                if (game == null)
                {
                    return false;
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

                arg.Update2?.Invoke(CoreRunState.End);
                return true;
            }

            //导入MMC压缩包
            async Task<bool> MMC()
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
                    return false;
                }

                var res = mmc.ToColorMC(mmc1);
                game = res.Game;

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
                game = await InstancesPath.CreateGame(new CreateGameArg
                {
                    Game = game,
                    Request = arg.Request,
                    Overwirte = arg.Overwirte
                });

                if (game == null)
                {
                    return false;
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

                arg.Update2?.Invoke(CoreRunState.End);
                return true;
            }

            //导入HMCL压缩包
            async Task<bool> HMCL()
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
                    return false;
                }

                game = obj.ToColorMC();
                if (!string.IsNullOrWhiteSpace(arg.Name))
                {
                    game.Name = arg.Name;
                }
                if (!string.IsNullOrWhiteSpace(arg.Group))
                {
                    game.GroupName = arg.Group;
                }

                game = await InstancesPath.CreateGame(new CreateGameArg
                {
                    Game = game,
                    Request = arg.Request,
                    Overwirte = arg.Overwirte
                });

                if (game == null)
                {
                    return false;
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

                arg.Update2?.Invoke(CoreRunState.End);
                return true;
            }

            //直接解压压缩包
            async Task<bool> Unzip()
            {
                arg.Update2?.Invoke(CoreRunState.Read);
                if (string.IsNullOrWhiteSpace(arg.Name))
                {
                    arg.Name = Path.GetFileName(arg.Dir);
                }

                arg.Update2?.Invoke(CoreRunState.Start);

                game = await InstancesPath.CreateGame(new CreateGameArg
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
                    return false;
                }

                await new ZipUtils(ZipUpdate: arg.Zip).UnzipAsync(game.GetGamePath(), arg.Dir, st!);
                arg.Update2?.Invoke(CoreRunState.End);

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

                return true;
            }

            switch (arg.Type)
            {
                //ColorMC格式
                case PackType.ColorMC:
                    import = await ColorMC();
                    break;
                //Curseforge压缩包
                case PackType.CurseForge:
                    var res1 = await ModPackHelper.InstallCurseForgeModPackAsync(new InstallModPackZipArg
                    {
                        Zip = st,
                        Name = arg.Name,
                        Group = arg.Group,
                        Request = arg.Request,
                        Overwirte = arg.Overwirte,
                        Update = arg.Update,
                        Update2 = arg.Update2
                    });
                    import = res1.State;
                    game = res1.Game;

                    arg.Update2?.Invoke(CoreRunState.End);
                    break;
                //Modrinth压缩包
                case PackType.Modrinth:
                    var res2 = await ModPackHelper.InstallModrinthModPackAsync(new InstallModPackZipArg
                    {
                        Zip = st,
                        Name = arg.Name,
                        Group = arg.Group,
                        Request = arg.Request,
                        Overwirte = arg.Overwirte,
                        Update = arg.Update,
                        Update2 = arg.Update2
                    });

                    import = res2.State;
                    game = res2.Game;

                    arg.Update2?.Invoke(CoreRunState.End);
                    break;
                //MMC压缩包
                case PackType.MMC:
                    import = await MMC();
                    break;
                //HMCL压缩包
                case PackType.HMCL:
                    import = await HMCL();
                    break;
                //直接解压
                case PackType.ZipPack:
                    import = await Unzip();
                    break;
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(LanguageHelper.Get("Core.Pack.Error2"), e, false);
        }
        finally
        {
            st?.Close();
            st?.Dispose();
        }
        if (!import && game != null)
        {
            await game.Remove();
        }
        arg.Update2?.Invoke(CoreRunState.End);
        return new GameRes { State = import, Game = game };
    }

    /// <summary>
    /// 安装Modrinth整合包
    /// </summary>
    /// <param name="arg">整合包信息</param>
    /// <returns>导入结果</returns>
    public static async Task<GameRes> InstallModrinth(DownloadModrinthArg arg)
    {
        var file = arg.Data.Files.FirstOrDefault(a => a.Primary) ?? arg.Data.Files[0];
        var item = new FileItemObj()
        {
            Url = file.Url,
            Name = file.Filename,
            Sha1 = file.Hashes.Sha1,
            Local = Path.Combine(DownloadManager.DownloadDir, file.Filename),
        };

        var res1 = await DownloadManager.StartAsync([item]);
        if (!res1)
        {
            return new();
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

            if (arg.Data1.IconUrl != null)
            {
                await res2.Game.SetGameIconFromUrlAsync(arg.Data1.IconUrl);
            }
        }

        return res2;
    }

    /// <summary>
    /// 安装curseforge整合包
    /// </summary>
    /// <param name="arg">整合包信息</param>
    /// <returns>导入结果</returns>
    public static async Task<GameRes> InstallCurseForge(DownloadCurseForgeArg arg)
    {
        arg.Data.FixDownloadUrl();

        var item = new FileItemObj()
        {
            Url = arg.Data.DownloadUrl,
            Name = arg.Data.FileName,
            Local = Path.Combine(DownloadManager.DownloadDir, arg.Data.FileName),
        };

        var res1 = await DownloadManager.StartAsync([item]);
        if (!res1)
            return new GameRes { State = false };

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

            if (arg.Data1.Logo != null)
            {
                await res2.Game.SetGameIconFromUrlAsync(arg.Data1.Logo.Url);
            }
        }

        return res2;
    }
}
