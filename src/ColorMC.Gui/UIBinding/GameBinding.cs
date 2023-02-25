using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using DynamicData;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;

namespace ColorMC.Gui.UIBinding;

public static class GameBinding
{
    public static List<GameSettingObj> GetGames()
    {
        return InstancesPath.Games;
    }

    public static List<string> GetGameVersion(bool? type1, bool? type2, bool? type3)
    {
        var list = new List<string>();
        if (VersionPath.Versions == null)
            return list;

        foreach (var item in VersionPath.Versions.versions)
        {
            if (item.type == "release")
            {
                if (type1 == true)
                {
                    list.Add(item.id);
                }
            }
            else if (item.type == "snapshot")
            {
                if (type2 == true)
                {
                    list.Add(item.id);
                }
            }
            else
            {
                if (type3 == true)
                {
                    list.Add(item.id);
                }
            }
        }

        return list;
    }

    public static async Task<bool> AddGame(string name, string version,
        Loaders loaders, string? loaderversion = null, string? group = null)
    {
        var game = new GameSettingObj()
        {
            Name = name,
            Version = version,
            Loader = loaders,
            LoaderVersion = loaderversion,
            GroupName = group
        };

        game = await InstancesPath.CreateVersion(game);

        return game != null;
    }

    public static Task<(bool, GameSettingObj?)> AddPack(string dir, PackType type, string? name, string? group)
    {
        return InstancesPath.InstallFromZip(dir, type, name, group);
    }

    public static Dictionary<string, List<GameSettingObj>> GetGameGroups()
    {
        return InstancesPath.Groups;
    }

    public static async Task<List<FileItemDisplayObj>?> GetPackList(SourceType type, string? version, string? filter, int page, int sortOrder, string categoryId)
    {
        version ??= "";
        filter ??= "";
        if (type == SourceType.CurseForge)
        {
            var list = await CurseForge.GetModPackList(version, page, filter: filter, sortOrder: sortOrder, categoryId: categoryId);
            if (list == null)
                return null;
            var list1 = new List<FileItemDisplayObj>();
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
            var list = await Modrinth.GetModPackList(version, page, filter: filter, sortOrder: sortOrder, categoryId: categoryId);
            if (list == null)
                return null;
            var list1 = new List<FileItemDisplayObj>();
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

    public static List<string> GetCurseForgeSortTypes()
    {
        return new List<string>()
        {
            CurseForgeSortField.Featured.GetName(),
            CurseForgeSortField.Popularity.GetName(),
            CurseForgeSortField.LastUpdated.GetName(),
            CurseForgeSortField.Name.GetName(),
            CurseForgeSortField.Author.GetName(),
            CurseForgeSortField.TotalDownloads.GetName(),
            CurseForgeSortField.Category.GetName(),
            CurseForgeSortField.GameVersion.GetName()
        };
    }

    public static List<string> GetModrinthSortTypes()
    {
        return new List<string>()
        {
            MSortingObj.Relevance.GetName(),
            MSortingObj.Downloads.GetName(),
            MSortingObj.Follows.GetName(),
            MSortingObj.Newest.GetName(),
            MSortingObj.Updated.GetName()
        };
    }

    public static List<string> GetSortOrder()
    {
        return new()
        {
            App.GetLanguage("GameBinding.SortOrder.Item1"),
            App.GetLanguage("GameBinding.SortOrder.Item2")
        };
    }

    public static List<string> GetSourceList()
    {
        return new()
        {
            SourceType.CurseForge.GetName(),
            SourceType.Modrinth.GetName(),
        };
    }

    private static List<string>? CurseForgeGameVersions;

    public static async Task<List<string>?> GetCurseForgeGameVersions()
    {
        if (CurseForgeGameVersions != null)
        {
            return CurseForgeGameVersions;
        }    
        var list = await CurseForge.GetCurseForgeVersionType();
        if (list == null)
        {
            return null;
        }

        list.data.RemoveAll(a =>
        {
            return a.id is 68441 or 615 or 1 or 3 or 2 or 73247 or 75208;
        });

        var list111 = new List<CurseForgeVersionType.Item>();
        list111.AddRange(from item in list.data
                         where item.id > 17
                         orderby item.id descending
                         select item);
        list111.AddRange(from item in list.data
                         where item.id < 18
                         orderby item.id ascending
                         select item);

        var list2 = await CurseForge.GetCurseForgeVersion();
        if (list2 == null)
        {
            return null;
        }

        var list3 = new List<string>
        {
            ""
        };
        foreach (var item in list111)
        {
            var list4 = from item1 in list2.data
                        where item1.type == item.id
                        select item1.versions;
            var list5 = list4.FirstOrDefault();
            if (list5 != null)
            {
                list3.AddRange(list5);
            }
        }

        CurseForgeGameVersions = list3;

        return list3;
    }

    private static List<string>? ModrinthGameVersions;

    public static async Task<List<string>?> GetModrinthGameVersions()
    {
        if(ModrinthGameVersions!=null)
        {
            return ModrinthGameVersions;
        }

        var list = await Modrinth.GetGameVersion();
        if (list == null)
        {
            return null;
        }

        var list1 = new List<string>
        {
            ""
        };

        list1.AddRange(from item in list select item.version);

        ModrinthGameVersions = list1;

        return list1;
    }

    private static CurseForgeCategoriesObj? CurseForgeCategories;

    public static async Task<Dictionary<string, string>?> GetCurseForgeCategories(
        FileType type = FileType.ModPack)
    {
        if (CurseForgeCategories == null)
        {
            var list6 = await CurseForge.GetCategories();
            if (list6 == null)
            {
                return null;
            }

            CurseForgeCategories = list6;
        }

        var list7 = from item2 in CurseForgeCategories.data
                    where item2.classId == type switch
                    {
                        FileType.Mod => CurseForge.ClassMod,
                        FileType.World => CurseForge.ClassWorld,
                        FileType.Resourcepack => CurseForge.ClassResourcepack,
                        _ => CurseForge.ClassModPack
                    }
                    select (item2.name, item2.id);

        return list7.ToDictionary(a => a.id.ToString(), a => a.name);
    }

    private static List<ModrinthCategoriesObj>? ModrinthCategories;

    public static async Task<Dictionary<string, string>?> GetModrinthCategories(
        FileType type = FileType.ModPack)
    {
        if (ModrinthCategories == null)
        {
            var list6 = await Modrinth.GetCategories();
            if (list6 == null)
            {
                return null;
            }

            ModrinthCategories = list6;
        }

        var list7 = from item2 in ModrinthCategories
                    where item2.project_type == type switch
                    {
                        FileType.Shaderpack => Modrinth.ClassShaderpack,
                        FileType.Resourcepack => Modrinth.ClassResourcepack,
                        FileType.ModPack => Modrinth.ClassModPack,
                        _ => Modrinth.ClassMod
                    }
                    && item2.header == "categories"
                    select item2.name;

        return list7.ToDictionary(a => a);
    }

    public static async Task<bool> InstallCurseForge(CurseForgeObj.Data.LatestFiles data,
        CurseForgeObj.Data data1, string? name, string? group)
    {
        var res = await InstancesPath.InstallFromCurseForge(data, name, group);
        if (!res.Item1)
        {
            return false;
        }
        if (data1.logo != null)
        {
            var res1 = await SetGameIconFromUrl(res.Item2!, data1.logo.url);
            if (!res1)
            {
                return false;
            }
        }

        return true;
    }

    public static async Task<bool> InstallModrinth(ModrinthVersionObj data,
        ModrinthSearchObj.Hit data1, string? name, string? group)
    {
        var res = await InstancesPath.InstallFromModrinth(data, name, group);
        if (!res.Item1)
        {
            return false;
        }
        if (data1.icon_url != null)
        {
            var res1 = await SetGameIconFromUrl(res.Item2!, data1.icon_url);
            if (!res1)
            {
                return false;
            }
        }

        return true;
    }

    public static async Task<bool> SetGameIconFromUrl(GameSettingObj obj, string url)
    {
        try
        {
            var data = await BaseClient.GetBytes(url);
            await File.WriteAllBytesAsync(obj.GetIconFile(), data);
            return true;
        }
        catch (Exception e)
        {
            Logs.Error(App.GetLanguage("GameBinding.Error5"), e);
            App.ShowError(App.GetLanguage("GameBinding.Error5"), e);
            return false;
        }
    }

    public static async Task SetGameIconFromFile(Window win, GameSettingObj obj)
    {
        try
        {
            var file = await BaseBinding.OpFile(win,
                App.GetLanguage("GameBinding.Info2"),
                new string[] { "*.png", "*.jpg", "*.bmp" },
                App.GetLanguage("GameBinding.Info3"));
            if (file?.Any() == true)
            {
                var item = file[0];
                var name = item.GetPath();
                var info = await Image.IdentifyAsync(name);
                if (info.Width != info.Height || info.Width > 200 || info.Height > 200)
                {
                    (win as IBaseWindow)?.Info.Show(App.GetLanguage("GameBinding.Error6"));
                    return;
                }
                var data = await File.ReadAllBytesAsync(name);
                await File.WriteAllBytesAsync(obj.GetIconFile(), data);
            }
        }
        catch (Exception e)
        {
            Logs.Error(App.GetLanguage("GameBinding.Error5"), e);
            App.ShowError(App.GetLanguage("GameBinding.Error5"), e);
        }
    }

    public static async Task<List<FileDisplayObj>?> GetPackFile(SourceType type, string id, int page, FileType type1 = FileType.ModPack)
    {
        if (type == SourceType.CurseForge)
        {
            var list = await CurseForge.GetCurseForgeFiles(id, page);
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
            var list = await Modrinth.Version(id);
            if (list == null)
                return null;

            var list1 = new List<FileDisplayObj>();
            list.ForEach(item =>
            {
                var file = item.files.FirstOrDefault(a => a.primary);
                if (file == null)
                {
                    file = item.files[0];
                }
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

    public static async Task<(bool, string?)> Launch(GameSettingObj? obj, bool debug)
    {
        if (obj == null)
        {
            return (false, App.GetLanguage("GameBinding.Error1"));
        }

        if (BaseBinding.Games.ContainsValue(obj))
        {
            return (false, App.GetLanguage("GameBinding.Error4"));
        }

        var login = UserBinding.GetLastUser();
        if (login == null)
        {
            return (false, App.GetLanguage("GameBinding.Error2"));
        }
        if (login.AuthType == AuthType.Offline)
        {
            var have = AuthDatabase.Auths.Keys.Any(a => a.Item2 == AuthType.OAuth);
            if (!have)
            {
                return (false, App.GetLanguage("GameBinding.Error7"));
            }
        }

        if (UserBinding.IsLock(login))
        {
            var res = await App.MainWindow!.Info.ShowWait(App.GetLanguage("GameBinding.Info1"));
            if (!res)
                return (false, App.GetLanguage("GameBinding.Error3"));
        }

        if (debug)
        {
            App.ShowGameEdit(obj, 6);
        }

        return await BaseBinding.Launch(obj, login);
    }

    public static bool AddGameGroup(string name)
    {
        return InstancesPath.AddGroup(name);
    }

    public static void MoveGameGroup(GameSettingObj obj, string? now)
    {
        obj.MoveGameGroup(now);
        App.MainWindow?.Load();
    }

    public static async Task<bool> ReloadVersion()
    {
        await VersionPath.GetFromWeb();

        return VersionPath.Have();
    }

    public static void SaveGame(GameSettingObj obj, string? versi, Loaders loader, string? loadv)
    {
        if (!string.IsNullOrWhiteSpace(versi))
        {
            obj.Version = versi;

            var version1 = VersionPath.Versions!.versions.FirstOrDefault(a => a.id == versi);
            if (version1 != null)
            {
                if (version1.type == "release")
                {
                    obj.GameType = GameType.Release;
                }
                else if (version1.type == "snapshot")
                {
                    obj.GameType = GameType.Snapshot;
                }
                else
                {
                    obj.GameType = GameType.Other;
                }
            }
        }
        obj.Loader = loader;
        if (!string.IsNullOrWhiteSpace(loadv))
        {
            obj.LoaderVersion = loadv;
        }
        obj.Save();
    }

    public static void SetJavaLocal(GameSettingObj obj, string? name, string? local)
    {
        obj.JvmName = name;
        obj.JvmLocal = local;
        obj.Save();
    }

    public static void SetGameJvmMemArg(GameSettingObj obj, uint min, uint max)
    {
        obj.JvmArg ??= new();
        obj.JvmArg.MinMemory = min;
        obj.JvmArg.MaxMemory = max;
        obj.Save();
    }

    public static void SetGameJvmArg(GameSettingObj obj, JvmArgObj obj1)
    {
        obj.JvmArg = obj1;
        obj.Save();
    }

    public static void SetGameWindow(GameSettingObj obj, WindowSettingObj obj1)
    {
        obj.Window = obj1;
        obj.Save();
    }

    public static void SetGameServer(GameSettingObj obj, ServerObj obj1)
    {
        obj.StartServer = obj1;
        obj.Save();
    }

    public static void SetGameProxy(GameSettingObj obj, ProxyHostObj obj1)
    {
        obj.ProxyHost = obj1;
        obj.Save();
    }

    public static Task<List<ModObj>> GetGameMods(GameSettingObj obj)
    {
        return obj.GetMods();
    }

    public static void ModEnDi(ModObj obj)
    {
        if (obj.Disable)
        {
            obj.Enable();
        }
        else
        {
            obj.Disable();
        }
    }

    public static void DeleteMod(ModObj mod)
    {
        string name = new FileInfo(mod.Local).Name;
        foreach (var item in mod.Game.Mods)
        {
            if (item.Value.File == name)
            {
                mod.Game.Mods.Remove(item.Key);
                mod.Game.SaveModInfo();
                break;
            }
        }
        mod.Delete();
    }

    public static void AddMods(GameSettingObj obj, List<string> file)
    {
        obj.AddMods(file);
    }

    public static List<string> GetAllConfig(GameSettingObj obj)
    {
        var list = new List<string>();
        var dir = obj.GetGamePath().Length + 1;

        var file = obj.GetOptionsFile();
        if (!File.Exists(file))
        {
            File.Create(file).Dispose();
        }

        list.Add(obj.GetOptionsFile()[dir..]);
        string con = obj.GetConfigPath();

        var list1 = PathC.GetAllFile(con);
        foreach (var item in list1)
        {
            list.Add(item.FullName[dir..]);
        }

        return list;
    }

    public static string ReadConfigFile(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return File.ReadAllText(Path.GetFullPath(dir + "/" + name));
    }

    public static void SaveConfigFile(GameSettingObj obj, string name, string? text)
    {
        var dir = obj.GetGamePath();

        File.WriteAllText(Path.GetFullPath(dir + "/" + name), text);
    }

    public static Task<List<string>?> GetForgeVersion(string version)
    {
        return ForgeHelper.GetVersionList(version, BaseClient.Source);
    }

    public static Task<List<string>?> GetFabricVersion(string version)
    {
        return FabricHelper.GetLoaders(version, BaseClient.Source);
    }

    public static Task<List<string>?> GetQuiltVersion(string version)
    {
        return QuiltHelper.GetLoaders(version, BaseClient.Source);
    }

    public static async Task<List<WorldDisplayObj>> GetWorlds(GameSettingObj obj)
    {
        var list = new List<WorldDisplayObj>();
        var res = await obj.GetWorlds();
        foreach (var item in res)
        {
            var obj1 = new WorldDisplayObj()
            {
                Name = item.LevelName,
                Mode = LanguageHelper.GetNameWithGameType(item.GameType),
                Time = OtherUtils.TimestampToDataTime(item.LastPlayed).ToString(),
                Local = item.Local,
                Difficulty = LanguageHelper.GetNameWithDifficulty(item.Difficulty),
                Hardcore = item.Hardcore == 1,
                World = item
            };
            if (item.Icon != null)
            {
                using MemoryStream stream = new();
                await stream.WriteAsync(item.Icon);
                stream.Seek(0, SeekOrigin.Begin);
                obj1.Pic = new Avalonia.Media.Imaging.Bitmap(stream);
            }
            list.Add(obj1);
        }

        return list;
    }

    public static async Task<bool> AddWorld(GameSettingObj obj, string file)
    {
        var res = await obj.ImportWorldZip(file);
        if (!res)
        {
            BaseBinding.OpFile(file);
        }

        return res;
    }

    public static void DeleteWorld(WorldObj world)
    {
        world.Remove();
    }

    public static Task ExportWorld(WorldObj world, string file)
    {
        return world.ExportWorldZip(file);
    }

    public static Task ExportGame(GameSettingObj obj,
        string file, List<string> filter, PackType pack)
    {
        return obj.Export(file, filter, pack);
    }

    public static async Task<List<ResourcepackDisplayObj>> GetResourcepacks(GameSettingObj obj)
    {
        var list = new List<ResourcepackDisplayObj>();
        var list1 = await obj.GetResourcepacks();

        foreach (var item in list1)
        {
            if (item.Broken)
            {
                list.Add(new()
                {
                    Local = item.Local,
                });
            }
            else
            {
                var obj1 = new ResourcepackDisplayObj()
                {
                    Local = item.Local,
                    PackFormat = item.pack.pack_format,
                    Description = item.pack.description,
                    Pack = item
                };

                if (item.Icon != null)
                {
                    using MemoryStream stream = new();
                    await stream.WriteAsync(item.Icon);
                    stream.Seek(0, SeekOrigin.Begin);
                    obj1.Icon = new Avalonia.Media.Imaging.Bitmap(stream);
                }

                list.Add(obj1);
            }
        }

        return list;
    }

    public static void DeleteResourcepack(ResourcepackObj obj)
    {
        File.Delete(obj.Local);
    }

    public static Task<bool> AddResourcepack(GameSettingObj obj, string file)
    {
        return obj.ImportResourcepack(file);
    }

    public static void DeleteScreenshot(string file)
    {
        File.Delete(file);
    }

    public static void ClearScreenshots(GameSettingObj obj)
    {
        obj.ClearScreenshots();
    }

    public static async Task<List<ScreenshotDisplayObj>>
        GetScreenshots(GameSettingObj obj)
    {
        var list = new List<ScreenshotDisplayObj>();
        var list1 = obj.GetScreenshots();

        await Parallel.ForEachAsync(list1, async (item, cancel) =>
        {
            using var image = Image.Load(item);
            image.Mutate(p =>
            {
                p.Resize(200, 120);
            });
            using var stream = new MemoryStream();
            await image.SaveAsPngAsync(stream, cancel);
            stream.Seek(0, SeekOrigin.Begin);
            var obj1 = new ScreenshotDisplayObj()
            {
                Local = item,
                Image = new(stream),
                Name = Path.GetFileName(item)
            };

            list.Add(obj1);
        });

        return list;
    }

    public static async Task DeleteGame(GameSettingObj obj)
    {
        await obj.Remove();
        App.CloseGameEdit(obj);

        App.MainWindow?.IsDelete();
        App.MainWindow?.Load();
    }

    public static GameSettingObj? GetGame(string? name)
    {
        return InstancesPath.GetGame(name);
    }

    public static void OpPath(GameSettingObj obj)
    {
        BaseBinding.OpPath(obj.GetGamePath());
    }

    public static void OpenMcmod(ModDisplayObj obj)
    {
        BaseBinding.OpUrl($"https://search.mcmod.cn/s?key={obj.Name}");
    }

    public static List<ServerInfoObj> GetServers(GameSettingObj obj)
    {
        return obj.GetServerInfo();
    }

    public static List<ShaderpackDisplayObj> GetShaderpacks(GameSettingObj obj)
    {
        var list = new List<ShaderpackDisplayObj>();
        foreach (var item in obj.GetShaderpacks())
        {
            list.Add(new()
            {
                Name = Path.GetFileName(item.Local),
                Local = item.Local,
                Shaderpack = item
            });
        }

        return list;
    }

    public static void AddServer(GameSettingObj obj, string name, string ip)
    {
        obj.AddServer(name, ip);
    }

    public static void SetServer(GameSettingObj obj, List<ServerInfoObj> list)
    {
        obj.SaveServer(list);
    }

    public static Task<List<string>?> GetForgeSupportVersion()
    {
        return ForgeHelper.GetSupportVersion(BaseClient.Source);
    }

    public static Task<List<string>?> GetFabricSupportVersion()
    {
        return FabricHelper.GetSupportVersion(BaseClient.Source);
    }
    public static Task<List<string>?> GetQuiltSupportVersion()
    {
        return QuiltHelper.GetSupportVersion(BaseClient.Source);
    }

    public static void DeleteConfig(GameSettingObj obj)
    {
        obj.JvmArg = null;
        obj.JvmName = "";
        obj.JvmLocal = "";
        obj.StartServer = null;
        obj.ProxyHost = null;
        obj.AdvanceJvm = null;

        obj.Save();
    }

    public static void SetAdvanceJvmArg(GameSettingObj obj, AdvanceJvmObj obj1)
    {
        obj.AdvanceJvm = obj1;

        obj.Save();
    }

    public static bool AddShaderpack(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
    {
        foreach (var item in file)
        {
            bool ok = obj.AddShaderpack(item.GetPath());
            if (ok == false)
                return false;
        }

        return true;
    }

    public static async Task<List<SchematicDisplayObj>> GetSchematics(GameSettingObj obj)
    {
        var list = await obj.GetSchematics();
        var list1 = new List<SchematicDisplayObj>();
        foreach (var item in list)
        {
            if (item.Broken)
            {
                list1.Add(new()
                {
                    Name = "损坏的结构文件",
                    Local = item.Local,
                    Schematic = item
                });
            }
            else
            {
                list1.Add(new()
                {
                    Name = item.Name,
                    Local = item.Local,
                    Width = item.Width,
                    Height = item.Height,
                    Length = item.Length,
                    Author = item.Author,
                    Description = item.Description,
                    Schematic = item
                });
            }
        }

        return list1;
    }

    public static bool AddSchematic(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
    {
        foreach (var item in file)
        {
            bool ok = obj.AddSchematic(item.GetPath());
            if (ok == false)
                return false;
        }

        return true;
    }

    public static List<string> GetPackType()
    {
        return new()
        {
            PackType.ColorMC.GetName(),
            PackType.CurseForge.GetName(),
            PackType.Modrinth.GetName(),
            PackType.MMC.GetName(),
            PackType.HMCL.GetName(),
        };
    }

    public static List<string> GetAddType()
    {
        return new()
        {
            FileType.Mod.GetName(),
            FileType.World.GetName(),
            FileType.Shaderpack.GetName(),
            FileType.Resourcepack.GetName(),
            FileType.DataPacks.GetName(),
        };
    }

    public static List<SourceType> GetSourceList(FileType type)
    {
        switch (type)
        {
            case FileType.Mod:
            case FileType.DataPacks:
            case FileType.Resourcepack:
                return new()
                {
                    SourceType.CurseForge,
                    SourceType.Modrinth,
                };
            case FileType.Shaderpack:
                return new()
                {
                    SourceType.Modrinth,
                };
            case FileType.World:
                return new()
                {
                    SourceType.CurseForge,
                };
            default:
                return new();
        }
    }

    public static async Task<List<FileItemDisplayObj>?> GetList(FileType now, SourceType type, string? version, string? filter, int page, int sortOrder, string categoryId)
    {
        version ??= "";
        filter ??= "";
        if (type == SourceType.CurseForge)
        {
            var list = now switch
            {
                FileType.Mod => await CurseForge.GetModList(version, page, filter: filter, sortOrder: sortOrder, categoryId: categoryId),
                FileType.World => await CurseForge.GetWorldList(version, page, filter: filter, sortOrder: sortOrder, categoryId: categoryId),
                FileType.Resourcepack => await CurseForge.GetResourcepackList(version, page, filter: filter, sortOrder: sortOrder, categoryId: categoryId),
                FileType.DataPacks => await CurseForge.GetDataPacksList(version, page, filter: filter, sortOrder: sortOrder),
                _ => null
            };
            if (list == null)
                return null;
            var list1 = new List<FileItemDisplayObj>();
            list.data.ForEach(item =>
            {
                list1.Add(new()
                {
                    ID = item.id.ToString(),
                    Name = item.name,
                    Summary = item.summary,
                    Author = item.authors.GetString(),
                    DownloadCount = item.downloadCount,
                    ModifiedDate = item.dateModified,
                    Logo = item.logo?.url,
                    FileType = now,
                    SourceType = SourceType.CurseForge,
                    Data = item
                });
            });

            return list1;
        }
        else if (type == SourceType.Modrinth)
        {
            var list = now switch
            {
                FileType.Mod => await Modrinth.GetModList(version, page, filter: filter, sortOrder: sortOrder, categoryId: categoryId),
                FileType.Resourcepack => await Modrinth.GetResourcepackList(version, page, filter: filter, sortOrder: sortOrder, categoryId: categoryId),
                FileType.DataPacks => await Modrinth.GetDataPackList(version, page, filter: filter, sortOrder: sortOrder, categoryId: categoryId),
                FileType.Shaderpack => await Modrinth.GetShaderpackList(version, page, filter: filter, sortOrder: sortOrder, categoryId: categoryId),
                _ => null
            };
            if (list == null)
                return null;
            var list1 = new List<FileItemDisplayObj>();
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
                    Data = item
                });
            });

            return list1;
        }

        return null;
    }

    private static string GetString(this List<CurseForgeObj.Data.Authors> authors)
    {
        var builder = new StringBuilder();
        foreach (var item in authors)
        {
            builder.Append(item.name).Append(',');
        }

        return builder.ToString()[..^1];
    }

    public static async Task<bool> Download(FileType type, GameSettingObj obj, CurseForgeObj.Data.LatestFiles? data)
    {
        if (data == null)
            return false;

        switch (type)
        {
            case FileType.Mod:
                foreach (var item1 in obj.Mods)
                {
                    if (item1.Key == data.modId.ToString())
                    {
                        File.Delete(Path.GetFullPath(obj.GetModsPath() + '/' + item1.Value.File));
                    }
                }

                var item = new DownloadItem()
                {
                    Name = data.displayName,
                    Url = data.downloadUrl,
                    Local = Path.GetFullPath(obj.GetModsPath() + "/" + data.fileName),
                    SHA1 = data.hashes.Where(a => a.algo == 1)
                                .Select(a => a.value).FirstOrDefault(),
                    Overwrite = true
                };

                var res = await DownloadManager.Download(item);
                if (res)
                {
                    var obj1 = new ModPackInfoObj()
                    {
                        FileId = data.id.ToString(),
                        ModId = data.modId.ToString(),
                        File = data.fileName,
                        Name = data.displayName,
                        Url = data.downloadUrl,
                        SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault()
                    };
                    if (obj.Mods.ContainsKey(obj1.ModId))
                    {
                        obj.Mods[obj1.ModId] = obj1;
                    }
                    else
                    {
                        obj.Mods.Add(obj1.ModId, obj1);
                    }
                    obj.SaveModInfo();
                }
                return res;
            case FileType.World:
                item = new DownloadItem()
                {
                    Name = data.displayName,
                    Url = data.downloadUrl,
                    Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + data.fileName),
                    SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault(),
                    Overwrite = true
                };

                res = await DownloadManager.Download(item);
                if (!res)
                {
                    return false;
                }

                return await AddWorld(obj, item.Local);
            case FileType.Resourcepack:
                item = new DownloadItem()
                {
                    Name = data.displayName,
                    Url = data.downloadUrl,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + data.fileName),
                    SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault(),
                    Overwrite = true
                };

                return await DownloadManager.Download(item);
            default:
                return false;
        }
    }

    public static async Task<bool> Download(FileType type, GameSettingObj obj, ModrinthVersionObj? data)
    {
        if (data == null)
            return false;

        var file = data.files.FirstOrDefault(a => a.primary);
        if (file == null)
        {
            file = data.files[0];
        }

        switch (type)
        {
            case FileType.Mod:
                foreach (var item1 in obj.Mods)
                {
                    if (item1.Key == data.project_id)
                    {
                        File.Delete(Path.GetFullPath(obj.GetModsPath() + '/' + item1.Value.File));
                    }
                }

                var item = new DownloadItem()
                {
                    Name = data.name,
                    Url = file.url,
                    Local = Path.GetFullPath(obj.GetModsPath() + "/" + file.filename),
                    SHA1 = file.hashes.sha1,
                    Overwrite = true
                };

                var res = await DownloadManager.Download(item);
                if (res)
                {
                    var obj1 = new ModPackInfoObj()
                    {
                        FileId = data.id.ToString(),
                        ModId = data.project_id,
                        File = file.filename,
                        Name = data.name,
                        Url = file.url,
                        SHA1 = file.hashes.sha1
                    };
                    if (obj.Mods.ContainsKey(obj1.ModId))
                    {
                        obj.Mods[obj1.ModId] = obj1;
                    }
                    else
                    {
                        obj.Mods.Add(obj1.ModId, obj1);
                    }
                    obj.SaveModInfo();
                }
                return res;
            case FileType.World:
                item = new DownloadItem()
                {
                    Name = data.name,
                    Url = file.url,
                    Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + file.filename),
                    SHA1 = file.hashes.sha1,
                    Overwrite = true
                };

                res = await DownloadManager.Download(item);
                if (!res)
                {
                    return false;
                }

                return await AddWorld(obj, item.Local);
            case FileType.Resourcepack:
                item = new DownloadItem()
                {
                    Name = data.name,
                    Url = file.url,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + file.filename),
                    SHA1 = file.hashes.sha1,
                    Overwrite = true
                };

                return await DownloadManager.Download(item);
            default:
                return false;
        }
    }
}