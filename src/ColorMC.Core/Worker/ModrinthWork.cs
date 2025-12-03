using ColorMC.Core.Downloader;
using ColorMC.Core.GuiHandle;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Worker;

public class ModrinthWork : ModPackWork, IModPackWork
{
    private ModrinthPackObj? _info;

    public ModrinthWork(Stream st, IOverGameGui? gui, IAddGui? packgui, CancellationToken token) : base(st, gui, packgui, token)
    {

    }

    public ModrinthWork(string file, IOverGameGui? gui, IAddGui? packgui) : base(file, gui, packgui, default)
    {

    }

    public async Task<bool> CheckUpgrade()
    {
        if (_info == null || Game == null)
        {
            return false;
        }
        //获取当前整合包数据
        ModrinthPackObj? info3 = null;
        var json = Path.Combine(Game.GetBasePath(), Names.NameModrinthFile);
        if (File.Exists(json))
        {
            try
            {
                using var stream = PathHelper.OpenRead(json);
                info3 = JsonUtils.ToObj(stream, JsonType.ModrinthPackObj);
            }
            catch
            {

            }
        }

        //获取Mod信息
        var res = ModrinthHelper.GetModrinthModInfo(Game, _info, Packgui);

        Downloads = [];

        string path = Game.GetGamePath();

        if (info3 != null)
        {
            //筛选新旧整合包文件差距
            var addlist = new List<ModrinthPackObj.ModrinthPackFileObj>();
            var removelist = new List<ModrinthPackObj.ModrinthPackFileObj>();

            ModrinthPackObj.ModrinthPackFileObj?[] temp1 = [.. _info.Files];
            ModrinthPackObj.ModrinthPackFileObj?[] temp2 = [.. info3.Files];

            for (int b = 0; b < temp1.Length; b++)
            {
                var item = temp1[b];
                if (item == null)
                    continue;
                for (int a = 0; a < temp2.Length; a++)
                {
                    var item1 = temp2[a];
                    if (item1 == null)
                        continue;
                    if (item.Hashes.Sha1 == item1.Hashes.Sha1)
                    {
                        temp1[b] = null;
                        temp2[a] = null;
                    }
                }
            }

            foreach (var item in temp1)
            {
                if (item != null)
                {
                    removelist.Add(item);
                }
            }

            foreach (var item in temp2)
            {
                if (item != null)
                {
                    addlist.Add(item);
                }
            }

            foreach (var item in removelist)
            {
                PathHelper.Delete(Path.Combine(path, item.Path));

                var url = item.Downloads.FirstOrDefault(a => a.StartsWith($"{UrlHelper.ModrinthDownload}data/"));
                if (url is { })
                {
                    var modid = StringHelper.GetString(url, "data/", "/ver");
                    Game.Mods.Remove(modid);
                }
            }

            foreach (var item in addlist)
            {
                var item11 = res.List!.First(a => a.Sha1 == item.Hashes.Sha1);
                Downloads.Add(item11);
                var url = item.Downloads.FirstOrDefault(a => a.StartsWith($"{UrlHelper.ModrinthDownload}data/"));
                if (url != null)
                {
                    var modid = StringHelper.GetString(url, "data/", "/ver");
                    var fileid = StringHelper.GetString(url, "versions/", "/");

                    Game.Mods.Remove(modid);
                    Game.Mods.Add(modid, new()
                    {
                        Path = item.Path[..item.Path.IndexOf('/')],
                        Name = item.Path,
                        File = item.Path,
                        Sha1 = item11.Sha1!,
                        ModId = modid,
                        FileId = fileid,
                        Url = url
                    });
                }
            }
        }
        else
        {
            //没有整合包信息
            var addlist = new List<ModInfoObj>();
            var removelist = new List<ModInfoObj>();

            ModInfoObj[] temp1 = [.. Game.Mods.Values];
            ModInfoObj?[] temp2 = [.. res.Mods.Values];

            foreach (var item in temp1)
            {
                for (int a = 0; a < temp2.Length; a++)
                {
                    var item1 = temp2[a];
                    if (item1 == null)
                        continue;
                    if (item.ModId == item1.ModId)
                    {
                        temp2[a] = null;
                        if (item.FileId != item1.FileId
                            || item.Sha1 != item1.Sha1)
                        {
                            addlist.Add(item1);
                            removelist.Add(item);
                            break;
                        }
                    }
                }
            }

            foreach (var item in temp2)
            {
                if (item != null)
                {
                    addlist.Add(item);
                }
            }

            foreach (var item in removelist)
            {
                PathHelper.Delete(Path.GetFullPath($"{path}/{item.File}"));
                Game.Mods.Remove(item.ModId);
            }

            foreach (var item in addlist)
            {
                Downloads.Add(res.List!.First(a => a.Sha1 == item.Sha1));
                Game.Mods.Add(item.ModId, item);
            }
        }

        using var stream1 = PathHelper.OpenWrite(json);
        JsonUtils.ToString(stream1, _info, JsonType.ModrinthPackObj);

        return true;
    }

    public async Task<GameSettingObj?> CreateGame(string? group)
    {
        if (_info == null)
        {
            return null;
        }

        var name = $"{_info.Name}-{_info.VersionId}";
        //创建游戏实例
        return Game = await InstancesPath.CreateGameAsync(new GameSettingObj()
        {
            GroupName = group,
            Name = name,
            Version = GameVersion,
            ModPack = true,
            ModPackType = SourceType.Modrinth,
            Loader = Loader,
            LoaderVersion = LoaderVersion
        }, Gui);
    }

    public async Task Download()
    {
        if (Downloads != null)
        {
            await DownloadManager.StartAsync(Downloads, Packgui, Token);
        }
    }

    public async Task<bool> GetInfo()
    {
        if (_info == null || Game == null)
        {
            return false;
        }

        var list = ModrinthHelper.GetModrinthModInfo(Game, _info, Packgui, Token);

        if (Token.IsCancellationRequested)
        {
            return false;
        }

        Game.Mods.Clear();
        foreach (var item in Game.Mods)
        {
            Game.Mods.Add(item.Key, item.Value);
        }

        Game.SaveModInfo();

        if (list.List != null)
        {
            Downloads = [.. list.List];
        }

        return list.State;
    }

    /// <summary>
    /// 获取主信息
    /// </summary>
    /// <returns></returns>
    public bool ReadInfo()
    {
        if (Zip.Entries.FirstOrDefault(item => item.Key == Names.NameModrinthFile) is { } ent)
        {
            using var stream = ent.OpenEntryStream();
            _info = JsonUtils.ToObj(stream, JsonType.ModrinthPackObj);
        }
        else
        {
            return false;
        }

        if (_info == null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 获取版本数据
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ReadVersion()
    {
        if (_info == null)
        {
            return false;
        }
        if (_info.Dependencies.TryGetValue(Names.NameForgeKey, out var version))
        {
            Loader = Loaders.Forge;
            LoaderVersion = version;
        }
        else if (_info.Dependencies.TryGetValue(Names.NameNeoForgeKey, out version))
        {
            Loader = Loaders.NeoForge;
            LoaderVersion = version;
        }
        else if (_info.Dependencies.TryGetValue(Names.NameFabricLoaderKey, out version))
        {
            Loader = Loaders.Fabric;
            LoaderVersion = version;
        }
        else if (_info.Dependencies.TryGetValue(Names.NameQuiltLoaderKey, out version))
        {
            Loader = Loaders.Quilt;
            LoaderVersion = version;
        }

        GameVersion = _info.Dependencies[Names.NameMinecraftKey];

        if (VersionPath.CheckUpdateAsync(GameVersion) == null)
        {
            await VersionPath.GetFromWebAsync();
            if (VersionPath.CheckUpdateAsync(GameVersion) == null)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> Unzip()
    {
        if (_info == null || Game == null)
        {
            return false;
        }

        int length = Names.NameOverrideDir.Length;
        string dir = Names.NameOverrideDir;

        var size = Zip.Entries.Count;
        var index = 0;

        Packgui?.SetNowSub(0, size);

        //解压文件
        foreach (var e in Zip.Entries)
        {
            if (Token.IsCancellationRequested)
            {
                return false;
            }
            if (!FuntionUtils.IsFile(e))
            {
                index++;
                Packgui?.SetNowSub(index, size);
                continue;
            }

            Packgui?.SetSubText(e.Key!);
            index++;
            if (e.Key!.StartsWith(dir))
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(Game.GetGamePath() + e.Key[length..]);
                file = PathHelper.ReplacePathName(file);
                await PathHelper.WriteBytesAsync(file, stream);
            }
            else
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(Game.GetBasePath() + "/" + e.Key);
                file = PathHelper.ReplacePathName(file);
                await PathHelper.WriteBytesAsync(file, stream);
            }
        }

        return true;
    }

    public void UpdateGame(GameSettingObj game)
    {
        Game = game;
        game.Loader = Loader;
        game.LoaderVersion = LoaderVersion;
        game.Version = GameVersion;

        game.Save();
    }

    public void Dispose()
    {
        Zip?.Dispose();
    }
}
