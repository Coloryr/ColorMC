﻿using System.Text;
using ColorMC.Core.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

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
                var mmc = JsonConvert.DeserializeObject<MMCObj>(PathHelper.ReadText(Path.Combine(arg.Local, Names.NameMMCJsonFile))!);
                if (mmc != null)
                {
                    var mmc1 = PathHelper.ReadText(Path.Combine(arg.Local, Names.NameMMCCfgFile))!;
                    var res = mmc.ToColorMC(mmc1);
                    game = res.Game;
                    game.Icon = res.Icon + ".png";
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
                if (!item.EndsWith(".json"))
                {
                    continue;
                }

                try
                {
                    var obj1 = JsonConvert.DeserializeObject<OfficialObj>(PathHelper.ReadText(item)!);
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
            Name = arg.Name ?? throw new Exception("Name is empty"),
            Version = (await GameHelper.GetGameVersions(GameType.Release))[0],
            Loader = Loaders.Normal
        };

        if (!string.IsNullOrWhiteSpace(arg.Name))
        {
            game.Name = arg.Name;
        }

        game.GroupName = arg.Group;

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

        await game.CopyFileAsync(new()
        {
            Local = arg.Local,
            Unselect = arg.Unselect,
            Dir = ismmc,
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
                using var st1 = await CoreHttpClient.DownloadClient.GetStreamAsync(arg.Dir);
                var memoryStream = new MemoryStream();
                await st1.CopyToAsync(memoryStream);
                st = memoryStream;
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
                using var zFile = new ZipFile(st);
                using var stream1 = new MemoryStream();
                bool find = false;
                foreach (ZipEntry e in zFile)
                {
                    if (e.IsFile && e.Name == Names.NameGameFile)
                    {
                        using var stream = zFile.GetInputStream(e);
                        await stream.CopyToAsync(stream1);
                        find = true;
                        break;
                    }
                }

                if (!find)
                {
                    return false;
                }

                game = JsonConvert.DeserializeObject<GameSettingObj>
                    (Encoding.UTF8.GetString(stream1.ToArray()));

                if (game == null)
                {
                    return false;
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

                foreach (ZipEntry e in zFile)
                {
                    if (e.IsFile)
                    {
                        using var stream = zFile.GetInputStream(e);
                        var path = game.GetBasePath();
                        string file = Path.Combine(path, e.Name);
                        await PathHelper.WriteBytesAsync(file, stream);
                    }
                }

                arg.Update2?.Invoke(CoreRunState.End);
                return true;
            }

            //导入MMC压缩包
            async Task<bool> MMC()
            {
                arg.Update2?.Invoke(CoreRunState.Read);
                using var zFile = new ZipFile(st);
                using var stream1 = new MemoryStream();
                using var stream2 = new MemoryStream();
                bool find = false;
                bool find1 = false;
                string path = "";
                foreach (ZipEntry e in zFile)
                {
                    if (e.IsFile && !find && e.Name.EndsWith(Names.NameMMCJsonFile))
                    {
                        using var stream = zFile.GetInputStream(e);
                        await stream.CopyToAsync(stream1);
                        path = e.Name[..^Path.GetFileName(e.Name).Length];
                        find = true;
                    }

                    if (e.IsFile && !find1 && e.Name.EndsWith(Names.NameMMCCfgFile))
                    {
                        using var stream = zFile.GetInputStream(e);
                        await stream.CopyToAsync(stream2);
                        find1 = true;
                    }

                    if (find && find1)
                        break;
                }

                if (!find || !find1)
                {
                    return false;
                }

                var mmc = JsonConvert.DeserializeObject<MMCObj>
                    (Encoding.UTF8.GetString(stream1.ToArray()));
                if (mmc == null)
                {
                    return false;
                }

                var mmc1 = Encoding.UTF8.GetString(stream2.ToArray());

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

                foreach (ZipEntry e in zFile)
                {
                    if (e.IsFile && e.Name.StartsWith(path))
                    {
                        using var stream = zFile.GetInputStream(e);
                        string file = Path.GetFullPath($"{game.GetBasePath()}/{e.Name[path.Length..]}");
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
                using var zFile = new ZipFile(st);
                using var stream1 = new MemoryStream();
                using var stream2 = new MemoryStream();
                bool find = false;
                bool find1 = false;
                foreach (ZipEntry e in zFile)
                {
                    if (e.IsFile && e.Name == Names.NameHMCLFile)
                    {
                        using var stream = zFile.GetInputStream(e);
                        await stream.CopyToAsync(stream1);
                        find = true;
                    }

                    if (e.IsFile && e.Name == Names.NameManifestFile)
                    {
                        using var stream = zFile.GetInputStream(e);
                        await stream.CopyToAsync(stream2);
                        find1 = true;
                    }

                    if (find && find1)
                    {
                        break;
                    }
                }

                if (!find)
                {
                    return false;
                }

                var obj = JsonConvert.DeserializeObject<HMCLObj>
                    (Encoding.UTF8.GetString(stream1.ToArray()));

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

                if (find1)
                {
                    var obj1 = JsonConvert.DeserializeObject<CurseForgePackObj>
                        (Encoding.UTF8.GetString(stream2.ToArray()));
                    if (obj1 != null)
                    {
                        overrides = obj1.Overrides;
                    }
                }

                foreach (ZipEntry e in zFile)
                {
                    if (e.IsFile && e.Name.StartsWith(overrides))
                    {
                        string file = Path.GetFullPath(game.GetGamePath() + e.Name[overrides.Length..]);
                        if (e.Name.EndsWith(Names.NameIconFile))
                        {
                            file = game.GetIconFile();
                        }
                        using var stream = zFile.GetInputStream(e);
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
                        var obj1 = JsonConvert.DeserializeObject<OfficialObj>(PathHelper.ReadText(item)!);
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
