using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils.LaunchSetting;
using DynamicData;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public static Task<(bool, GameSettingObj?)> AddPack(string dir, PackType type)
    {
        return InstancesPath.InstallFromZip(dir, type);
    }

    public static Dictionary<string, List<GameSettingObj>> GetGameGroups()
    {
        return InstancesPath.Groups;
    }

    public static Task<CurseForgeObj?> GetPackList(string? version, int sort, string? filter, int page, int sortOrder)
    {
        version ??= "";
        filter ??= "";
        return CurseForge.GetModPackList(version, page, sort, filter, sortOrder: sortOrder);
    }

    public static Task<CurseForgeObj?> GetModList(string? version, int sort, string? filter, int page, int sortOrder)
    {
        version ??= "";
        filter ??= "";
        return CurseForge.GetModList(version, page, sort, filter, sortOrder: sortOrder);
    }

    public static Task<CurseForgeObj?> GetWorldList(string? version, int sort, string? filter, int page, int sortOrder)
    {
        version ??= "";
        filter ??= "";
        return CurseForge.GetWorldList(version, page, sort, filter, sortOrder: sortOrder);
    }

    public static Task<CurseForgeObj?> GetResourcepackList(string? version, int sort, string? filter, int page, int sortOrder)
    {
        version ??= "";
        filter ??= "";
        return CurseForge.GetResourcepackList(version, page, sort, filter, sortOrder: sortOrder);
    }

    public static List<string> GetCurseForgeTypes()
    {
        var list = new List<string>()
        {
            SortField.Featured.GetName(),
            SortField.Popularity.GetName(),
            SortField.LastUpdated.GetName(),
            SortField.Name.GetName(),
            SortField.Author.GetName(),
            SortField.TotalDownloads.GetName(),
            SortField.Category.GetName(),
            SortField.GameVersion.GetName()
        };

        return list;
    }

    public static List<string> GetSortOrder()
    {
        return new()
        {
            Localizer.Instance["GameBinding.SortOrder.Item1"],
            Localizer.Instance["GameBinding.SortOrder.Item2"]
        };
    }

    public static async Task<List<string>?> GetCurseForgeGameVersions()
    {
        var list = await CurseForge.GetCurseForgeVersionType();
        if (list == null)
        {
            return null;
        }

        list.data.RemoveAll(a =>
        {
            return a.id is 68441 or 615 or 1 or 3 or 2 or 73247 or 75208;
        });

        var list1 = from item in list.data
                    where item.id > 17
                    orderby item.id descending
                    select item;

        var list11 = from item in list.data
                     where item.id < 18
                     orderby item.id ascending
                     select item;

        var list111 = new List<CurseForgeVersionType.Item>();
        list111.AddRange(list1);
        list111.AddRange(list11);

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

        return list3;
    }

    public static async Task<bool> InstallCurseForge(CurseForgeObj.Data.LatestFiles data,
        CurseForgeObj.Data data1)
    {
        var res = await InstancesPath.InstallFromCurseForge(data);
        if (!res.Item1)
        {
            return false;
        }
        if (data1.logo != null)
        {
            await SetGameIconFromUrl(res.Item2!, data1.logo.url);
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
            Logs.Error(Localizer.Instance["GameBinding.Error5"], e);
            App.ShowError(Localizer.Instance["GameBinding.Error5"], e);
            return false;
        }
    }

    public static async Task SetGameIconFromFile(Window win, GameSettingObj obj)
    {
        try
        {
            var file = await win.StorageProvider.OpenFilePickerAsync(new()
            {
                Title = Localizer.Instance["GameBinding.Info2"],
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>()
            {
                new(Localizer.Instance["GameBinding.Info3"])
                {
                     Patterns = new List<string>()
                     {
                        "*.png",
                        "*.jpg",
                        "*.bmp"
                     }
                }
            }
            });
            if (file?.Any() == true)
            {
                var item = file[0];
                var name = item.GetPath();
                var info = await Image.IdentifyAsync(name);
                if (info.Width != info.Height || info.Width > 200 || info.Height > 200)
                {
                    (win as IBaseWindow)?.Info.Show(Localizer.Instance["GameBinding.Erro65"]);
                    return;
                }
                var data = await File.ReadAllBytesAsync(name);
                await File.WriteAllBytesAsync(obj.GetIconFile(), data);
            }
        }
        catch (Exception e)
        {
            Logs.Error(Localizer.Instance["GameBinding.Error5"], e);
            App.ShowError(Localizer.Instance["GameBinding.Error5"], e);
        }
    }

    public static Task<CurseForgeFileObj?> GetPackFile(long id, int page)
    {
        return CurseForge.GetCurseForgeFiles(id, page);
    }

    public static async Task<(bool, string?)> Launch(GameSettingObj? obj, bool debug)
    {
        if (obj == null)
        {
            return (false, Localizer.Instance["GameBinding.Error1"]);
        }

        if (BaseBinding.Games.ContainsValue(obj))
        {
            return (false, Localizer.Instance["GameBinding.Error4"]);
        }

        var login = UserBinding.GetLastUser();
        if (login == null)
        {
            return (false, Localizer.Instance["GameBinding.Error2"]);
        }

        if (UserBinding.IsLock(login))
        {
            var res = await App.MainWindow!.Info.ShowWait(Localizer.Instance["GameBinding.Info1"]);
            if (!res)
                return (false, Localizer.Instance["GameBinding.Error3"]);
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
        foreach (var item in mod.Game.CurseForgeMods)
        {
            if (item.Value.File == name)
            {
                mod.Game.CurseForgeMods.Remove(item.Key);
                mod.Game.SaveCurseForgeMod();
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

    public static Task<bool> DownloadMod(GameSettingObj obj, CurseForgeObj.Data.LatestFiles data)
    {
        foreach (var item in obj.CurseForgeMods)
        {
            if (item.Key == data.modId)
            {
                File.Delete(Path.GetFullPath(obj.GetModsPath() + '/' + item.Value.File));
            }
        }

        var item1 = new DownloadItem()
        {
            Name = data.displayName,
            Url = data.downloadUrl,
            Local = Path.GetFullPath(obj.GetModsPath() + "/" + data.fileName),
            SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault(),
            Overwrite = true
        };

        return DownloadManager.Download(item1);
    }

    public static void AddModInfo(GameSettingObj obj, CurseForgeObj.Data.LatestFiles data)
    {
        var obj1 = new CurseForgeModObj1()
        {
            Id = data.id,
            ModId = data.modId,
            File = data.fileName,
            Name = data.displayName,
            Url = data.downloadUrl,
            SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault()
        };
        if (obj.CurseForgeMods.ContainsKey(obj1.ModId))
        {
            obj.CurseForgeMods[obj1.ModId] = obj1;
        }
        else
        {
            obj.CurseForgeMods.Add(obj1.ModId, obj1);
        }
        obj.SaveCurseForgeMod();
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

    public static Task<bool> AddWorld(GameSettingObj obj, string file)
    {
        return obj.ImportWorldZip(file);
    }

    public static void DeleteWorld(WorldObj world)
    {
        world.Remove();
    }

    public static Task ExportWorld(WorldObj world, string file)
    {
        return world.ExportWorldZip(file);
    }

    public static async Task<bool> DownloadWorld(GameSettingObj obj, CurseForgeObj.Data.LatestFiles data)
    {
        var item = new DownloadItem()
        {
            Name = data.displayName,
            Url = data.downloadUrl,
            Local = Path.GetFullPath(DownloadManager.DownloadDir + "/" + data.fileName),
            SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault(),
            Overwrite = true
        };

        var res = await DownloadManager.Download(item);
        if (!res)
        {
            return false;
        }

        return await AddWorld(obj, item.Local);
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

    public static async Task<bool> DownloadResourcepack(GameSettingObj obj, CurseForgeObj.Data.LatestFiles data)
    {
        var item = new DownloadItem()
        {
            Name = data.displayName,
            Url = data.downloadUrl,
            Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + data.fileName),
            SHA1 = data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault(),
            Overwrite = true
        };

        var res = await DownloadManager.Download(item);

        return res;
    }

    public static async Task<List<ScreenshotDisplayObj>> GetScreenshots(GameSettingObj obj)
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
}
