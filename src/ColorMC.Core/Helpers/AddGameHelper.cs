using System.Text;
using ColorMC.Core.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace ColorMC.Core.Helpers;

public static class AddGameHelper
{
    /// <summary>
    /// 导入文件夹
    /// </summary>
    /// <param name="name">实例名字</param>
    /// <param name="local">位置</param>
    /// <param name="unselect">排除的文件</param>
    /// <param name="group">游戏群组</param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>
    public static async Task<GameRes> AddGame(AddGameArg arg)
    {
        if (string.IsNullOrWhiteSpace(arg.Local))
        {
            throw new Exception("Local is empty");
        }
        GameSettingObj? game = null;

        bool isfind = false;

        var file1 = Path.GetFullPath(arg.Local + "/" + "mmc-pack.json");
        var file2 = Path.GetFullPath(arg.Local + "/" + "instance.cfg");
        if (File.Exists(file1) && File.Exists(file2))
        {
            try
            {
                var mmc = JsonConvert.DeserializeObject<MMCObj>(PathHelper.ReadText(file1)!);
                if (mmc != null)
                {
                    var mmc1 = PathHelper.ReadText(file2)!;
                    game = mmc.ToColorMC(mmc1, out var icon);
                    game.Icon = icon + ".png";
                    isfind = true;
                }
            }
            catch
            {

            }
        }

        if (!isfind)
        {
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
                    if (obj1 != null && obj1.id != null)
                    {
                        game = obj1.ToColorMC();
                        isfind = true;
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
            return new GameRes
            {
                State = false
            };
        }

        await game.CopyFile(arg.Local, arg.Unselect);

        return new GameRes
        {
            State = true,
            Game = game
        };
    }

    /// <summary>
    /// 导入整合包
    /// </summary>
    /// <param name="dir">压缩包路径</param>
    /// <param name="type">类型</param>
    /// <param name="type">名字</param>
    /// <param name="type">群组</param>
    public static async Task<GameRes> InstallZip(InstallZipArg arg)
    {
        GameSettingObj? game = null;
        bool import = false;
        Stream? stream4 = null;
        try
        {
            stream4 = PathHelper.OpenRead(arg.Dir);
            switch (arg.Type)
            {
                //ColorMC格式
                case PackType.ColorMC:
                    {
                        arg.Update2?.Invoke(CoreRunState.Read);
                        using ZipFile zFile = new(stream4);
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

                        game = await InstancesPath.CreateGame(new CreateGameArg
                        {
                            Game = game,
                            Request = arg.Request,
                            Overwirte = arg.Overwirte
                        });

                        if (game == null)
                            break;

                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile)
                            {
                                using var stream = zFile.GetInputStream(e);
                                var path = game.GetBasePath();
                                string file = Path.GetFullPath(path + '/' + e.Name);
                                await PathHelper.WriteBytesAsync(file, stream);
                            }
                        }

                        arg.Update2?.Invoke(CoreRunState.End);
                        import = true;
                        break;
                    }
                //Curseforge压缩包
                case PackType.CurseForge:
                    var res1 = await ModPackHelper.InstallCurseForgeModPackAsync(new InstallModPackZipArg
                    {
                        Zip = arg.Dir,
                        Name = arg.Name,
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
                        Zip = arg.Dir,
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
                    {
                        arg.Update2?.Invoke(CoreRunState.Read);
                        using ZipFile zFile = new(stream4);
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

                        game = mmc.ToColorMC(mmc1, out var icon);

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
                        if (!string.IsNullOrWhiteSpace(icon))
                        {
                            game.Icon = icon + ".png";
                        }
                        game = await InstancesPath.CreateGame(new CreateGameArg
                        {
                            Game = game,
                            Request = arg.Request,
                            Overwirte = arg.Overwirte
                        });

                        if (game == null)
                            break;

                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile && e.Name.StartsWith(path))
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

                        arg.Update2?.Invoke(CoreRunState.End);
                        import = true;
                        break;
                    }
                //HMCL压缩包
                case PackType.HMCL:
                    {
                        arg.Update2?.Invoke(CoreRunState.Read);
                        using ZipFile zFile = new(stream4);
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
                            break;

                        string overrides = "overrides";

                        if (find1)
                        {
                            var obj1 = JsonConvert.DeserializeObject<CurseForgePackObj>
                                (Encoding.UTF8.GetString(stream2.ToArray()));
                            if (obj1 != null)
                            {
                                overrides = obj1.overrides;
                            }
                        }

                        foreach (ZipEntry e in zFile)
                        {
                            if (e.IsFile && e.Name.StartsWith(overrides))
                            {
                                string file = Path.GetFullPath(string.Concat(game.GetGamePath(),
                                    e.Name.AsSpan(overrides.Length)));
                                if (e.Name.EndsWith("icon.png"))
                                {
                                    file = game.GetIconFile();
                                }
                                using var stream = zFile.GetInputStream(e);

                                FileInfo info2 = new(file);
                                info2.Directory?.Create();
                                using FileStream stream3 = new(file, FileMode.Create,
                                    FileAccess.ReadWrite, FileShare.ReadWrite);
                                await stream.CopyToAsync(stream3);
                            }
                        }

                        arg.Update2?.Invoke(CoreRunState.End);
                        import = true;
                        break;
                    }
                //直接解压
                case PackType.ZipPack:
                    {
                        arg.Update2?.Invoke(CoreRunState.Read);

                        arg.Name ??= Path.GetFileName(arg.Dir);

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

                        if (game != null)
                        {
                            await new ZipUtils(ZipUpdate: arg.Zip).UnzipAsync(game!.GetGamePath(), arg.Dir, stream4!);
                            arg.Update2?.Invoke(CoreRunState.End);
                            import = true;
                        }
                        break;
                    }
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(LanguageHelper.Get("Core.Pack.Error2"), e, false);
        }
        finally
        {
            stream4?.Dispose();
        }
        if (!import && game != null)
        {
            await game.Remove(arg.Request);
        }
        arg.Update2?.Invoke(CoreRunState.End);
        return new GameRes { State = import, Game = game };
    }

    /// <summary>
    /// 安装Modrinth整合包
    /// </summary>
    /// <param name="data">整合包信息</param>
    /// <param name="name">名字</param>
    /// <param name="group">群组</param>
    /// <returns>结果</returns>
    public static async Task<GameRes> InstallModrinth(DownloadModrinthArg arg)
    {
        var file = arg.Data.files.FirstOrDefault(a => a.primary) ?? arg.Data.files[0];
        var item = new DownloadItemObj()
        {
            Url = file.url,
            Name = file.filename,
            SHA1 = file.hashes.sha1,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + file.filename),
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
            res2.Game!.PID = arg.Data.project_id;
            res2.Game.FID = arg.Data.id;
            res2.Game.Save();

            if (arg.Data1.icon_url != null)
            {
                await res2.Game.SetGameIconFromUrl(arg.Data1.icon_url);
            }
        }

        return res2;
    }

    /// <summary>
    /// 安装curseforge整合包
    /// </summary>
    /// <param name="data">整合包信息</param>
    /// <param name="name">名字</param>
    /// <param name="group">群组</param>
    /// <returns>结果</returns>
    public static async Task<GameRes> InstallCurseForge(DownloadCurseForgeArg arg)
    {
        arg.Data.FixDownloadUrl();

        var item = new DownloadItemObj()
        {
            Url = arg.Data.downloadUrl,
            Name = arg.Data.fileName,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + arg.Data.fileName),
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
            res2.Game!.PID = arg.Data.modId.ToString();
            res2.Game.FID = arg.Data.id.ToString();
            res2.Game.Save();

            if (arg.Data1.logo != null)
            {
                await res2.Game.SetGameIconFromUrl(arg.Data1.logo.url);
            }
        }

        return res2;
    }


}
