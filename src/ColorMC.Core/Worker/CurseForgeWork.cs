using ColorMC.Core.Downloader;
using ColorMC.Core.GuiHandle;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Utils;
using SharpCompress.Archives.Zip;

namespace ColorMC.Core.Worker;

public class CurseForgeWork : ModPackWork, IModPackWork
{
    private CurseForgePackObj? _info;

    public CurseForgeWork(ZipArchive zip, IOverGameGui? gui, IAddGui? packgui, CancellationToken token) : base(zip, gui, packgui, token)
    {

    }

    public CurseForgeWork(string file, IOverGameGui? gui, IAddGui? packgui) : base(file, gui, packgui, CancellationToken.None)
    {

    }

    /// <summary>
    /// 获取主信息
    /// </summary>
    public bool ReadInfo()
    {
        if (Zip.Entries.FirstOrDefault(item => item.Key == Names.NameManifestFile) is { } ent)
        {
            using var stream = ent.OpenEntryStream();
            _info = JsonUtils.ToObj(stream, JsonType.CurseForgePackObj);
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

        foreach (var item in _info.Minecraft.ModLoaders)
        {
            if (item.Id.StartsWith(Names.NameForgeKey))
            {
                Loader = Loaders.Forge;
                LoaderVersion = item.Id.Replace(Names.NameForgeKey + "-", "");
            }
            else if (item.Id.StartsWith(Names.NameNeoForgeKey))
            {
                Loader = Loaders.NeoForge;
                LoaderVersion = item.Id.Replace(Names.NameNeoForgeKey + "-", "");
            }
            else if (item.Id.StartsWith(Names.NameFabricKey))
            {
                Loader = Loaders.Fabric;
                LoaderVersion = item.Id.Replace(Names.NameFabricKey + "-", "");
            }
            else if (item.Id.StartsWith(Names.NameQuiltKey))
            {
                Loader = Loaders.Quilt;
                LoaderVersion = item.Id.Replace(Names.NameQuiltKey + "-", "");
            }
        }

        if (LoaderVersion.StartsWith(_info.Minecraft.Version + "-")
            && LoaderVersion.Length > _info.Minecraft.Version.Length + 1)
        {
            LoaderVersion = LoaderVersion[(_info.Minecraft.Version.Length + 1)..];
        }

        GameVersion = _info.Minecraft.Version;

        if (CheckHelpers.CheckGameArgFileAsync(GameVersion) == null)
        {
            await VersionPath.GetFromWebAsync();
            if (CheckHelpers.CheckGameArgFileAsync(GameVersion) == null)
            {
                return false;
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

    /// <summary>
    /// 创建游戏实例
    /// </summary>
    /// <returns></returns>
    public async Task<GameSettingObj?> CreateGame(string? group)
    {
        if (_info == null)
        {
            return null;
        }
        var name = $"{_info.Name}-{_info.Version}";

        return Game = await InstancesPath.CreateGameAsync(new GameSettingObj()
        {
            GroupName = group,
            Name = name,
            Version = GameVersion,
            Modpack = true,
            Loader = Loader,
            ModPackType = SourceType.CurseForge,
            LoaderVersion = LoaderVersion
        }, Gui);
    }

    /// <summary>
    /// 解压文件
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Unzip(List<ZipArchiveEntry>? unselect)
    {
        if (_info == null || Game == null)
        {
            return false;
        }

        unselect ??= [];
        
        var size = Zip.Entries.Count;
        var index = 0;

        Packgui?.SetNowSub(0, size);

        foreach (var e in Zip.Entries)
        {
            if (Token.IsCancellationRequested)
            {
                return false;
            }
            if (unselect.Contains(e) || !FunctionUtils.IsFile(e))
            {
                index++;
                Packgui?.SetNowSub(index, size);
                continue;
            }

            Packgui?.SetSubText(e.Key!);
            index++;
            if (e.Key!.StartsWith(_info.Overrides + "/"))
            {
                using var stream = e.OpenEntryStream();
                string file = Path.GetFullPath(Game.GetGamePath() + e.Key[_info.Overrides.Length..]);
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

    /// <summary>
    /// 获取Mod信息
    /// </summary>
    /// <param name="packgui"></param>
    /// <returns></returns>
    public async Task<bool> GetInfo()
    {
        if (_info == null || Game == null)
        {
            return false;
        }

        var list = await CurseForgeHelper.GetModPackInfoAsync(Game, _info, Packgui, Token);

        if (Token.IsCancellationRequested)
        {
            return false;
        }

        Game.Mods.Clear();
        foreach (var item in list.Mods)
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

    public async Task Download()
    {
        if (Downloads != null)
        {
            await DownloadManager.StartAsync(Downloads, Packgui, Token);
        }
    }

    public async Task<bool> CheckUpgrade()
    {
        if (_info == null || Game == null)
        {
            return false;
        }
        CurseForgePackObj? info3 = null;
        var json = Path.Combine(Game.GetBasePath(), Names.NameManifestFile);
        if (File.Exists(json))
        {
            try
            {
                using var stream = PathHelper.OpenRead(json);
                info3 = JsonUtils.ToObj(stream, JsonType.CurseForgePackObj);
            }
            catch
            {

            }
        }

        var path = Game.GetGamePath();

        Downloads = [];

        int b = 0;

        //筛选需要升级的资源
        if (info3 != null)
        {
            var addlist = new List<CurseForgePackObj.FilesObj>();
            var removelist = new List<CurseForgePackObj.FilesObj>();

            CurseForgePackObj.FilesObj?[] temp1 = [.. _info.Files];
            CurseForgePackObj.FilesObj?[] temp2 = [.. info3.Files];

            for (int index1 = 0; index1 < temp1.Length; index1++)
            {
                var item = temp1[index1];
                for (int index2 = 0; index2 < temp2.Length; index2++)
                {
                    var item1 = temp2[index2];
                    if (item1 == null)
                        continue;
                    if (item!.ProjectID == item1.ProjectID)
                    {
                        temp1[index1] = null;
                        temp2[index2] = null;
                        if (item.FileID != item1.FileID)
                        {
                            addlist.Add(item1);
                            removelist.Add(item);
                            break;
                        }
                    }
                }
            }

            foreach (var item in temp1)
            {
                if (item != null)
                {
                    addlist.Add(item);
                }
            }

            foreach (var item in temp2)
            {
                if (item != null)
                {
                    removelist.Add(item);
                }
            }

            foreach (var item in removelist)
            {
                if (Game.Mods.Remove(item.ProjectID.ToString(), out var mod))
                {
                    PathHelper.Delete(Path.Combine(path, mod.Path, mod.File));
                }
            }

            if (addlist.Count > 0)
            {
                Packgui?.SetNowSub(addlist.Count, 0);
            }

            foreach (var item in addlist)
            {
                var res = await CurseForgeAPI.GetModAsync(item);
                if (res == null || res.Data == null)
                {
                    return false;
                }

                var path1 = await CurseForgeHelper.GetItemPathAsync(Game, res.Data);
                var modid = res.Data.ModId.ToString();
                var item1 = res.Data.MakeDownloadObj(Game, path1);
                Downloads.Add(item1);

                Game.Mods.Remove(modid, out _);
                Game.Mods.TryAdd(modid, res.Data.MakeModInfo(path1.Path));

                Packgui?.SetNowSub(addlist.Count, ++b);
            }
        }
        else
        {
            //没有筛选信息通过sha1筛选
            var addlist = new List<ModInfoObj>();
            var removelist = new List<ModInfoObj>();

            var res = await CurseForgeHelper.GetModPackInfoAsync(Game, _info, Packgui);
            if (!res.State)
            {
                return false;
            }

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
                var local = Path.Combine(path, item.Path, item.File);
                if (File.Exists(local))
                {
                    PathHelper.Delete(local);
                }
                Game.Mods.Remove(item.ModId);
            }

            foreach (var item in addlist)
            {
                Downloads.Add(res.List!.First(a => a.Sha1 == item.Sha1));
                Game.Mods.Add(item.ModId, item);
            }
        }

        return true;
    }

    public void Dispose()
    {
        Zip?.Dispose();
    }
}
