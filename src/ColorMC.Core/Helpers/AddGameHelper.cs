using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.GuiHandle;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Utils;
using ColorMC.Core.Worker;
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
    public static async Task<GameRes> AddGameFolder(string local, string? name, string? group,
        List<string>? unselect, IOverGameGui? gui, IZipGui? zipgui)
    {
        if (string.IsNullOrWhiteSpace(local))
        {
            throw new ArgumentNullException(nameof(local));
        }

        GameSettingObj? game = null;

        bool isfind = false;
        bool ismmc = false;

        //是否为MMC实例
        if (GameHelper.IsMMCVersion(local))
        {
            try
            {
                using var stream = PathHelper.OpenRead(Path.Combine(local, Names.NameMMCJsonFile));
                var mmc = JsonUtils.ToObj(stream, JsonType.MMCObj);
                if (mmc != null)
                {
                    using var stream1 = PathHelper.OpenRead(Path.Combine(local, Names.NameMMCCfgFile));
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
            var files = Directory.GetFiles(local);
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
            GroupName = group
        };

        //没有名字使用输入名字，已有名字同时有输入名字则覆盖
        if (string.IsNullOrWhiteSpace(game.Name))
        {
            game.Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        else if (!string.IsNullOrWhiteSpace(name))
        {
            game.Name = name;
        }

        //创建游戏实例
        game = await InstancesPath.CreateGameAsync(game, gui);
        if (game == null)
        {
            return new GameRes();
        }

        //复制游戏文件
        await game.CopyFileAsync(local, unselect, ismmc, zipgui);

        return new GameRes
        {
            State = true,
            Game = game
        };
    }

    /// <summary>
    /// 导入CurseForge整合包
    /// </summary>
    /// <param name="arg">导入参数</param>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> ModPackAsync(PackType type, string? group, Stream st,
        IOverGameGui? gui, IAddGui? packgui, CancellationToken token)
    {
        packgui?.SetState(AddState.ReadInfo);
        packgui?.SetNow(1, 5);
        packgui?.SetNowSub(0, 1);

        using IModPackWork work = type switch
        {
            PackType.CurseForge => new CurseForgeWork(st, gui, packgui, token),
            PackType.Modrinth => new ModrinthWork(st, gui, packgui, token),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        if (!work.ReadInfo() || !await work.ReadVersion())
        {
            return new GameRes();
        }

        if (token.IsCancellationRequested)
        {
            return new GameRes();
        }

        var game = await work.CreateGame(group);

        if (token.IsCancellationRequested)
        {
            return new GameRes { Game = game };
        }

        packgui?.SetState(AddState.Unzip);
        packgui?.SetNow(2, 5);
        packgui?.SetNowSub(0, 1);

        if (!await work.Unzip() || token.IsCancellationRequested)
        {
            return new GameRes() { Game = game };
        }

        packgui?.SetSubText(null);
        packgui?.SetNowSub(0, 0);

        packgui?.SetState(AddState.GetInfo);
        packgui?.SetNow(3, 5);
        packgui?.SetNowSub(0, 1);

        if (!await work.GetInfo() || token.IsCancellationRequested)
        {
            return new GameRes { Game = game };
        }

        packgui?.SetSubText(null);
        packgui?.SetNowSub(0, 1);
        packgui?.SetState(AddState.DownloadFile);
        packgui?.SetNow(4, 5);

        await work.Download();

        packgui?.SetState(AddState.Done);
        packgui?.SetNow(5, 5);

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 导入ColorMC压缩包
    /// </summary>
    /// <param name="arg">导入参数</param>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> ColorMCAsync(Stream st, IOverGameGui? gui, IAddGui? packgui)
    {
        packgui?.SetState(AddState.ReadInfo);
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
        game = await InstancesPath.CreateGameAsync(game, gui);

        if (game == null)
        {
            return new GameRes();
        }

        //复制文件
        var size = zFile.Entries.Count;
        var index = 0;

        packgui?.SetState(AddState.Unzip);
        packgui?.SetNowSub(index, size);

        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                index++;
                continue;
            }
            packgui?.SetNowSub(index, size);
            packgui?.SetSubText(e.Key!);
            index++;
            using var stream = e.OpenEntryStream();
            string file = Path.GetFullPath(game.GetBasePath() + "/" + e.Key);
            await PathHelper.WriteBytesAsync(file, stream);
        }

        packgui?.SetSubText(null);
        packgui?.SetState(AddState.Done);

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 导入MMC压缩包
    /// </summary>
    /// <param name="arg">导入参数</param>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> MMCAsync(string? name, string? group, string file,
        Stream st, IOverGameGui? gui, IAddGui? packgui)
    {
        packgui?.SetState(AddState.ReadInfo);
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
            game.Name = Path.GetFileName(file);
        }
        if (!string.IsNullOrWhiteSpace(res.Icon))
        {
            game.Icon = res.Icon + ".png";
        }

        game = await InstancesPath.CreateGameAsync(game, gui);

        if (game == null)
        {
            return new GameRes();
        }

        var size = zFile.Entries.Count;
        var index = 0;

        packgui?.SetState(AddState.Unzip);
        packgui?.SetNowSub(index, size);

        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                index++;
                continue;
            }
            if (e.Key!.StartsWith(path))
            {
                packgui?.SetNowSub(index, size);
                packgui?.SetSubText(e.Key);
                index++;
                using var stream = e.OpenEntryStream();
                string file1 = Path.GetFullPath($"{game.GetBasePath()}/{e.Key[path.Length..]}");
                await PathHelper.WriteBytesAsync(file1, stream);
            }
        }

        packgui?.SetSubText(null);

        game.ReadCustomJson();
        if (game.CustomJson.Count > 0)
        {
            game.CustomLoader ??= new();
            game.CustomLoader.CustomJson = true;
        }

        packgui?.SetState(AddState.Done);

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 导入HMCL压缩包
    /// </summary>
    /// <param name="arg">导入参数</param>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> HMCLAsync(string? name, string? group, string file, Stream st, IOverGameGui? gui, IAddGui? packgui)
    {
        packgui?.SetState(AddState.ReadInfo);
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
            game.Name = Path.GetFileName(file);
        }

        game = await InstancesPath.CreateGameAsync(game, gui);

        if (game == null)
        {
            return new GameRes();
        }

        string overrides = Names.NameOverrideDir;

        if (obj1 != null)
        {
            overrides = obj1.Overrides;
        }

        var size = zFile.Entries.Count;
        var index = 0;

        packgui?.SetState(AddState.Unzip);
        packgui?.SetNowSub(index, size);

        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                index++;
                continue;
            }
            if (e.Key!.StartsWith(overrides))
            {
                packgui?.SetNowSub(index, size);
                packgui?.SetSubText(e.Key);
                index++;
                string file1 = Path.GetFullPath(game.GetGamePath() + e.Key[overrides.Length..]);
                if (e.Key.EndsWith(Names.NameIconFile))
                {
                    file1 = game.GetIconFile();
                }
                using var stream = e.OpenEntryStream();
                await PathHelper.WriteBytesAsync(file1, stream);
            }
        }

        packgui?.SetState(AddState.Done);

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 直接解压压缩包
    /// </summary>
    /// <param name="st">输入流</param>
    /// <returns>导入结果</returns>
    private static async Task<GameRes> UnzipAsync(string? name, string? group, string file, Stream st, IOverGameGui? gui, IAddGui? packgui)
    {
        packgui?.SetState(AddState.ReadInfo);

        if (string.IsNullOrWhiteSpace(name))
        {
            name = Path.GetFileName(file);
        }

        var game = await InstancesPath.CreateGameAsync(new GameSettingObj()
        {
            GroupName = group,
            Name = name
        }, gui);

        if (game == null)
        {
            return new GameRes();
        }

        using var zFile = ZipArchive.Open(st);
        var size = zFile.Entries.Count;
        var index = 0;

        packgui?.SetState(AddState.Unzip);
        packgui?.SetNowSub(index, size);

        foreach (var e in zFile.Entries)
        {
            if (!FuntionUtils.IsFile(e))
            {
                index++;
                continue;
            }

            packgui?.SetNowSub(index, size);
            packgui?.SetSubText(e.Key);
            index++;
            string file1 = Path.GetFullPath(game.GetGamePath() + e.Key);
            if (e.Key!.EndsWith(Names.NameIconFile))
            {
                file1 = game.GetIconFile();
            }
            using var stream = e.OpenEntryStream();
            await PathHelper.WriteBytesAsync(file1, stream);
        }

        packgui?.SetState(AddState.ReadInfo);
        packgui?.SetSubText(null);

        //尝试解析版本号
        var files = Directory.GetFiles(game!.GetGamePath());
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

        packgui?.SetState(AddState.Done);

        return new GameRes { State = true, Game = game };
    }

    /// <summary>
    /// 导入整合包
    /// </summary>
    /// <returns>导入结果</returns>
    public static async Task<GameRes> InstallZip(string? name, string? group, string file,
        PackType type, IOverGameGui? gui, IAddGui? packgui, CancellationToken token = default)
    {
        GameRes? res1 = null;
        Stream? st = null;
        try
        {
            //如果是http则下载
            if (file.StartsWith("http"))
            {
                var file1 = Path.Combine(DownloadManager.DownloadDir, FuntionUtils.NewUUID());
                var res = await DownloadManager.StartAsync([new FileItemObj()
                {
                    Url = file,
                    Overwrite = true,
                    Local = file1,
                    Name = Path.GetFileName(file)
                }], token: token);
                if (!res)
                {
                    return new GameRes();
                }
                st = PathHelper.OpenRead(file);
            }
            else
            {
                st = PathHelper.OpenRead(file);
            }
            if (st == null)
            {
                return new();
            }
            res1 = type switch
            {
                PackType.ColorMC => await ColorMCAsync(st, gui, packgui),
                PackType.CurseForge or PackType.Modrinth => await ModPackAsync(type, group, st, gui, packgui, token),
                PackType.MMC => await MMCAsync(name, group, file, st, gui, packgui),
                PackType.HMCL => await HMCLAsync(name, group, file, st, gui, packgui),
                PackType.ZipPack => await UnzipAsync(name, group, file, st, gui, packgui),
                _ => null
            };

            return res1 ?? new GameRes();
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new InstallModPackErrorEventArgs(file, e));
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

        return new GameRes();
    }

    /// <summary>
    /// 下载并安装Modrinth整合包
    /// </summary>
    /// <param name="arg">整合包信息</param>
    /// <returns>导入结果</returns>
    public static async Task<GameRes> InstallModrinth(string? group,
        ModrinthVersionObj data, string? icon, IOverGameGui? gui, IAddGui? packgui)
    {
        var item = data.MakeDownloadObj(DownloadManager.DownloadDir);

        var res1 = await DownloadManager.StartAsync([item]);
        if (!res1)
        {
            return new GameRes();
        }

        var res2 = await InstallZip(null, group, item.Local, PackType.Modrinth, gui, packgui);
        if (res2.State)
        {
            res2.Game!.PID = data.ProjectId;
            res2.Game.FID = data.Id;
            res2.Game.Save();

            if (icon != null)
            {
                await res2.Game.SetGameIconFromUrlAsync(icon);
            }
        }

        return res2;
    }

    /// <summary>
    /// 下载并安装curseforge整合包
    /// </summary>
    /// <returns>导入结果</returns>
    public static async Task<GameRes> InstallCurseForge(string? group, CurseForgeModObj.CurseForgeDataObj data,
        string? icon, IOverGameGui? gui, IAddGui? packgui, CancellationToken token = default)
    {
        packgui?.SetState(AddState.DownloadPack);

        var item = data.MakeDownloadObj(DownloadManager.DownloadDir);

        packgui?.SetSubText(item.Name);
        var res1 = await DownloadManager.StartAsync([item], packgui, token);
        packgui?.SetSubText(null);
        if (!res1)
        {
            return new GameRes();
        }

        var res2 = await InstallZip(null, group, item.Local, PackType.CurseForge, gui, packgui, token);
        if (res2.State && !token.IsCancellationRequested)
        {
            res2.Game!.PID = data.ModId.ToString();
            res2.Game.FID = data.Id.ToString();
            res2.Game.Save();

            if (icon != null)
            {
                await res2.Game.SetGameIconFromUrlAsync(icon);
            }
        }

        return res2;
    }
}
