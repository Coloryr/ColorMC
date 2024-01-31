using Avalonia.Threading;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Java;
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
    private readonly static List<string> PCJavaType =
    ["Adoptium", "Zulu", "Dragonwell", "OpenJ9", "Graalvm"];

    private readonly static List<string> PhoneJavaType =
        ["PojavLauncherTeam"];

    private const string Android = "Android";
    private const string Arm64 = "Arm64";

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
            FileType.DataPacks
            or FileType.Resourcepack
            or FileType.Shaderpack =>
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
        return await App.StartDownload(list1);
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
        return await App.StartDownload(list1);
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

                res = await App.StartDownload([item]);
                if (!res)
                {
                    return false;
                }

                return await GameBinding.AddWorld(obj, item.Local);
            case FileType.Resourcepack:
                return await App.StartDownload([new()
                {
                    Name = data.displayName,
                    Url = data.downloadUrl,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + data.fileName),
                    SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault(),
                    Overwrite = true
                }]);
            case FileType.Shaderpack:
                return await App.StartDownload([new()
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
            FileType.Resourcepack => await App.StartDownload([new()
                {
                    Name = data.name,
                    Url = file.url,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + file.filename),
                    SHA1 = file.hashes.sha1,
                    Overwrite = true
                }]),
            FileType.Shaderpack => await App.StartDownload([new()
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

        return await App.StartDownload([new()
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

        return await App.StartDownload([new()
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
            WebType.Guide1 => "https://gitee.com/Coloryr/ColorMC_Pic/blob/master/guide/Main.md",
            WebType.Mcmod => "https://www.mcmod.cn/",
            WebType.Github => "https://www.github.com/Coloryr/ColorMC",
            WebType.Sponsor => "https://coloryr.github.io/sponsor.html",
            WebType.Minecraft => "https://www.minecraft.net/",
            WebType.SakuraFrp => "https://www.natfrp.com/user/",
            WebType.Apache2_0 => "https://www.apache.org/licenses/LICENSE-2.0.html",
            WebType.MIT => "https://mit-license.org/",
            WebType.MiSans => "https://hyperos.mi.com/font/",
            WebType.BSD => "https://licenses.nuget.org/BSD-2-Clause",
            WebType.OpenFrp => "https://console.openfrp.net/home/",
            WebType.OpenFrpApi => "https://github.com/ZGIT-Network/OPENFRP-APIDOC",
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

    public static async Task<List<NetFrpCloudServerModel>?> GetCloudServer()
    {
        var list = await ColorMCAPI.GetCloudServer();
        if (list == null || list?["list"] is not { } list1)
        {
            return null;
        }

        return list1.ToObject<List<NetFrpCloudServerModel>>();
    }

    public static Task<bool> ShareIP(string token, string ip)
    {
        return ColorMCAPI.PutCloudServer(token, ip);
    }

    public static async Task<(bool, List<string>? Arch, List<string>? Os,
        List<string>? MainVersion, List<JavaDownloadObj>? Download)> GetJavaList(int type, int os, int mainversion)
    {
        if (SystemInfo.Os == OsType.Android)
        {
            var res = await GetPojavLauncherTeamList();
            if (res == null)
            {
                return (false, null, null, null, null);
            }

            return (true, [Arm64], [Android], ["", "8", "17", "21"], res);
        }

        if (mainversion == -1)
            mainversion = 0;
        if (os == -1)
            os = 0;

        switch (type)
        {
            case 0:
                {
                    var res = await GetAdoptiumList(mainversion, os);
                    if (!res.Item1)
                    {
                        return (false, null, null, null, null);
                    }
                    else
                    {
                        return (true, res.Arch, AdoptiumApi.SystemType, await AdoptiumApi.GetJavaVersion(), res.Item3);
                    }
                }
            case 1:
                {
                    return await GetZuluList();
                }
            case 2:
                {
                    var res = await GetDragonwellList();
                    if (res == null)
                    {
                        return (false, null, null, null, null);
                    }
                    else
                    {
                        return (true, null, null, null, res);
                    }
                }
            case 3:
                {
                    return await GetOpenJ9List();
                }
            case 4:
                {
                    return (true, null, null, null, GetGraalvmList());
                }
            default:
                return (false, null, null, null, null);
        }
    }

    private static async Task<(bool, List<string>? Arch, List<string>? Os, List<string>? MainVersion,
        List<JavaDownloadObj>?)> GetZuluList()
    {
        try
        {
            var list = await ZuluApi.GetJavaList();
            if (list == null)
            {
                return (false, null, null, null, null);
            }

            var arch = new List<string>
            {
                ""
            };
            arch.AddRange(from item in list
                          group item by item.arch + '_' + item.hw_bitness into newGroup
                          orderby newGroup.Key descending
                          select newGroup.Key);

            var mainversion = new List<string>
            {
                ""
            };
            mainversion.AddRange(from item in list
                                 group item by item.java_version[0] into newGroup
                                 orderby newGroup.Key descending
                                 select newGroup.Key.ToString());

            var os = new List<string>
            {
                ""
            };
            os.AddRange(from item in list
                        group item by item.os into newGroup
                        orderby newGroup.Key descending
                        select newGroup.Key.ToString());

            var list1 = new List<JavaDownloadObj>();
            foreach (var item in list)
            {
                if (item.name.EndsWith(".deb") || item.name.EndsWith(".rpm")
                    || item.name.EndsWith(".msi") || item.name.EndsWith(".dmg"))
                {
                    continue;
                }

                list1.Add(new()
                {
                    Name = item.name,
                    Arch = item.arch + '_' + item.hw_bitness,
                    Os = item.os,
                    MainVersion = item.zulu_version[0].ToString(),
                    Version = ToStr(item.zulu_version),
                    Size = UIUtils.MakeFileSize1(0),
                    Url = item.url,
                    Sha256 = item.sha256_hash,
                    File = item.name
                });
            }

            return (true, arch, os, mainversion, list1);
        }
        catch (Exception e)
        {
            App.ShowError(App.Lang("Gui.Error46"), e);
            return (false, null, null, null, null);
        }
    }

    private static List<JavaDownloadObj> GetGraalvmList()
    {
        return new List<JavaDownloadObj>()
            {
                new()
                {
                    File = "graalvm-jdk-17_macos-aarch64_bin.tar.gz",
                    Name = "17_macOS_ARM",
                    Url = "https://download.oracle.com/graalvm/17/latest/graalvm-jdk-17_macos-aarch64_bin.tar.gz",
                    Arch = "aarch64",
                    MainVersion = "17",
                    Os = "macos",
                },
                new()
                {
                    File = "graalvm-jdk-17_macos-x64_bin.tar.gz",
                    Name = "17_macOS_x64",
                    Url = "https://download.oracle.com/graalvm/17/latest/graalvm-jdk-17_macos-x64_bin.tar.gz",
                    Arch = "x64",
                    MainVersion = "17",
                    Os = "macos",
                },
                new()
                {
                    File = "graalvm-jdk-17_linux-aarch64_bin.tar.gz",
                    Name = "17_Linux_ARM",
                    Url = "https://download.oracle.com/graalvm/17/latest/graalvm-jdk-17_linux-aarch64_bin.tar.gz",
                    Arch = "aarch64",
                    MainVersion = "17",
                    Os = "linux",
                },
                new()
                {
                    File = "graalvm-jdk-17_linux-x64_bin.tar.gz",
                    Name = "17_Linux_x64",
                    Url = "https://download.oracle.com/graalvm/17/latest/graalvm-jdk-17_linux-x64_bin.tar.gz",
                    Arch = "x64",
                    MainVersion = "17",
                    Os = "linux",
                },
                new()
                {
                    File = "graalvm-jdk-17_windows-x64_bin.zip",
                    Name = "17_Windows_x64",
                    Url = "https://download.oracle.com/graalvm/17/latest/graalvm-jdk-17_windows-x64_bin.zip",
                    Arch = "x64",
                    MainVersion = "17",
                    Os = "windows",
                },

                new()
                {
                    File = "graalvm-jdk-21_macos-aarch64_bin.tar.gz",
                    Name = "21_macOS_ARM",
                    Url = "https://download.oracle.com/graalvm/21/latest/graalvm-jdk-21_macos-aarch64_bin.tar.gz",
                    Arch = "aarch64",
                    MainVersion = "21",
                    Os = "macos",
                },
                new()
                {
                    File = "graalvm-jdk-21_macos-x64_bin.tar.gz",
                    Name = "21_macOS_x64",
                    Url = "https://download.oracle.com/graalvm/21/latest/graalvm-jdk-21_macos-x64_bin.tar.gz",
                    Arch = "x64",
                    MainVersion = "21",
                    Os = "macos",
                },
                new()
                {
                    File = "graalvm-jdk-21_linux-aarch64_bin.tar.gz",
                    Name = "21_Linux_ARM",
                    Url = "https://download.oracle.com/graalvm/21/latest/graalvm-jdk-21_linux-aarch64_bin.tar.gz",
                    Arch = "aarch64",
                    MainVersion = "21",
                    Os = "linux",
                },
                new()
                {
                    File = "graalvm-jdk-21_linux-x64_bin.tar.gz",
                    Name = "21_Linux_x64",
                    Url = "https://download.oracle.com/graalvm/21/latest/graalvm-jdk-21_linux-x64_bin.tar.gz",
                    Arch = "x64",
                    MainVersion = "21",
                    Os = "linux",
                },
                new()
                {
                    File = "graalvm-jdk-21_windows-x64_bin.zip",
                    Name = "21_Windows_x64",
                    Url = "https://download.oracle.com/graalvm/21/latest/graalvm-jdk-21_windows-x64_bin.zip",
                    Arch = "x64",
                    MainVersion = "21",
                    Os = "windows",
                }
            };
    }

    private static string ToStr(List<int> list)
    {
        string a = "";
        foreach (var item in list)
        {
            a += item + ".";
        }
        return a[..^1];
    }

    private static async Task<(bool, List<string>? Arch, List<JavaDownloadObj>?)>
        GetAdoptiumList(int mainversion, int os)
    {
        try
        {
            var versions = await AdoptiumApi.GetJavaVersion();
            if (versions == null)
            {
                return (false, null, null);
            }
            var version = versions[mainversion];
            var list = await AdoptiumApi.GetJavaList(version, os);
            if (list == null)
            {
                return (false, null, null);
            }

            var arch = new List<string>
            {
                ""
            };
            arch.AddRange(from item in list
                          group item by item.binary.architecture into newGroup
                          orderby newGroup.Key descending
                          select newGroup.Key);

            var list3 = new List<JavaDownloadObj>();
            foreach (var item in list)
            {
                if (item.binary.image_type == "debugimage")
                    continue;
                list3.Add(new()
                {
                    Name = item.binary.scm_ref + "_" + item.binary.image_type,
                    Arch = item.binary.architecture,
                    Os = item.binary.os,
                    MainVersion = version,
                    Version = item.version.openjdk_version,
                    Size = UIUtils.MakeFileSize1(item.binary.package.size),
                    Url = item.binary.package.link,
                    Sha256 = item.binary.package.checksum,
                    File = item.binary.package.name
                });
            }

            return (true, arch, list3);
        }
        catch (Exception e)
        {
            App.ShowError(App.Lang("Gui.Error46"), e);
            return (false, null, null);
        }
    }

    private static void AddDragonwell(List<JavaDownloadObj> list, DragonwellObj.Item item)
    {
        string main = "8";
        string version = item.version8;
        string file;
        if (item.xurl8 != null)
        {
            file = Path.GetFileName(item.xurl8);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.xurl8,
                File = file
            });
        }
        if (item.aurl8 != null)
        {
            file = Path.GetFileName(item.aurl8);
            list.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.aurl8,
                File = file
            });
        }
        if (item.wurl8 != null)
        {
            file = Path.GetFileName(item.wurl8);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.wurl8,
                File = file
            });
        }

        main = "11";
        version = item.version11;
        if (item.xurl11 != null)
        {
            file = Path.GetFileName(item.xurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.xurl11,
                File = file
            });
        }
        if (item.aurl11 != null)
        {
            file = Path.GetFileName(item.aurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.aurl11,
                File = file
            });
        }
        if (item.apurl11 != null)
        {
            file = Path.GetFileName(item.apurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "x64_alpine",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.apurl11,
                File = file
            });
        }
        if (item.wurl11 != null)
        {
            file = Path.GetFileName(item.wurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.wurl11,
                File = file
            });
        }
        if (item.rurl11 != null)
        {
            file = Path.GetFileName(item.rurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "riscv64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.rurl11,
                File = file
            });
        }

        main = "17";
        version = item.version17;
        if (item.xurl17 != null)
        {
            file = Path.GetFileName(item.xurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.xurl17,
                File = file
            });
        }
        if (item.aurl17 != null)
        {
            file = Path.GetFileName(item.aurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.aurl17,
                File = file
            });
        }
        if (item.apurl17 != null)
        {
            file = Path.GetFileName(item.apurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "x64_alpine",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.apurl17,
                File = file
            });
        }
        if (item.wurl17 != null)
        {
            file = Path.GetFileName(item.wurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.wurl17,
                File = file
            });
        }
    }

    private static async Task<List<JavaDownloadObj>?> GetDragonwellList()
    {
        try
        {
            var list = await Dragonwell.GetJavaList();
            if (list == null)
            {
                return null;
            }

            var list1 = new List<JavaDownloadObj>();

            AddDragonwell(list1, list.extended);
            AddDragonwell(list1, list.standard);

            return list1;
        }
        catch (Exception e)
        {
            App.ShowError(App.Lang("Gui.Error46"), e);
            return null;
        }
    }

    private static async Task<(bool ok, List<string>? Arch, List<string>? Os,
        List<string>? MainVersion, List<JavaDownloadObj>?)> GetOpenJ9List()
    {
        try
        {
            var (Arch, Os, MainVersion, Data) = await OpenJ9Api.GetJavaList();
            if (Os == null)
            {
                return (false, null, null, null, null);
            }
            var list1 = new List<JavaDownloadObj>();

            foreach (var item in Data!)
            {
                var temp = item.name.Split("<br>");
                if (temp.Length != 3)
                {
                    continue;
                }
                var version = temp[0].Replace("<b>", "").Replace("</b>", "");
                if (item.jdk != null)
                    list1.Add(new()
                    {
                        Name = temp[2] + " " + temp[1] + "_jdk",
                        Os = item.os,
                        Arch = item.arch,
                        MainVersion = item.version.ToString(),
                        Version = version,
                        Size = "0",
                        Url = item.jdk.opt1.downloadLink,
                        Sha256 = item.jdk.opt1.checksum,
                        File = Path.GetFileName(item.jdk.opt1.downloadLink)
                    });
                if (item.jre != null)
                    list1.Add(new()
                    {
                        Name = temp[2] + " " + temp[1] + "_jre",
                        Os = item.os,
                        Arch = item.arch,
                        MainVersion = item.version.ToString(),
                        Version = version,
                        Size = "0",
                        Url = item.jre.opt1.downloadLink,
                        Sha256 = item.jre.opt1.checksum,
                        File = Path.GetFileName(item.jre.opt1.downloadLink)
                    });
            }

            return (true, Arch, Os, MainVersion, list1);
        }
        catch (Exception e)
        {
            App.ShowError(App.Lang("Gui.Error46"), e);
            return (false, null, null, null, null);
        }
    }

    private static async Task<List<JavaDownloadObj>?> GetPojavLauncherTeamList()
    {
        try
        {
            var list = new List<JavaDownloadObj>();
            var res = await ColorMCAPI.GetJavaList();
            if (res == null)
            {
                return null;
            }
            res.Jre8.ForEach(item =>
            {
                list.Add(new()
                {
                    Name = item.Name,
                    Os = Android,
                    Arch = Arm64,
                    MainVersion = "8",
                    Version = item.Name.Split('-')[2],
                    Size = item.Size,
                    Url = item.Url,
                    Sha1 = item.Sha1,
                    File = item.Name
                });
            });
            res.Jre17.ForEach(item =>
            {
                list.Add(new()
                {
                    Name = item.Name,
                    Os = Android,
                    Arch = Arm64,
                    MainVersion = "17",
                    Version = item.Name.Split('-')[2],
                    Size = item.Size,
                    Url = item.Url,
                    Sha1 = item.Sha1,
                    File = item.Name
                });
            });
            res.Jre21.ForEach(item =>
            {
                list.Add(new()
                {
                    Name = item.Name,
                    Os = Android,
                    Arch = Arm64,
                    MainVersion = "21",
                    Version = item.Name.Split('-')[2],
                    Size = item.Size,
                    Url = item.Url,
                    Sha1 = item.Sha1,
                    File = item.Name
                });
            });

            return list;
        }
        catch
        {
            return null;
        }
    }

    public static List<string> GetJavaType()
    {
        return SystemInfo.Os == OsType.Android ? PhoneJavaType : PCJavaType;
    }
}
