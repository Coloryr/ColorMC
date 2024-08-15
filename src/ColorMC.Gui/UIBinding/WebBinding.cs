using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.OptiFine;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UIBinding;

public static class WebBinding
{
    private static readonly List<string> PCJavaType = ["Adoptium", "Zulu", "Dragonwell", "OpenJ9", "Graalvm"];

    private static readonly List<string> PhoneJavaType = ["PojavLauncherTeam"];

    private const string Android = "Android";
    private const string Arm64 = "Arm64";

    /// <summary>
    /// ��ȡ���ϰ��б�
    /// </summary>
    /// <param name="type"></param>
    /// <param name="version"></param>
    /// <param name="filter"></param>
    /// <param name="page"></param>
    /// <param name="sort"></param>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    public static async Task<List<FileItemDisplayModel>?> GetModPackList(SourceType type, string? version,
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
            var list1 = new List<FileItemDisplayModel>();
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
            var list1 = new List<FileItemDisplayModel>();
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

    /// <summary>
    /// ��ȡ���ϰ��ļ��б�
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <param name="page"></param>
    /// <param name="mc"></param>
    /// <param name="loader"></param>
    /// <param name="type1"></param>
    /// <returns></returns>
    public static async Task<List<FileDisplayModel>?> GetPackFileList(SourceType type, string id,
        int page, string? mc, Loaders loader, FileType type1 = FileType.ModPack)
    {
        if (type == SourceType.CurseForge)
        {
            var list = await CurseForgeAPI.GetCurseForgeFiles(id, mc, page, type1 == FileType.Mod ? loader : Loaders.Normal);
            if (list == null)
                return null;

            var list1 = new List<FileDisplayModel>();
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

            var list1 = new List<FileDisplayModel>();
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

    /// <summary>
    /// ��ȡ����Դ�б�
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
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

    /// <summary>
    /// ��ȡ�ļ��б�
    /// </summary>
    /// <param name="now"></param>
    /// <param name="type"></param>
    /// <param name="version"></param>
    /// <param name="filter"></param>
    /// <param name="page"></param>
    /// <param name="sort"></param>
    /// <param name="categoryId"></param>
    /// <param name="loader"></param>
    /// <returns></returns>
    public static async Task<List<FileItemDisplayModel>?> GetList(FileType now, SourceType type, string? version, string? filter, int page, int sort, string categoryId, Loaders loader)
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
            var list1 = new List<FileItemDisplayModel>();
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
            var list1 = new List<FileItemDisplayModel>();

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

    /// <summary>
    /// ��ȡģ�������б�
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<ModDownloadRes> GetDownloadModList(GameSettingObj obj, CurseForgeModObj.Data? data)
    {
        if (data == null)
        {
            return new();
        }

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
                List<DownloadModArg> items = [];
                foreach (var item2 in item1.List)
                {
                    version.Add(item2.displayName);
                    items.Add(new()
                    {
                        Item = item2.MakeModDownloadObj(path),
                        Info = item2.MakeModInfo(InstancesPath.Name11)
                    });
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

        return new()
        {
            Item = data.MakeModDownloadObj(path),
            Info = data.MakeModInfo(InstancesPath.Name11),
            List = [.. res.Values]
        };
    }

    /// <summary>
    /// ��ȡģ�������б�
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<ModDownloadRes> GetDownloadModList(GameSettingObj obj, ModrinthVersionObj? data)
    {
        if (data == null)
        {
            return new();
        }

        var res = new Dictionary<string, DownloadModModel>();
        if (data.dependencies != null && data.dependencies.Count > 0)
        {
            var list2 = await ModrinthAPI.GetModDependencies(data, obj.Version, obj.Loader);
            foreach (var item1 in list2)
            {
                if (res.ContainsKey(item1.ModId) || obj.Mods.ContainsKey(item1.ModId)
                    || item1.ModId == data.project_id)
                {
                    continue;
                }
                List<string> version = [];
                List<DownloadModArg> items = [];
                foreach (var item2 in item1.List)
                {
                    version.Add(item2.name);
                    items.Add(new()
                    {
                        Item = item2.MakeModDownloadObj(obj),
                        Info = item2.MakeModInfo()
                    });
                }
                res.Add(item1.ModId, new()
                {
                    Download = false,
                    Name = item1.Name,
                    ModVersion = version,
                    Items = items,
                    SelectVersion = 0
                });
            }
        }

        return new()
        {
            Item = data.MakeModDownloadObj(obj),
            Info = data.MakeModInfo(),
            List = [.. res.Values]
        };
    }

    /// <summary>
    /// ����ģ��
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static async Task<bool> DownloadMod(GameSettingObj obj, ICollection<DownloadModItemArg> list)
    {
        foreach (var item in list)
        {
            PathHelper.Delete(item.Model.Local);
        }

        var list1 = new List<DownloadItemObj>();
        foreach (var item in list)
        {
            item.Item.Later = (s) =>
            {
                obj.AddModInfo(item.Info);
            };
            list1.Add(item.Item);
        }
        return await DownloadManager.StartAsync(list1);
    }

    /// <summary>
    /// ����ģ��
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static async Task<bool> DownloadMod(GameSettingObj obj, ICollection<DownloadModArg> list)
    {
        var list1 = new List<DownloadItemObj>();
        foreach (var item in list)
        {
            item.Item.Later = (s) =>
            {
                obj.AddModInfo(item.Info);
            };
            list1.Add(item.Item);
        }
        return await DownloadManager.StartAsync(list1);
    }

    /// <summary>
    /// ������Դ
    /// </summary>
    /// <param name="type"></param>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <returns></returns>
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

                res = await DownloadManager.StartAsync([item]);
                if (!res)
                {
                    return false;
                }

                return await GameBinding.AddWorld(obj, item.Local);
            case FileType.Resourcepack:
                return await DownloadManager.StartAsync([new()
                {
                    Name = data.displayName,
                    Url = data.downloadUrl,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + data.fileName),
                    SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault(),
                    Overwrite = true
                }]);
            case FileType.Shaderpack:
                return await DownloadManager.StartAsync([new()
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

    /// <summary>
    /// ������Դ
    /// </summary>
    /// <param name="type"></param>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<bool> Download(FileType type, GameSettingObj obj, ModrinthVersionObj? data)
    {
        if (data == null)
        {
            return false;
        }

        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];

        return type switch
        {
            FileType.Resourcepack => await DownloadManager.StartAsync([new()
                {
                    Name = data.name,
                    Url = file.url,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + file.filename),
                    SHA1 = file.hashes.sha1,
                    Overwrite = true
                }]),
            FileType.Shaderpack => await DownloadManager.StartAsync([new()
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

    /// <summary>
    /// ���ص�ͼ��Դ
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<bool> Download(WorldObj obj1, CurseForgeModObj.Data? data)
    {
        if (data == null)
        {
            return false;
        }

        data.FixDownloadUrl();

        return await DownloadManager.StartAsync([new()
        {
            Name = data.displayName,
            Url = data.downloadUrl,
            Local = Path.GetFullPath(obj1.GetWorldDataPacksPath() + "/" + data.fileName),
            SHA1 = data.hashes.Where(a => a.algo == 1)
                .Select(a => a.value).FirstOrDefault(),
            Overwrite = true
        }]);
    }

    /// <summary>
    /// ���ص�ͼ��Դ
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<bool> Download(WorldObj obj1, ModrinthVersionObj? data)
    {
        if (data == null)
        {
            return false;
        }

        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];

        return await DownloadManager.StartAsync([new()
        {
            Name = data.name,
            Url = file.url,
            Local = Path.GetFullPath(obj1.GetWorldDataPacksPath() + "/" + file.filename),
            SHA1 = file.hashes.sha1,
            Overwrite = true
        }]);
    }

    /// <summary>
    /// ��ȡԭվ��ַ
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string? GetUrl(this FileItemDisplayModel obj)
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

    /// <summary>
    /// ��ȡMcMod��ַ
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string? GetMcMod(this FileItemDisplayModel obj)
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

    /// <summary>
    /// ��ȡ�����޸��б�
    /// </summary>
    /// <returns></returns>
    public static async Task<List<OptifineObj>?> GetOptifine()
    {
        return await OptifineAPI.GetOptifineVersion();
    }

    /// <summary>
    /// ���ظ����޸�
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static Task<MessageRes> DownloadOptifine(GameSettingObj obj, OptifineObj item)
    {
        return OptifineAPI.DownloadOptifine(obj, item);
    }

    /// <summary>
    /// ���ģ�����
    /// </summary>
    /// <param name="game"></param>
    /// <param name="mods"></param>
    /// <returns></returns>
    public static async Task<List<DownloadModItemArg>> CheckModUpdate(GameSettingObj game, List<ModDisplayModel> mods)
    {
        string path = game.GetModsPath();
        var list = new ConcurrentBag<DownloadModItemArg>();
        await Parallel.ForEachAsync(mods, async (item, cancel) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.PID) || string.IsNullOrWhiteSpace(item.FID))
                {
                    return;
                }

                var type1 = DownloadItemHelper.TestSourceType(item.PID, item.FID);

                var list1 = await GetPackFileList(type1, item.PID, 0,
                   game.Version, game.Loader, FileType.Mod);
                if (list1 == null || list1.Count == 0)
                {
                    return;
                }
                var item1 = list1[0];
                if (item1.ID1 != item.FID)
                {
                    if (item1.Data is CurseForgeModObj.Data data)
                    {
                        list.Add(new()
                        {
                            Item = data.MakeModDownloadObj(path),
                            Info = data.MakeModInfo(InstancesPath.Name11),
                            Model = item
                        });
                    }
                    else if (item1.Data is ModrinthVersionObj data1)
                    {
                        list.Add(new()
                        {
                            Item = data1.MakeModDownloadObj(game),
                            Info = data1.MakeModInfo(),
                            Model = item
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("WebBinding.Error1"), e);
            }
        });

        foreach (var item in list)
        {
            item.Model.IsNew = true;
        }

        return [.. list];
    }

    /// <summary>
    /// ��McMod��ҳ
    /// </summary>
    /// <param name="obj"></param>
    public static void OpenMcmod(ModDisplayModel obj)
    {
        BaseBinding.OpUrl($"https://search.mcmod.cn/s?key={obj.Name}");
    }

    /// <summary>
    /// ����ģ��
    /// </summary>
    /// <param name="name"></param>
    /// <param name="page"></param>
    /// <param name="loader"></param>
    /// <param name="version"></param>
    /// <param name="modtype"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    public static async Task<List<FileItemDisplayModel>?> SearchMcmod(string name, int page, Loaders loader, string version, string modtype, int sort)
    {
        var list = await ColorMCAPI.GetMcMod(name, page, loader, version, modtype, sort);
        if (list == null)
            return null;

        var list1 = new List<FileItemDisplayModel>();
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
                ModifiedDate = item.mcmod_update_time.ToString()
            });
        }

        return list1;
    }

    /// <summary>
    /// ����ҳ
    /// </summary>
    /// <param name="type"></param>
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
            WebType.Live2DCore => "https://www.live2d.com/download/cubism-sdk/download-native/",
            WebType.ColorMCDownload => "https://github.com/Coloryr/ColorMC/releases",
            WebType.EditSkin => "https://www.minecraft.net/en-us/msaprofile/mygames/editskin",
            WebType.LittleSkinEditSkin => "https://littleskin.cn/user/closet",
            WebType.UIGuide => "https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/Main.md#%E5%AE%A2%E6%88%B7%E7%AB%AF%E5%AE%.9A%E5%88%B6",
            WebType.UIGuide1 => "https://gitee.com/Coloryr/ColorMC_Pic/blob/master/guide/Main.md#%E5%AE%A2%E6%88%B7%E7%AB%AF%E5%AE%9A%E5%88%B6",
            _ => "https://colormc.coloryr.com"
        });
    }

    /// <summary>
    /// ��ȡForge֧�ֵ���Ϸ�汾
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetForgeSupportVersion()
    {
        return ForgeAPI.GetSupportVersion(false, WebClient.Source);
    }
    /// <summary>
    /// ��ȡFabric֧�ֵ���Ϸ�汾
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetFabricSupportVersion()
    {
        return FabricAPI.GetSupportVersion(WebClient.Source);
    }
    /// <summary>
    /// ��ȡQuilt֧�ֵ���Ϸ�汾
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetQuiltSupportVersion()
    {
        return QuiltAPI.GetSupportVersion(WebClient.Source);
    }
    /// <summary>
    /// ��ȡNeoForge֧�ֵ���Ϸ�汾
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetNeoForgeSupportVersion()
    {
        return ForgeAPI.GetSupportVersion(true, WebClient.Source);
    }
    /// <summary>
    /// ��ȡOptifine֧�ֵ���Ϸ�汾
    /// </summary>
    /// <returns></returns>
    public static async Task<List<string>?> GetOptifineSupportVersion()
    {
        return await OptifineAPI.GetSupportVersion();
    }

    /// <summary>
    /// ��ȡForge�汾
    /// </summary>
    /// <param name="version">��Ϸ�汾</param>
    /// <returns></returns>
    public static Task<List<string>?> GetForgeVersion(string version)
    {
        return ForgeAPI.GetVersionList(false, version, WebClient.Source);
    }
    /// <summary>
    /// ��ȡFabric�汾
    /// </summary>
    /// <param name="version">��Ϸ�汾</param>
    /// <returns></returns>
    public static Task<List<string>?> GetFabricVersion(string version)
    {
        return FabricAPI.GetLoaders(version, WebClient.Source);
    }
    /// <summary>
    /// ��ȡQuilt�汾
    /// </summary>
    /// <param name="version">��Ϸ�汾</param>
    /// <returns></returns>
    public static Task<List<string>?> GetQuiltVersion(string version)
    {
        return QuiltAPI.GetLoaders(version, WebClient.Source);
    }
    /// <summary>
    /// ��ȡNeoForge�汾
    /// </summary>
    /// <param name="version">��Ϸ�汾</param>
    /// <returns></returns>
    public static Task<List<string>?> GetNeoForgeVersion(string version)
    {
        return ForgeAPI.GetVersionList(true, version, WebClient.Source);
    }
    /// <summary>
    /// ��ȡOptifine�汾
    /// </summary>
    /// <param name="version">��Ϸ�汾</param>
    /// <returns></returns>
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

    /// <summary>
    /// ��Mclo�ϴ���־
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Task<string?> PushMclo(string data)
    {
        return McloAPI.Push(data);
    }

    /// <summary>
    /// ��ȡColorMC������־
    /// </summary>
    /// <returns></returns>
    public static Task<string?> GetNewLog()
    {
        return ColorMCAPI.GetNewLog();
    }

    /// <summary>
    /// ��ȡӳ���б�
    /// </summary>
    /// <returns></returns>
    public static async Task<List<NetFrpCloudServerModel>?> GetFrpServer()
    {
        var list = await ColorMCAPI.GetCloudServer();
        if (list == null || list?["list"] is not { } list1)
        {
            return null;
        }

        GameSocket.Clear();
        var list2 = list1.ToObject<List<NetFrpCloudServerModel>>();
        list2?.ForEach(GameSocket.AddServerInfo);

        return list2;
    }

    /// <summary>
    /// ����ӳ��
    /// </summary>
    /// <param name="token"></param>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static Task<bool> ShareIP(string token, string ip)
    {
        return ColorMCAPI.PutCloudServer(token, ip);
    }

    public static async Task<GetJavaListRes> GetJavaList(int type, int os, int mainversion)
    {
        if (SystemInfo.Os == OsType.Android)
        {
            var res = await GetPojavLauncherTeamList();
            if (res == null)
            {
                return new();
            }

            return new()
            {
                Res = true,
                Arch = [Arm64],
                Os = [Android],
                MainVersion = ["", "8", "17", "21"],
                Download = res
            };
        }

        if (mainversion == -1)
            mainversion = 0;
        if (os == -1)
            os = 0;

        switch (type)
        {
            case 0:
                var res = await GetAdoptiumList(mainversion, os);
                if (res.Res)
                {
                    return new()
                    {
                        Res = true,
                        Arch = res.Arch,
                        Os = AdoptiumApi.SystemType,
                        MainVersion = await AdoptiumApi.GetJavaVersion(),
                        Download = res.Download
                    };
                }
                break;
            case 1:
                return await GetZuluList();
            case 2:
                var res1 = await GetDragonwellList();
                if (res1 != null)
                {
                    return new()
                    {
                        Res = true,
                        Download = res1
                    };
                }
                break;
            case 3:
                return await GetOpenJ9List();
            case 4:
                return new()
                {
                    Res = true,
                    Download = GetGraalvmList()
                };
        }

        return new();
    }

    private static async Task<GetJavaListRes> GetZuluList()
    {
        try
        {
            var list = await ZuluApi.GetJavaList();
            if (list == null)
            {
                return new();
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

            var list1 = new List<JavaDownloadModel>();
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

            return new()
            {
                Res = true,
                Arch = arch,
                Os = os,
                MainVersion = mainversion,
                Download = list1
            };
        }
        catch (Exception e)
        {
            WindowManager.ShowError(App.Lang("WebBinding.Error2"), e);
            return new();
        }
    }

    private static List<JavaDownloadModel> GetGraalvmList()
    {
        return
        [
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
        ];
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

    private static async Task<GetJavaAdoptiumListRes> GetAdoptiumList(int mainversion, int os)
    {
        try
        {
            var versions = await AdoptiumApi.GetJavaVersion();
            if (versions == null)
            {
                return new();
            }
            var version = versions[mainversion];
            var list = await AdoptiumApi.GetJavaList(version, os);
            if (list == null)
            {
                return new();
            }

            var arch = new List<string>
            {
                ""
            };
            arch.AddRange(from item in list
                          group item by item.binary.architecture into newGroup
                          orderby newGroup.Key descending
                          select newGroup.Key);

            var list3 = new List<JavaDownloadModel>();
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

            return new()
            {
                Res = true,
                Arch = arch,
                Download = list3
            };
        }
        catch (Exception e)
        {
            WindowManager.ShowError(App.Lang("WebBinding.Error2"), e);
            return new();
        }
    }

    private static void AddDragonwell(List<JavaDownloadModel> list, DragonwellObj.Item item)
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

    private static async Task<List<JavaDownloadModel>?> GetDragonwellList()
    {
        try
        {
            var list = await Dragonwell.GetJavaList();
            if (list == null)
            {
                return null;
            }

            var list1 = new List<JavaDownloadModel>();

            AddDragonwell(list1, list.extended);
            AddDragonwell(list1, list.standard);

            return list1;
        }
        catch (Exception e)
        {
            WindowManager.ShowError(App.Lang("WebBinding.Error2"), e);
            return null;
        }
    }

    private static async Task<GetJavaListRes> GetOpenJ9List()
    {
        try
        {
            var res = await OpenJ9Api.GetJavaList();
            if (res == null)
            {
                return new();
            }
            var list1 = new List<JavaDownloadModel>();

            foreach (var item in res.Download!)
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

            return new()
            {
                Res = true,
                Arch = res.Arch,
                Os = res.Os,
                MainVersion = res.MainVersion,
                Download = list1
            };
        }
        catch (Exception e)
        {
            WindowManager.ShowError(App.Lang("WebBinding.Error2"), e);
            return new();
        }
    }

    private static async Task<List<JavaDownloadModel>?> GetPojavLauncherTeamList()
    {
        try
        {
            var list = new List<JavaDownloadModel>();
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

    /// <summary>
    /// ��ȡJava����
    /// </summary>
    /// <returns></returns>
    public static List<string> GetJavaType()
    {
        return SystemInfo.Os == OsType.Android ? PhoneJavaType : PCJavaType;
    }

    /// <summary>
    /// ��ע����ҳ
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    public static void OpenRegister(AuthType type, string? name)
    {
        switch (type)
        {
            case AuthType.OAuth:
                BaseBinding.OpUrl("https://www.minecraft.net/zh-hans/login");
                break;
            case AuthType.Nide8:
                BaseBinding.OpUrl($"https://login.mc-user.com:233/{name}/loginreg");
                break;
            case AuthType.LittleSkin:
                BaseBinding.OpUrl("https://littleskin.cn/auth/register");
                break;
        }
    }

    /// <summary>
    /// ��ȡMinecraft News
    /// </summary>
    /// <returns></returns>
    public static async Task<MinecraftNewObj?> LoadNews()
    {
        try
        {
            return await MinecraftAPI.GetMinecraftNew();
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("WebBinding.Error3"), e);
            return null;
        }
    }
}
