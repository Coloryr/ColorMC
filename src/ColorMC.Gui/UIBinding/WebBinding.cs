using Avalonia.Threading;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.McMod;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.OptiFine;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class WebBinding
{
    public static async Task<List<FileItemObj>?> GetPackList(SourceType type, string? version,
        string? filter, int page, int sort, string categoryId)
    {
        version ??= "";
        filter ??= "";
        if (type == SourceType.CurseForge)
        {
            var list = await CurseForgeAPI.GetModPackList(version, page, filter: filter,
                sortField: sort switch
                {
                    0 => 1,
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    4 => 6,
                    _ => 2
                }, sortOrder: sort switch
                {
                    0 => 1,
                    1 => 1,
                    2 => 1,
                    3 => 0,
                    4 => 1,
                    _ => 1
                }, categoryId: categoryId);
            if (list == null)
                return null;
            var list1 = new List<FileItemObj>();
            list.data.ForEach(item =>
            {
                list1.Add(new()
                {
                    Name = item.name,
                    Summary = item.summary,
                    Author = item.authors.GetString(),
                    DownloadCount = item.downloadCount,
                    ModifiedDate = item.dateModified,
                    Logo = item.logo?.url,
                    FileType = FileType.ModPack,
                    SourceType = SourceType.CurseForge,
                    Data = item
                });
            });

            return list1;
        }
        else if (type == SourceType.Modrinth)
        {
            var list = await ModrinthAPI.GetModPackList(version, page, filter: filter, sortOrder: sort, categoryId: categoryId);
            if (list == null)
                return null;
            var list1 = new List<FileItemObj>();
            list.hits.ForEach(item =>
            {
                list1.Add(new()
                {
                    Name = item.title,
                    Summary = item.description,
                    Author = item.author,
                    DownloadCount = item.downloads,
                    ModifiedDate = item.date_modified,
                    Logo = item.icon_url,
                    FileType = FileType.ModPack,
                    SourceType = SourceType.Modrinth,
                    Data = item
                });
            });

            return list1;
        }

        return null;
    }

    public static async Task<List<FileDisplayObj>?> GetPackFile(SourceType type, string id,
        int page, string? mc, Loaders loader, FileType type1 = FileType.ModPack)
    {
        if (type == SourceType.CurseForge)
        {
            var list = await CurseForgeAPI.GetCurseForgeFiles(id, mc, page, type1 == FileType.Mod ? loader : Loaders.Normal);
            if (list == null)
                return null;

            var list1 = new List<FileDisplayObj>();
            list.data.ForEach(item =>
            {
                list1.Add(new()
                {
                    ID = item.modId.ToString(),
                    ID1 = item.id.ToString(),
                    Name = item.displayName,
                    Size = UIUtils.MakeFileSize1(item.fileLength),
                    Download = item.downloadCount,
                    Time = DateTime.Parse(item.fileDate).ToString(),
                    FileType = type1,
                    SourceType = SourceType.CurseForge,
                    Data = item
                });
            });

            return list1;
        }
        else if (type == SourceType.Modrinth)
        {
            var list = await ModrinthAPI.GetFileVersions(id, mc, loader);
            if (list == null)
                return null;

            var list1 = new List<FileDisplayObj>();
            list.ForEach(item =>
            {
                var file = item.files.FirstOrDefault(a => a.primary) ?? item.files[0];
                list1.Add(new()
                {
                    ID = item.project_id,
                    ID1 = item.id,
                    Name = item.name,
                    Size = UIUtils.MakeFileSize1(file.size),
                    Download = item.downloads,
                    Time = DateTime.Parse(item.date_published).ToString(),
                    FileType = type1,
                    SourceType = SourceType.Modrinth,
                    Data = item
                });
            });

            return list1;
        }

        return null;
    }

    public static List<SourceType> GetSourceList(FileType type)
    {
        return type switch
        {
            FileType.Mod =>
                [
                    SourceType.CurseForge,
                    SourceType.Modrinth,
                    SourceType.McMod
                ],
            FileType.DataPacks or FileType.Resourcepack =>
                [
                    SourceType.CurseForge,
                    SourceType.Modrinth,
                ],
            FileType.Shaderpack =>
                [
                    SourceType.CurseForge,
                    SourceType.Modrinth,
                ],
            FileType.World =>
                [
                    SourceType.CurseForge,
                ],
            _ => [],
        };
    }

    public static async Task<List<FileItemObj>?> GetList(FileType now, SourceType type, string? version, string? filter, int page, int sort, string categoryId, Loaders loader)
    {
        version ??= "";
        filter ??= "";
        if (type == SourceType.CurseForge)
        {
            var list = now switch
            {
                FileType.Mod => await CurseForgeAPI.GetModList(version, page, filter: filter,
                    sortField: sort switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 3,
                        3 => 4,
                        4 => 6,
                        _ => 2
                    }, sortOrder: sort switch
                    {
                        0 => 1,
                        1 => 1,
                        2 => 1,
                        3 => 0,
                        4 => 1,
                        _ => 1
                    }, categoryId: categoryId, loader: loader),
                FileType.World => await CurseForgeAPI.GetWorldList(version, page, filter: filter,
                    sortField: sort switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 3,
                        3 => 4,
                        4 => 6,
                        _ => 2
                    }, sortOrder: sort switch
                    {
                        0 => 1,
                        1 => 1,
                        2 => 1,
                        3 => 0,
                        4 => 1,
                        _ => 1
                    }, categoryId: categoryId),
                FileType.Resourcepack => await CurseForgeAPI.GetResourcepackList(version, page, filter: filter,
                    sortField: sort switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 3,
                        3 => 4,
                        4 => 6,
                        _ => 2
                    }, sortOrder: sort switch
                    {
                        0 => 1,
                        1 => 1,
                        2 => 1,
                        3 => 0,
                        4 => 1,
                        _ => 1
                    }, categoryId: categoryId),
                FileType.DataPacks => await CurseForgeAPI.GetDataPacksList(version, page, filter: filter,
                    sortField: sort switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 3,
                        3 => 4,
                        4 => 6,
                        _ => 2
                    }, sortOrder: sort switch
                    {
                        0 => 1,
                        1 => 1,
                        2 => 1,
                        3 => 0,
                        4 => 1,
                        _ => 1
                    }),
                FileType.Shaderpack => await CurseForgeAPI.GetShadersList(version, page, filter: filter,
                    sortField: sort switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 3,
                        3 => 4,
                        4 => 6,
                        _ => 2
                    }, sortOrder: sort switch
                    {
                        0 => 1,
                        1 => 1,
                        2 => 1,
                        3 => 0,
                        4 => 1,
                        _ => 1
                    }),
                _ => null
            };
            if (list == null)
                return null;
            var list1 = new List<FileItemObj>();
            var modlist = new List<string>();
            list.data.ForEach(item =>
            {
                modlist.Add(item.id.ToString());
            });
            var list2 = await ColorMCAPI.GetMcModFromCF(modlist);
            list.data.ForEach(item =>
            {
                var id = item.id.ToString();
                list1.Add(new()
                {
                    ID = id,
                    Name = item.name,
                    Summary = item.summary,
                    Author = item.authors.GetString(),
                    DownloadCount = item.downloadCount,
                    ModifiedDate = item.dateModified,
                    Logo = item.logo?.url,
                    FileType = now,
                    SourceType = SourceType.CurseForge,
                    Data = item,
                    Data1 = list2?.TryGetValue(id, out var data1) == true ? data1 : null
                });
            });

            return list1;
        }
        else if (type == SourceType.Modrinth)
        {
            var list = now switch
            {
                FileType.Mod => await ModrinthAPI.GetModList(version, page, filter: filter, sortOrder: sort, categoryId: categoryId, loader: loader),
                FileType.Resourcepack => await ModrinthAPI.GetResourcepackList(version, page, filter: filter, sortOrder: sort, categoryId: categoryId),
                FileType.DataPacks => await ModrinthAPI.GetDataPackList(version, page, filter: filter, sortOrder: sort, categoryId: categoryId),
                FileType.Shaderpack => await ModrinthAPI.GetShaderpackList(version, page, filter: filter, sortOrder: sort, categoryId: categoryId),
                _ => null
            };
            if (list == null)
                return null;
            var list1 = new List<FileItemObj>();

            var modlist = new List<string>();
            list.hits.ForEach(item =>
            {
                modlist.Add(item.project_id);
            });
            var list2 = await ColorMCAPI.GetMcModFromMO(modlist);
            list.hits.ForEach(item =>
            {
                list1.Add(new()
                {
                    ID = item.project_id,
                    Name = item.title,
                    Summary = item.description,
                    Author = item.author,
                    DownloadCount = item.downloads,
                    ModifiedDate = item.date_modified,
                    Logo = item.icon_url,
                    FileType = FileType.ModPack,
                    SourceType = SourceType.Modrinth,
                    Data = item,
                    Data1 = list2?.TryGetValue(item.project_id, out var data1) == true
                        ? data1 : null
                });
            });

            return list1;
        }

        return null;
    }

    public static async Task<(DownloadItemObj? Item, ModInfoObj? Info,
        List<DownloadModModel>? List)>
        DownloadMod(GameSettingObj obj, CurseForgeModObj.Data? data)
    {
        if (data == null)
            return (null, null, null);

        string path = obj.GetModsPath();

        var res = new Dictionary<string, DownloadModModel>();
        if (data.dependencies != null && data.dependencies.Count > 0)
        {
            var res1 = await CurseForgeAPI.GetModDependencies(data, obj.Version, obj.Loader, true);

            foreach (var item1 in res1)
            {
                if (res.ContainsKey(item1.Info.ModId) || obj.Mods.ContainsKey(item1.Info.ModId))
                {
                    continue;
                }

                List<string> version = [];
                List<(DownloadItemObj Item, ModInfoObj Info)> items = [];
                foreach (var item2 in item1.List)
                {
                    version.Add(item2.displayName);
                    items.Add((item2.MakeModDownloadObj(obj, path), item2.MakeModInfo(InstancesPath.Name11)));
                }
                res.Add(item1.Info.ModId, new()
                {
                    Download = false,
                    Name = item1.Info.Name,
                    ModVersion = version,
                    Items = items,
                    SelectVersion = 0,
                    Optional = item1.Info.Opt
                });
            }
        }

        return (data.MakeModDownloadObj(obj, path), data.MakeModInfo(InstancesPath.Name11), res.Values.ToList());
    }

    public static async Task<(DownloadItemObj? Item, ModInfoObj? Info,
        List<DownloadModModel>? List)>
        DownloadMod(GameSettingObj obj, ModrinthVersionObj? data)
    {
        if (data == null)
            return (null, null, null);

        var res = new Dictionary<string, DownloadModModel>();
        if (data.dependencies != null && data.dependencies.Count > 0)
        {
            var list2 = await ModrinthAPI.GetModDependencies(data, obj.Version, obj.Loader);
            foreach (var item1 in list2)
            {
                if (res.ContainsKey(item1.Info.ModId) || obj.Mods.ContainsKey(item1.Info.ModId))
                    continue;
                List<string> version = [];
                List<(DownloadItemObj Item, ModInfoObj Info)> items = [];
                foreach (var item2 in item1.List)
                {
                    version.Add(item2.name);
                    items.Add((item2.MakeModDownloadObj(obj), item2.MakeModInfo()));
                }
                res.Add(item1.Info.ModId, new()
                {
                    Download = false,
                    Name = item1.Info.Name,
                    ModVersion = version,
                    Items = items,
                    SelectVersion = 0
                });
            }
        }

        return (data.MakeModDownloadObj(obj), data.MakeModInfo(), res.Values.ToList());
    }

    public static async Task<bool> DownloadMod(GameSettingObj obj,
        IList<(DownloadItemObj Item, ModInfoObj Info, ModDisplayModel Mod)> list)
    {
        foreach (var (Item, Info, Mod) in list)
        {
            PathHelper.Delete(Mod.Local);
        }

        var list1 = new List<DownloadItemObj>();
        foreach (var (Item, Info, Mod) in list)
        {
            Item.Later = (s) =>
            {
                obj.AddModInfo(Info);
            };
            list1.Add(Item);
        }
        return await DownloadManager.Start(list1);
    }

    public static async Task<bool> DownloadMod(GameSettingObj obj,
        IList<(DownloadItemObj Item, ModInfoObj Info)> list)
    {
        var list1 = new List<DownloadItemObj>();
        foreach (var (Item, Info) in list)
        {
            Item.Later = (s) =>
            {
                obj.AddModInfo(Info);
            };
            list1.Add(Item);
        }
        return await DownloadManager.Start(list1);
    }

    public static async Task<bool> Download(FileType type, GameSettingObj obj, CurseForgeModObj.Data? data)
    {
        if (data == null)
            return false;

        data.FixDownloadUrl();
        bool res;
        DownloadItemObj item;
        switch (type)
        {
            case FileType.World:
                item = new DownloadItemObj()
                {
                    Name = data.displayName,
                    Url = data.downloadUrl,
                    Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + data.fileName),
                    SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault(),
                    Overwrite = true
                };

                res = await DownloadManager.Start([item]);
                if (!res)
                {
                    return false;
                }

                return await GameBinding.AddWorld(obj, item.Local);
            case FileType.Resourcepack:
                return await DownloadManager.Start([new()
                {
                    Name = data.displayName,
                    Url = data.downloadUrl,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + data.fileName),
                    SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault(),
                    Overwrite = true
                }]);
            case FileType.Shaderpack:
                return await DownloadManager.Start([new()
                {
                    Name = data.displayName,
                    Url = data.downloadUrl,
                    Local = Path.GetFullPath(obj.GetShaderpacksPath() + "/" + data.fileName),
                    SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault(),
                    Overwrite = true
                }]);
            default:
                return false;
        }
    }

    public static async Task<bool> Download(FileType type, GameSettingObj obj, ModrinthVersionObj? data)
    {
        if (data == null)
        {
            return false;
        }

        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];

        return type switch
        {
            FileType.Resourcepack => await DownloadManager.Start([new()
                {
                    Name = data.name,
                    Url = file.url,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + file.filename),
                    SHA1 = file.hashes.sha1,
                    Overwrite = true
                }]),
            FileType.Shaderpack => await DownloadManager.Start([new()
                {
                    Name = data.name,
                    Url = file.url,
                    Local = Path.GetFullPath(obj.GetShaderpacksPath() + "/" + file.filename),
                    SHA1 = file.hashes.sha1,
                    Overwrite = true
                }]),
            _ => false,
        };
    }

    public static async Task<bool> Download(WorldObj obj1, CurseForgeModObj.Data? data)
    {
        if (data == null)
        {
            return false;
        }

        data.FixDownloadUrl();

        return await DownloadManager.Start([new()
        {
            Name = data.displayName,
            Url = data.downloadUrl,
            Local = Path.GetFullPath(obj1.GetWorldDataPacksPath() + "/" + data.fileName),
            SHA1 = data.hashes.Where(a => a.algo == 1)
                .Select(a => a.value).FirstOrDefault(),
            Overwrite = true
        }]);
    }

    public static async Task<bool> Download(WorldObj obj1, ModrinthVersionObj? data)
    {
        if (data == null)
        {
            return false;
        }

        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];

        return await DownloadManager.Start([new()
        {
            Name = data.name,
            Url = file.url,
            Local = Path.GetFullPath(obj1.GetWorldDataPacksPath() + "/" + file.filename),
            SHA1 = file.hashes.sha1,
            Overwrite = true
        }]);
    }

    public static string? GetUrl(this FileItemObj obj)
    {
        if (obj.SourceType == SourceType.CurseForge)
        {
            return (obj.Data as CurseForgeObjList.Data)!.links.websiteUrl;
        }
        else if (obj.SourceType == SourceType.Modrinth)
        {
            var obj1 = (obj.Data as ModrinthSearchObj.Hit)!;
            return obj.FileType switch
            {
                FileType.ModPack => "https://modrinth.com/modpack/",
                FileType.Shaderpack => "https://modrinth.com/shaders/",
                FileType.Resourcepack => "https://modrinth.com/resourcepacks/",
                FileType.DataPacks => "https://modrinth.com/datapacks/",
                _ => "https://modrinth.com/mod/"
            } + obj1.project_id;
        }

        return null;
    }

    public static string? GetMcMod(this FileItemObj obj)
    {
        if ((obj.SourceType == SourceType.CurseForge
            || obj.SourceType == SourceType.Modrinth)
            && obj.Data1 is McModSearchItemObj obj1)
        {
            return $"https://www.mcmod.cn/class/{obj1.mcmod_id}.html";
        }
        else if (obj.SourceType == SourceType.McMod)
        {
            var obj2 = (obj.Data as McModSearchItemObj)!;
            return $"https://www.mcmod.cn/class/{obj2.mcmod_id}.html";
        }

        return null;
    }

    public static async Task<List<OptifineObj>?> GetOptifine()
    {
        var res = await OptifineAPI.GetOptifineVersion();
        return res.Item2;
    }

    public static Task<(bool, string?)> DownloadOptifine(GameSettingObj obj, OptifineObj item)
    {
        return OptifineAPI.DownloadOptifine(obj, item);
    }

    public static async Task<List<(DownloadItemObj Item, ModInfoObj Info, ModDisplayModel Mod)>>
        CheckModUpdate(GameSettingObj game, List<ModDisplayModel> mods)
    {
        string path = game.GetModsPath();
        var list = new ConcurrentBag<(DownloadItemObj Item, ModInfoObj Info, ModDisplayModel Mod)>();
        await Parallel.ForEachAsync(mods, async (item, cancel) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.PID) || string.IsNullOrWhiteSpace(item.FID))
                {
                    return;
                }

                var type1 = FuntionUtils.CheckNotNumber(item.PID) || FuntionUtils.CheckNotNumber(item.FID) ?
                   SourceType.Modrinth : SourceType.CurseForge;

                var list1 = await GetPackFile(type1, item.PID, 0,
                   game.Version, game.Loader, FileType.Mod);
                if (list1 == null || list1.Count == 0)
                {
                    return;
                }
                var item1 = list1[0];
                if (item1.ID1 != item.FID)
                {
                    switch (type1)
                    {
                        case SourceType.CurseForge:
                            if (item1.Data is CurseForgeModObj.Data data)
                            {
                                list.Add((data.MakeModDownloadObj(game, path), data.MakeModInfo(InstancesPath.Name11), item));
                            }
                            break;
                        case SourceType.Modrinth:
                            if (item1.Data is ModrinthVersionObj data1)
                            {
                                list.Add((data1.MakeModDownloadObj(game), data1.MakeModInfo(), item));
                            }
                            break;
                    }
                    Dispatcher.UIThread.Post(() =>
                    {
                        item.IsNew = true;
                    });
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("Gui.Error16"), e);
            }
        });

        return [.. list];
    }

    public static void OpenMcmod(ModDisplayModel obj)
    {
        BaseBinding.OpUrl($"https://search.mcmod.cn/s?key={obj.Name}");
    }

    public static async Task<List<FileItemObj>?> SearchMcmod(string key, int page)
    {
        var list = await ColorMCAPI.GetMcModFromName(key, page);
        if (list == null)
            return null;

        var list1 = new List<FileItemObj>();
        foreach (var item in list.Values)
        {
            list1.Add(new()
            {
                Logo = item.mcmod_icon.StartsWith("//")
                    ? "https:" + item.mcmod_icon : item.mcmod_icon,
                Name = item.mcmod_name,
                Summary = item.mcmod_text,
                Author = item.mcmod_author,
                FileType = FileType.Mod,
                SourceType = SourceType.McMod,
                Data = item,
                ModifiedDate = item.mcmod_time.ToString()
            });
        }

        return list1;
    }

    public static void OpenWeb(WebType type)
    {
        BaseBinding.OpUrl(type switch
        {
            WebType.Guide => "https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/Main.md",
            WebType.Mcmod => "https://www.mcmod.cn/",
            WebType.Github => "https://www.github.com/Coloryr/ColorMC",
            WebType.Sponsor => "https://coloryr.github.io/sponsor.html",
            WebType.Minecraft => "https://www.minecraft.net/",
            WebType.NetFrpSakura => "https://www.natfrp.com/user/",
            WebType.Apache2_0 => "https://www.apache.org/licenses/LICENSE-2.0.html",
            WebType.MIT => "https://mit-license.org/",
            WebType.MiSans => "https://hyperos.mi.com/font/",
            WebType.BSD => "https://licenses.nuget.org/BSD-2-Clause",
            WebType.NetOpenFrp => "https://console.openfrp.net/home/",
            _ => "https://colormc.coloryr.com"
        });
    }

    public static Task<List<string>?> GetForgeSupportVersion()
    {
        return ForgeAPI.GetSupportVersion(false, BaseClient.Source);
    }
    public static Task<List<string>?> GetFabricSupportVersion()
    {
        return FabricAPI.GetSupportVersion(BaseClient.Source);
    }
    public static Task<List<string>?> GetQuiltSupportVersion()
    {
        return QuiltAPI.GetSupportVersion(BaseClient.Source);
    }

    public static Task<List<string>?> GetNeoForgeVersion(string version)
    {
        return ForgeAPI.GetVersionList(true, version, BaseClient.Source);
    }

    public static Task<List<string>?> GetNeoForgeSupportVersion()
    {
        return ForgeAPI.GetSupportVersion(true, BaseClient.Source);
    }
    public static Task<List<string>?> GetForgeVersion(string version)
    {
        return ForgeAPI.GetVersionList(false, version, BaseClient.Source);
    }

    public static Task<List<string>?> GetFabricVersion(string version)
    {
        return FabricAPI.GetLoaders(version, BaseClient.Source);
    }

    public static Task<List<string>?> GetQuiltVersion(string version)
    {
        return QuiltAPI.GetLoaders(version, BaseClient.Source);
    }

    public static async Task<List<string>?> GetOptifineVersion(string version)
    {
        var list = await GetOptifine();
        if (list == null)
        {
            return null;
        }
        var list1 = new List<string>();
        foreach (var item in list)
        {
            if (item.MCVersion == version)
            {
                list1.Add(item.Version);
            }
        }

        return list1;
    }

    public static async Task<List<string>?> GetOptifineSupportVersion()
    {
        return await OptifineAPI.GetSupportVersion();
    }

    public static Task<string?> Push(string data)
    {
        return McloAPI.Push(data);
    }

    public static Task<string?> GetNewLog()
    {
        return ColorMCAPI.GetNewLog();
    }

    public static async Task<List<CloudServerModel>?> GetCloudServer()
    {
        var list = await ColorMCAPI.GetCloudServer();
        if (list == null || list?["list"] is not { } list1)
        {
            return null;
        }

        return list1.ToObject<List<CloudServerModel>>();
    }

    public static Task<bool> ShareIP(string token, string ip)
    {
        return ColorMCAPI.PutCloudServer(token, ip);
    }
}
