using System.Text;
using ColorMC.Core.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace ColorMC.Core.Helpers;

public static class InstallGameHelper
{
    /// <summary>
    /// 导入整合包
    /// </summary>
    /// <param name="dir">压缩包路径</param>
    /// <param name="type">类型</param>
    /// <param name="type">名字</param>
    /// <param name="type">群组</param>
    public static async Task<(bool, GameSettingObj?)> InstallZip(string dir, PackType type,
        string? name, string? group, ColorMCCore.ZipUpdate zip, ColorMCCore.Request request,
        ColorMCCore.GameOverwirte overwirte, ColorMCCore.PackUpdate update,
         ColorMCCore.PackState update2)
    {
        GameSettingObj? game = null;
        bool import = false;
        Stream? stream4 = null;
        try
        {
            stream4 = PathHelper.OpenRead(dir);
            switch (type)
            {
                //ColorMC格式
                case PackType.ColorMC:
                    {
                        update2(CoreRunState.Read);
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

                        if (InstancesPath.HaveGameWithName(game.Name))
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

                        game.AddToGroup();

                        update2(CoreRunState.End);
                        import = true;
                        break;
                    }
                //Curseforge压缩包
                case PackType.CurseForge:
                    (import, game) = await ModPackHelper.DownloadCurseForgeModPackAsync(dir, name,
                        group, request, overwirte, update, update2);

                    update2(CoreRunState.End);
                    break;
                //Modrinth压缩包
                case PackType.Modrinth:
                    (import, game) = await ModPackHelper.DownloadModrinthModPackAsync(dir, name,
                        group, request, overwirte, update, update2);

                    update2(CoreRunState.End);
                    break;
                //MMC压缩包
                case PackType.MMC:
                    {
                        update2(CoreRunState.Read);
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

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            game.Name = name;
                        }
                        if (!string.IsNullOrWhiteSpace(group))
                        {
                            game.GroupName = group;
                        }
                        if (string.IsNullOrWhiteSpace(game.Name))
                        {
                            game.Name = new FileInfo(dir).Name;
                        }
                        if (!string.IsNullOrWhiteSpace(icon))
                        {
                            game.Icon = icon + ".png";
                        }
                        game = await game.CreateGame(request, overwirte);

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

                        update2(CoreRunState.End);
                        import = true;
                        break;
                    }
                //HMCL压缩包
                case PackType.HMCL:
                    {
                        update2(CoreRunState.Read);
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
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            game.Name = name;
                        }
                        if (!string.IsNullOrWhiteSpace(group))
                        {
                            game.GroupName = group;
                        }

                        game = await game.CreateGame(request, overwirte);

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

                        update2(CoreRunState.End);
                        import = true;
                        break;
                    }
                //直接解压
                case PackType.ZipPack:
                    {
                        update2(CoreRunState.Read);

                        name ??= Path.GetFileName(dir);

                        update2(CoreRunState.Start);
                        game = await InstancesPath.CreateGame(new()
                        {
                            GroupName = group,
                            Name = name!
                        }, request, overwirte);

                        if (game != null)
                        {
                            await new ZipUtils(ZipUpdate: zip).UnzipAsync(game!.GetGamePath(), dir, stream4!);
                            update2(CoreRunState.End);
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
            await game.Remove(request);
        }
        update2(CoreRunState.End);
        return (import, game);
    }

    /// <summary>
    /// 安装Modrinth压缩包
    /// </summary>
    /// <param name="data">整合包信息</param>
    /// <param name="name">名字</param>
    /// <param name="group">群组</param>
    /// <returns>结果</returns>
    public static async Task<(bool, GameSettingObj?)> InstallModrinth(ModrinthVersionObj data,
        string? name, string? group, ColorMCCore.ZipUpdate zip, ColorMCCore.Request request,
        ColorMCCore.GameOverwirte overwirte, ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];
        var item = new DownloadItemObj()
        {
            Url = file.url,
            Name = file.filename,
            SHA1 = file.hashes.sha1,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + file.filename),
        };

        var res1 = await DownloadManager.StartAsync([item]);
        if (!res1)
            return (false, null);

        var res2 = await InstallZip(item.Local, PackType.Modrinth, name, group, zip,
            request, overwirte, update, update2);
        if (res2.Item1)
        {
            res2.Item2!.PID = data.project_id;
            res2.Item2.FID = data.id;
            res2.Item2.Save();
        }

        return res2;
    }

    /// <summary>
    /// 安装curseforge压缩包
    /// </summary>
    /// <param name="data">整合包信息</param>
    /// <param name="name">名字</param>
    /// <param name="group">群组</param>
    /// <returns>结果</returns>
    public static async Task<(bool, GameSettingObj?)> InstallCurseForge(CurseForgeModObj.Data data,
        string? name, string? group, ColorMCCore.ZipUpdate zip, ColorMCCore.Request request,
        ColorMCCore.GameOverwirte overwirte, ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        data.FixDownloadUrl();

        var item = new DownloadItemObj()
        {
            Url = data.downloadUrl,
            Name = data.fileName,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + data.fileName),
        };

        var res1 = await DownloadManager.StartAsync([item]);
        if (!res1)
            return (false, null);

        var res2 = await InstallZip(item.Local, PackType.CurseForge, name, group, zip,
            request, overwirte, update, update2);
        if (res2.Item1)
        {
            res2.Item2!.PID = data.modId.ToString();
            res2.Item2.FID = data.id.ToString();
            res2.Item2.Save();
        }

        return res2;
    }

    /// <summary>
    /// 升级整合包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="data">数据</param>
    /// <returns>结果</returns>
    public static async Task<bool> UpdateModPack(this GameSettingObj obj, CurseForgeModObj.Data data,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        data.FixDownloadUrl();

        var item = new DownloadItemObj()
        {
            Url = data.downloadUrl,
            Name = data.fileName,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + data.fileName),
        };

        var res = await DownloadManager.StartAsync([item]);
        if (!res)
            return false;

        res = await ModPackHelper.UpdateCurseForgeModPackAsync(obj, item.Local, update, update2);
        if (res)
        {
            obj.PID = data.modId.ToString();
            obj.FID = data.id.ToString();
            obj.Save();
            obj.SaveModInfo();
        }

        return res;
    }

    /// <summary>
    /// 升级整合包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="data"></param>
    /// <returns>升级结果</returns>
    public static async Task<bool> UpdateModPack(this GameSettingObj obj, ModrinthVersionObj data,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];
        var item = new DownloadItemObj()
        {
            Url = file.url,
            Name = file.filename,
            SHA1 = file.hashes.sha1,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + file.filename),
        };

        var res = await DownloadManager.StartAsync([item]);
        if (!res)
            return false;

        res = await ModPackHelper.UpdateModrinthModPackAsync(obj, item.Local, update, update2);
        if (res)
        {
            obj.PID = data.project_id;
            obj.FID = data.id;
            obj.Save();
            obj.SaveModInfo();
        }

        return res;
    }
}
