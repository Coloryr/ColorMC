using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ColorMC.Core;
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
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Frp;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UIBinding;

public static class WebBinding
{
    private static readonly List<string> PCJavaType = ["Adoptium", "Zulu", "Dragonwell", "OpenJ9", "Graalvm"];
#if Phone
    private static readonly List<string> PhoneJavaType = ["PojavLauncherTeam"];
#endif

    /// <summary>
    /// 获取整合包列表
    /// </summary>
    /// <param name="type"></param>
    /// <param name="version"></param>
    /// <param name="filter"></param>
    /// <param name="page"></param>
    /// <param name="sort"></param>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    public static async Task<ModPackListRes> GetModPackList(SourceType type, string? version,
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
            {
                return new();
            }
            var list1 = new List<FileItemModel>();
            var modlist = new List<string>();
            list.Data.ForEach(item =>
            {
                modlist.Add(item.Id.ToString());
            });
            var list2 = await ColorMCAPI.GetMcModFromCF(modlist);
            list.Data.ForEach(item =>
            {
                var id = item.Id.ToString();
                list1.Add(new(item, FileType.ModPack, list2?.TryGetValue(id, out var data1) == true ? data1 : null));
            });

            return new()
            {
                List = list1,
                Count = list.Pagination.TotalCount
            };
        }
        else if (type == SourceType.Modrinth)
        {
            var list = await ModrinthAPI.GetModPackList(version, page, filter: filter, sortOrder: sort, categoryId: categoryId);
            if (list == null)
            {
                return new();
            }
            var list1 = new List<FileItemModel>();
            var modlist = new List<string>();
            list.Hits.ForEach(item =>
            {
                modlist.Add(item.ProjectId);
            });
            var list2 = await ColorMCAPI.GetMcModFromMO(modlist);
            list.Hits.ForEach(item =>
            {
                list1.Add(new(item, FileType.ModPack, list2?.TryGetValue(item.ProjectId, out var data1) == true ? data1 : null));
            });

            return new()
            {
                List = list1,
                Count = list.Hits.Count
            };
        }

        return new();
    }

    /// <summary>
    /// 获取文件列表
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <param name="page"></param>
    /// <param name="mc"></param>
    /// <param name="loader"></param>
    /// <param name="type1"></param>
    /// <returns></returns>
    public static async Task<FileListRes> GetFileList(SourceType type, string id,
        int page, string? mc, Loaders loader, FileType type1 = FileType.ModPack)
    {
        if (type == SourceType.CurseForge)
        {
            var list = await CurseForgeAPI.GetCurseForgeFiles(id, mc, page, type1 == FileType.Mod ? loader : Loaders.Normal);
            if (list == null)
                return new();

            var list1 = new List<FileVersionItemModel>();
            list.Data.ForEach(item =>
            {
                list1.Add(new(item, type1));
            });

            return new()
            {
                List = list1,
                Count = list.Pagination.TotalCount
            };
        }
        else if (type == SourceType.Modrinth)
        {
            var list = await ModrinthAPI.GetFileVersions(id, mc, loader);
            if (list == null)
                return new();

            var list1 = new List<FileVersionItemModel>();
            list.ForEach(item =>
            {
                list1.Add(new(item, type1));
            });

            return new()
            {
                List = list1,
                Count = list.Count
            };
        }

        return new();
    }

    /// <summary>
    /// 获取下载源列表
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
    /// 获取文件列表
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
    public static async Task<ModPackListRes> GetList(FileType now, SourceType type, string? version, string? filter, int page, int sort, string categoryId, Loaders loader)
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
                return new();
            var list1 = new List<FileItemModel>();
            var modlist = new List<string>();
            list.Data.ForEach(item =>
            {
                modlist.Add(item.Id.ToString());
            });
            var list2 = await ColorMCAPI.GetMcModFromCF(modlist);
            list.Data.ForEach(item =>
            {
                var id = item.Id.ToString();
                list1.Add(new(item, now, list2?.TryGetValue(id, out var data1) == true ? data1 : null));
            });

            return new()
            {
                List = list1,
                Count = list.Pagination.TotalCount
            };
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
                return new();
            var list1 = new List<FileItemModel>();
            var modlist = new List<string>();
            list.Hits.ForEach(item =>
            {
                modlist.Add(item.ProjectId);
            });
            var list2 = await ColorMCAPI.GetMcModFromMO(modlist);
            list.Hits.ForEach(item =>
            {
                list1.Add(new(item, now, list2?.TryGetValue(item.ProjectId, out var data1) == true ? data1 : null));
            });

            return new()
            {
                List = list1,
                Count = list.Hits.Count
            };
        }

        return new();
    }

    /// <summary>
    /// 获取模组下载列表
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<ModDownloadRes> GetDownloadModList(GameSettingObj obj, CurseForgeModObj.DataObj? data)
    {
        if (data == null)
        {
            return new();
        }

        var res = new Dictionary<string, FileModVersionModel>();
        if (data.Dependencies != null && data.Dependencies.Count > 0)
        {
            var res1 = await CurseForgeHelper.GetModDependenciesAsync(data, obj.Version, obj.Loader);

            foreach (var item1 in res1)
            {
                var modid = item1.ModId.ToString();
                if (res.ContainsKey(modid) || obj.Mods.ContainsKey(modid) || data.Id == item1.ModId)
                {
                    continue;
                }

                List<string> version = [];
                List<DownloadModArg> items = [];
                foreach (var item2 in item1.List)
                {
                    version.Add(item2.DisplayName);
                    items.Add(new()
                    {
                        Item = item2.MakeModDownloadObj(obj),
                        Info = item2.MakeModInfo(Names.NameGameModDir)
                    });
                }
                res.Add(modid, new(item1.Name, version, items, item1.Opt));
            }
        }

        return new()
        {
            Item = data.MakeModDownloadObj(obj),
            Info = data.MakeModInfo(Names.NameGameModDir),
            List = [.. res.Values]
        };
    }

    /// <summary>
    /// 获取模组下载列表
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

        var res = new Dictionary<string, FileModVersionModel>();
        if (data.Dependencies != null && data.Dependencies.Count > 0)
        {
            var list2 = await ModrinthHelper.GetModDependenciesAsync(data, obj.Version, obj.Loader);
            foreach (var item1 in list2)
            {
                if (res.ContainsKey(item1.ModId) || obj.Mods.ContainsKey(item1.ModId)
                    || item1.ModId == data.ProjectId)
                {
                    continue;
                }
                List<string> version = [];
                List<DownloadModArg> items = [];
                foreach (var item2 in item1.List)
                {
                    version.Add(item2.Name);
                    items.Add(new()
                    {
                        Item = item2.MakeModDownloadObj(obj),
                        Info = item2.MakeModInfo(Names.NameGameModDir)
                    });
                }
                res.Add(item1.ModId, new(item1.Name, version, items, false));
            }
        }

        return new()
        {
            Item = data.MakeModDownloadObj(obj),
            Info = data.MakeModInfo(Names.NameGameModDir),
            List = [.. res.Values]
        };
    }

    public static void UpgradeMod(GameSettingObj obj, ICollection<ModUpgradeModel> list)
    {
        WindowManager.ShowAddUpgade(obj, list);
    }

    /// <summary>
    /// 下载模组
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static async Task<bool> DownloadMod(GameSettingObj obj, ICollection<DownloadModArg> list)
    {
        var list1 = new List<FileItemObj>();
        var setting = GameGuiSetting.ReadConfig(obj);
        foreach (var item in list)
        {
            item.Item.Later = (s) =>
            {
                obj.AddModInfo(item.Info);
                if (item.Old is { } old)
                {
                    PathHelper.Delete(item.Old.Local);
                }
            };

            if (item.Old is { } old && item.Item.Sha1 != null)
            {
                if (setting.ModName.TryGetValue(old.Sha1, out var value))
                {
                    setting.ModName[item.Item.Sha1] = value;
                    setting.ModName.Remove(old.Sha1);
                }

                if (item.Old.Disable)
                {
                    item.Item.Local = Path.ChangeExtension(item.Item.Local, Names.NameDisableExt);
                    item.Info.File = Path.ChangeExtension(item.Info.File, Names.NameDisableExt);
                }
            }

            list1.Add(item.Item);
        }

        GameGuiSetting.WriteConfig(obj, setting);
        return await DownloadManager.StartAsync(list1);
    }

    /// <summary>
    /// 下载资源
    /// </summary>
    /// <param name="type"></param>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<bool> Download(FileType type, GameSettingObj obj, CurseForgeModObj.DataObj? data)
    {
        if (data == null)
            return false;

        data.FixDownloadUrl();
        bool res;
        FileItemObj item;
        switch (type)
        {
            case FileType.World:
                item = new FileItemObj()
                {
                    Name = data.DisplayName,
                    Url = data.DownloadUrl,
                    Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + data.FileName),
                    Sha1 = data.Hashes.Where(a => a.Algo == 1)
                        .Select(a => a.Value).FirstOrDefault(),
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
                    Name = data.DisplayName,
                    Url = data.DownloadUrl,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + data.FileName),
                    Sha1 = data.Hashes.Where(a => a.Algo == 1)
                        .Select(a => a.Value).FirstOrDefault(),
                    Overwrite = true
                }]);
            case FileType.Shaderpack:
                return await DownloadManager.StartAsync([new()
                {
                    Name = data.DisplayName,
                    Url = data.DownloadUrl,
                    Local = Path.GetFullPath(obj.GetShaderpacksPath() + "/" + data.FileName),
                    Sha1 = data.Hashes.Where(a => a.Algo == 1)
                        .Select(a => a.Value).FirstOrDefault(),
                    Overwrite = true
                }]);
            default:
                return false;
        }
    }

    /// <summary>
    /// 下载资源
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

        var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];

        return type switch
        {
            FileType.Resourcepack => await DownloadManager.StartAsync([new()
                {
                    Name = data.Name,
                    Url = file.Url,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + file.Filename),
                    Sha1 = file.Hashes.Sha1,
                    Overwrite = true
                }]),
            FileType.Shaderpack => await DownloadManager.StartAsync([new()
                {
                    Name = data.Name,
                    Url = file.Url,
                    Local = Path.GetFullPath(obj.GetShaderpacksPath() + "/" + file.Filename),
                    Sha1 = file.Hashes.Sha1,
                    Overwrite = true
                }]),
            _ => false,
        };
    }

    /// <summary>
    /// 下载存档资源
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<bool> Download(WorldObj obj1, CurseForgeModObj.DataObj? data)
    {
        if (data == null)
        {
            return false;
        }

        data.FixDownloadUrl();

        return await DownloadManager.StartAsync([new()
        {
            Name = data.DisplayName,
            Url = data.DownloadUrl,
            Local = Path.GetFullPath(obj1.GetWorldDataPacksPath() + "/" + data.FileName),
            Sha1 = data.Hashes.Where(a => a.Algo == 1)
                .Select(a => a.Value).FirstOrDefault(),
            Overwrite = true
        }]);
    }

    /// <summary>
    /// 下载存档资源
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

        var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];

        return await DownloadManager.StartAsync([new()
        {
            Name = data.Name,
            Url = file.Url,
            Local = Path.GetFullPath(obj1.GetWorldDataPacksPath() + "/" + file.Filename),
            Sha1 = file.Hashes.Sha1,
            Overwrite = true
        }]);
    }

    /// <summary>
    /// 获取原站地址
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetUrl(this CurseForgeObjList.DataObj obj)
    {
        return obj.Links.WebsiteUrl;
    }

    public static string GetUrl(this ModrinthSearchObj.HitObj obj, FileType fileType)
    {
        return fileType switch
        {
            FileType.ModPack => "https://modrinth.com/modpack/",
            FileType.Shaderpack => "https://modrinth.com/shaders/",
            FileType.Resourcepack => "https://modrinth.com/resourcepacks/",
            FileType.DataPacks => "https://modrinth.com/datapacks/",
            _ => "https://modrinth.com/mod/"
        } + obj.ProjectId;
    }

    /// <summary>
    /// 获取McMod地址
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string? GetUrl(this McModSearchItemObj obj)
    {
        return $"https://www.mcmod.cn/{(obj.McmodType == 0 ? "class" : "modpack")}/{obj.McmodId}.html";
    }

    /// <summary>
    /// 获取高清修复列表
    /// </summary>
    /// <returns></returns>
    public static async Task<List<OptifineObj>?> GetOptifine()
    {
        return await OptifineAPI.GetOptifineVersion();
    }

    /// <summary>
    /// 下载高清修复
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static Task<MessageRes> DownloadOptifine(GameSettingObj obj, OptifineObj item)
    {
        return OptifineAPI.DownloadOptifine(obj, item);
    }

    /// <summary>
    /// 检查模组更新
    /// </summary>
    /// <param name="game"></param>
    /// <param name="mods"></param>
    /// <returns></returns>
    public static async Task<List<ModUpgradeModel>> CheckModUpdate(GameSettingObj game, List<ModDisplayModel> mods)
    {
        string path = game.GetModsPath();
        var list = new ConcurrentBag<ModUpgradeModel>();
        await Parallel.ForEachAsync(mods, async (item, cancel) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.PID) || string.IsNullOrWhiteSpace(item.FID))
                {
                    return;
                }

                var type1 = GameDownloadHelper.TestSourceType(item.PID, item.FID);

                var res = await GetFileList(type1, item.PID, 0,
                   game.Version, game.Loader, FileType.Mod);
                var list1 = res.List;

                if (list1 == null || list1.Count == 0
                || list1[0].ID1 == item.FID)
                {
                    return;
                }

                item.IsNew = true;

                List<string> version = [];
                List<DownloadModArg> items = [];
                foreach (var item1 in list1)
                {
                    if (item1.ID1 == item.FID)
                    {
                        continue;
                    }

                    if (item1.Data is CurseForgeModObj.DataObj data)
                    {
                        version.Add(data.DisplayName);
                        items.Add(new()
                        {
                            Item = data.MakeModDownloadObj(game),
                            Info = data.MakeModInfo(Names.NameGameModDir)
                        });
                    }
                    else if (item1.Data is ModrinthVersionObj data1)
                    {
                        version.Add(data1.Name);
                        items.Add(new()
                        {
                            Item = data1.MakeModDownloadObj(game),
                            Info = data1.MakeModInfo(Names.NameGameModDir)
                        });
                    }
                }

                list.Add(new(item.Obj, item.Name, version, items));
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("WebBinding.Error1"), e);
            }
        });

        return [.. list];
    }

    /// <summary>
    /// 打开McMod网页
    /// </summary>
    /// <param name="obj"></param>
    public static void OpenMcmod(ModDisplayModel obj)
    {
        BaseBinding.OpenUrl($"https://search.mcmod.cn/s?key={obj.Name}");
    }

    /// <summary>
    /// 搜索模组
    /// </summary>
    /// <param name="name"></param>
    /// <param name="page"></param>
    /// <param name="loader"></param>
    /// <param name="version"></param>
    /// <param name="modtype"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    public static async Task<List<FileItemModel>?> SearchMcmod(string name, int page, Loaders loader, string version, string modtype, int sort)
    {
        var list = await ColorMCAPI.GetMcMod(name, page, loader, version, modtype, sort);
        if (list == null)
            return null;

        var list1 = new List<FileItemModel>();
        foreach (var item in list.Values)
        {
            list1.Add(new(item, FileType.Mod));
        }

        return list1;
    }

    /// <summary>
    /// 打开网页
    /// </summary>
    /// <param name="type"></param>
    public static void OpenWeb(WebType type)
    {
        BaseBinding.OpenUrl(type switch
        {
            WebType.Guide => "https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/README.md",
            WebType.Guide1 => "https://gitee.com/Coloryr/ColorMC_Pic/blob/master/guide/README.md",
            WebType.Mcmod => "https://www.mcmod.cn/",
            WebType.Github => "https://www.github.com/Coloryr/ColorMC",
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
            WebType.UIGuide => "https://github.com/Coloryr/ColorMC/blob/master/CustomGui.md",
            WebType.UIGuide1 => "https://gitee.com/Coloryr/ColorMC/blob/master/CustomGui.md",
            WebType.Sponsor => "https://afdian.com/a/Color_yr",
            _ => "https://www.coloryr.com/wordpress/?page_id=27"
        });
    }

    /// <summary>
    /// 获取Forge支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetForgeSupportVersion()
    {
        return ForgeAPI.GetSupportVersion(false, CoreHttpClient.Source);
    }
    /// <summary>
    /// 获取Fabric支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetFabricSupportVersion()
    {
        return FabricAPI.GetSupportVersion(CoreHttpClient.Source);
    }
    /// <summary>
    /// 获取Quilt支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetQuiltSupportVersion()
    {
        return QuiltAPI.GetSupportVersion(CoreHttpClient.Source);
    }
    /// <summary>
    /// 获取NeoForge支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetNeoForgeSupportVersion()
    {
        return ForgeAPI.GetSupportVersion(true, CoreHttpClient.Source);
    }
    /// <summary>
    /// 获取Optifine支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static async Task<List<string>?> GetOptifineSupportVersion()
    {
        return await OptifineAPI.GetSupportVersion();
    }

    /// <summary>
    /// 获取Forge版本
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns></returns>
    public static Task<List<string>?> GetForgeVersion(string version)
    {
        return ForgeAPI.GetVersionList(false, version, CoreHttpClient.Source);
    }
    /// <summary>
    /// 获取Fabric版本
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns></returns>
    public static Task<List<string>?> GetFabricVersion(string version)
    {
        return FabricAPI.GetLoaders(version, CoreHttpClient.Source);
    }
    /// <summary>
    /// 获取Quilt版本
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns></returns>
    public static Task<List<string>?> GetQuiltVersion(string version)
    {
        return QuiltAPI.GetLoaders(version, CoreHttpClient.Source);
    }
    /// <summary>
    /// 获取NeoForge版本
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns></returns>
    public static Task<List<string>?> GetNeoForgeVersion(string version)
    {
        return ForgeAPI.GetVersionList(true, version, CoreHttpClient.Source);
    }
    /// <summary>
    /// 获取Optifine版本
    /// </summary>
    /// <param name="version">游戏版本</param>
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
    /// 向Mclo上传日志
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Task<string?> PushMclo(string data)
    {
        return McloAPI.Push(data);
    }

    /// <summary>
    /// 获取ColorMC更新日志
    /// </summary>
    /// <returns></returns>
    public static Task<string?> GetNewLog()
    {
        return ColorMCCloudAPI.GetNewLog();
    }

    /// <summary>
    /// 获取映射列表
    /// </summary>
    /// <returns></returns>
    public static async Task<List<FrpCloudObj>?> GetFrpServer(string version)
    {
        var list = await ColorMCCloudAPI.GetCloudServer(version);
        if (list == null || list?["list"] is not { } list1)
        {
            return null;
        }

        LaunchSocketUtils.Clear();
        var list2 = list1.ToObject<List<FrpCloudObj>>();
        list2?.ForEach(LaunchSocketUtils.AddServerInfo);

        return list2;
    }

    /// <summary>
    /// 共享映射
    /// </summary>
    /// <param name="token"></param>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static Task<bool> ShareIP(string token, string ip, FrpShareModel model)
    {
        return ColorMCCloudAPI.PutCloudServer(token, ip, model);
    }

    public static async Task<GetJavaListRes> GetJavaList(int type, int os, int mainversion)
    {
#if Phone
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
#else
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
#endif
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
                          group item by item.Arch + '_' + item.HwBitness into newGroup
                          orderby newGroup.Key descending
                          select newGroup.Key);

            var mainversion = new List<string>
            {
                ""
            };
            mainversion.AddRange(from item in list
                                 group item by item.JavaVersion[0] into newGroup
                                 orderby newGroup.Key descending
                                 select newGroup.Key.ToString());

            var os = new List<string>
            {
                ""
            };
            os.AddRange(from item in list
                        group item by item.Os into newGroup
                        orderby newGroup.Key descending
                        select newGroup.Key.ToString());

            var list1 = new List<JavaDownloadModel>();
            foreach (var item in list)
            {
                if (item.Name.EndsWith(".deb") || item.Name.EndsWith(".rpm")
                    || item.Name.EndsWith(".msi") || item.Name.EndsWith(".dmg"))
                {
                    continue;
                }

                list1.Add(new()
                {
                    Name = item.Name,
                    Arch = item.Arch + '_' + item.HwBitness,
                    Os = item.Os,
                    MainVersion = item.ZuluVersion[0].ToString(),
                    Version = ToStr(item.ZuluVersion),
                    Size = UIUtils.MakeFileSize1(0),
                    Url = item.Url,
                    Sha256 = item.Sha256Hash,
                    File = item.Name
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
                          group item by item.Binary.Architecture into newGroup
                          orderby newGroup.Key descending
                          select newGroup.Key);

            var list3 = new List<JavaDownloadModel>();
            foreach (var item in list)
            {
                if (item.Binary.ImageType == "debugimage")
                    continue;
                list3.Add(new()
                {
                    Name = item.Binary.ScmRef + "_" + item.Binary.ImageType,
                    Arch = item.Binary.Architecture,
                    Os = item.Binary.Os,
                    MainVersion = version,
                    Version = item.Version.OpenjdkVersion,
                    Size = UIUtils.MakeFileSize1(item.Binary.Package.Size),
                    Url = item.Binary.Package.Link,
                    Sha256 = item.Binary.Package.Checksum,
                    File = item.Binary.Package.Name
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

    private static void AddDragonwell(List<JavaDownloadModel> list, DragonwellObj.ItemObj item)
    {
        string main = "8";
        string version = item.Version8;
        string file;
        if (item.Xurl8 != null)
        {
            file = Path.GetFileName(item.Xurl8);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Xurl8,
                File = file
            });
        }
        if (item.Aurl8 != null)
        {
            file = Path.GetFileName(item.Aurl8);
            list.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Aurl8,
                File = file
            });
        }
        if (item.Wurl8 != null)
        {
            file = Path.GetFileName(item.Wurl8);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Wurl8,
                File = file
            });
        }

        main = "11";
        version = item.Version11;
        if (item.Xurl11 != null)
        {
            file = Path.GetFileName(item.Xurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Xurl11,
                File = file
            });
        }
        if (item.Aurl11 != null)
        {
            file = Path.GetFileName(item.Aurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Aurl11,
                File = file
            });
        }
        if (item.Apurl11 != null)
        {
            file = Path.GetFileName(item.Apurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "x64_alpine",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Apurl11,
                File = file
            });
        }
        if (item.Wurl11 != null)
        {
            file = Path.GetFileName(item.Wurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Wurl11,
                File = file
            });
        }
        if (item.Rurl11 != null)
        {
            file = Path.GetFileName(item.Rurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "riscv64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Rurl11,
                File = file
            });
        }

        main = "17";
        version = item.Version17;
        if (item.Xurl17 != null)
        {
            file = Path.GetFileName(item.Xurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Xurl17,
                File = file
            });
        }
        if (item.Aurl17 != null)
        {
            file = Path.GetFileName(item.Aurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Aurl17,
                File = file
            });
        }
        if (item.Apurl17 != null)
        {
            file = Path.GetFileName(item.Apurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "x64_alpine",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Apurl17,
                File = file
            });
        }
        if (item.Wurl17 != null)
        {
            file = Path.GetFileName(item.Wurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.Wurl17,
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

            AddDragonwell(list1, list.Extended);
            AddDragonwell(list1, list.Standard);

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
                var temp = item.Name.Split("<br>");
                if (temp.Length != 3)
                {
                    continue;
                }
                var version = temp[0].Replace("<b>", "").Replace("</b>", "");
                if (item.Jdk != null)
                    list1.Add(new()
                    {
                        Name = temp[2] + " " + temp[1] + "_jdk",
                        Os = item.Os,
                        Arch = item.Arch,
                        MainVersion = item.Version.ToString(),
                        Version = version,
                        Size = "0",
                        Url = item.Jdk.Opt1.DownloadLink,
                        Sha256 = item.Jdk.Opt1.Checksum,
                        File = Path.GetFileName(item.Jdk.Opt1.DownloadLink)
                    });
                if (item.Jre != null)
                    list1.Add(new()
                    {
                        Name = temp[2] + " " + temp[1] + "_jre",
                        Os = item.Os,
                        Arch = item.Arch,
                        MainVersion = item.Version.ToString(),
                        Version = version,
                        Size = "0",
                        Url = item.Jre.Opt1.DownloadLink,
                        Sha256 = item.Jre.Opt1.Checksum,
                        File = Path.GetFileName(item.Jre.Opt1.DownloadLink)
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

#if Phone
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
                    Os = "Android",
                    Arch = "Arm64",
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
                    Os = "Android",
                    Arch = "Arm64",
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
                    Os = "Android",
                    Arch = "Arm64",
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
#endif

    /// <summary>
    /// 获取Java类型
    /// </summary>
    /// <returns></returns>
    public static List<string> GetJavaType()
    {
#if Phone
        return SystemInfo.Os == OsType.Android ? PhoneJavaType : IosJavaType;
#else
        return PCJavaType;
#endif
    }

    /// <summary>
    /// 打开注册网页
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    public static void OpenRegister(AuthType type, string? name)
    {
        switch (type)
        {
            case AuthType.OAuth:
                BaseBinding.OpenUrl("https://www.minecraft.net/zh-hans/login");
                break;
            case AuthType.Nide8:
                BaseBinding.OpenUrl($"https://login.mc-user.com:233/{name}/loginreg");
                break;
            case AuthType.LittleSkin:
                BaseBinding.OpenUrl("https://littleskin.cn/auth/register");
                break;
        }
    }

    /// <summary>
    /// 获取Minecraft News
    /// </summary>
    /// <returns></returns>
    public static async Task<MinecraftNewObj?> LoadNews(int page)
    {
        try
        {
            return await MinecraftAPI.GetMinecraftNew(page);
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("WebBinding.Error3"), e);
            return null;
        }
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="model">需要下载的内容</param>
    /// <param name="model1">窗口</param>
    /// <returns></returns>
    public static async Task<FileItemObj?> MakeDownload(GameSettingObj obj, FileVersionItemModel model, BaseModel model1)
    {
        ModInfoObj? mod = null;
        if (model.FileType == FileType.Mod && obj.Mods.TryGetValue(model.ID, out mod))
        {
            var res1 = await model1.ShowAsync(App.Lang("AddWindow.Info15"));
            if (!res1)
            {
                return null;
            }
        }
        //分文件类型处理
        if (model.FileType == FileType.Mod)
        {
            try
            {
                var setting = GameGuiSetting.ReadConfig(obj);
                DownloadModArg? arg = null;
                if (model.SourceType == SourceType.CurseForge)
                {
                    var data = (model.Data as CurseForgeModObj.DataObj)!;
                    arg = new DownloadModArg()
                    {
                        Item = data.MakeModDownloadObj(obj),
                        Info = data.MakeModInfo(Names.NameGameModDir),
                        Old = await obj.ReadMod(mod)
                    };
                }
                else
                {
                    var data = (model.Data as ModrinthVersionObj)!;
                    arg = new DownloadModArg()
                    {
                        Item = data.MakeModDownloadObj(obj),
                        Info = data.MakeModInfo(Names.NameGameModDir),
                        Old = await obj.ReadMod(mod)
                    };
                }

                arg.Item.Later = (s) =>
                {
                    obj.AddModInfo(arg.Info);
                    if (arg.Old is { } old)
                    {
                        PathHelper.Delete(arg.Old.Local);
                    }
                };
                //添加模组信息
                if (arg.Old is { } old && arg.Item.Sha1 != null)
                {
                    if (setting.ModName.TryGetValue(old.Sha1, out var value))
                    {
                        setting.ModName[arg.Item.Sha1] = value;
                        setting.ModName.Remove(old.Sha1);
                    }

                    if (arg.Old.Disable)
                    {
                        arg.Item.Local = Path.ChangeExtension(arg.Item.Local, Names.NameDisableExt);
                        arg.Info.File = Path.ChangeExtension(arg.Info.File, Names.NameDisableExt);
                    }
                }

                GameGuiSetting.WriteConfig(obj, setting);

                return arg.Item;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("AddWindow.Error8"), e);
            }
        }
        else if (model.FileType == FileType.Shaderpack)
        {
            if (model.SourceType == SourceType.CurseForge)
            {
                var data = (model.Data as CurseForgeModObj.DataObj)!;
                return new()
                {
                    Name = data.DisplayName,
                    Url = data.DownloadUrl,
                    Local = Path.GetFullPath(obj.GetShaderpacksPath() + "/" + data.FileName),
                    Sha1 = data.Hashes.Where(a => a.Algo == 1)
                        .Select(a => a.Value).FirstOrDefault(),
                    Overwrite = true
                };
            }
            else
            {
                var data = (model.Data as ModrinthVersionObj)!;
                var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];
                return new()
                {
                    Name = data.Name,
                    Url = file.Url,
                    Local = Path.GetFullPath(obj.GetShaderpacksPath() + "/" + file.Filename),
                    Sha1 = file.Hashes.Sha1,
                    Overwrite = true
                };
            }
        }
        else if (model.FileType == FileType.Resourcepack)
        {
            if (model.SourceType == SourceType.CurseForge)
            {
                var data = (model.Data as CurseForgeModObj.DataObj)!;
                return new()
                {
                    Name = data.DisplayName,
                    Url = data.DownloadUrl,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + data.FileName),
                    Sha1 = data.Hashes.Where(a => a.Algo == 1)
                        .Select(a => a.Value).FirstOrDefault(),
                    Overwrite = true
                };
            }
            else
            {
                var data = (model.Data as ModrinthVersionObj)!;
                var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];
                return new()
                {
                    Name = data.Name,
                    Url = file.Url,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + file.Filename),
                    Sha1 = file.Hashes.Sha1,
                    Overwrite = true
                };
            }
        }

        return null;
    }
}
