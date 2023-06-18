using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Windows;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;

namespace ColorMC.Gui.UIBinding;

public static class GameBinding
{
    public static bool IsNotGame
        => InstancesPath.IsNotGame;
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

        game = await InstancesPath.CreateGame(game);

        return game != null;
    }

    public static async Task<bool> AddGame(string name, string local, List<string> unselect, string? group = null)
    {
        var game = new GameSettingObj()
        {
            Name = name,
            Version = GetGameVersion(true, false, false)[0],
            Loader = Loaders.Normal,
            LoaderVersion = null,
            GroupName = group
        };

        game = await InstancesPath.CreateGame(game);
        if (game == null)
        {
            return false;
        }

        var res = await Task.Run(() =>
        {
            try
            {
                local = Path.GetFullPath(local);
                var list = PathC.GetAllFile(local);
                list.RemoveAll(item => unselect.Contains(item.FullName));
                int basel = local.Length;
                var local1 = game.GetGamePath();
                foreach (var item in list)
                {
                    var path = item.FullName[basel..];
                    var info = new FileInfo(Path.GetFullPath(local1 + "/" + path));
                    info.Directory?.Create();
                    File.Copy(item.FullName, info.FullName);
                }
                return true;
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("Gui.Error26"), e);
                App.ShowError(App.GetLanguage("Gui.Error26"), e);
                return false;
            }
        });

        if (!res)
        {
            await game.Remove();
        }

        App.ShowGameEdit(game);

        return true;
    }

    public static Task<(bool, GameSettingObj?)> AddPack(string dir, PackType type, string? name, string? group)
    {
        return InstancesPath.InstallFromZip(dir, type, name, group);
    }

    public static Dictionary<string, List<GameSettingObj>> GetGameGroups()
    {
        return InstancesPath.Groups;
    }

    public static List<string> GetCurseForgeSortTypes()
    {
        return new List<string>()
        {
            CurseForgeSortField.Featured.GetName(),
            CurseForgeSortField.Popularity.GetName(),
            CurseForgeSortField.LastUpdated.GetName(),
            CurseForgeSortField.Name.GetName(),
            CurseForgeSortField.TotalDownloads.GetName()
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
            SourceType.Modrinth.GetName()
        };
    }

    public static Task<List<string>?> GetCurseForgeGameVersions()
    {
        return CurseForgeAPI.GetGameVersions();
    }

    public static Task<List<string>?> GetModrinthGameVersions()
    {
        return ModrinthAPI.GetGameVersion();
    }

    private static CurseForgeCategoriesObj? CurseForgeCategories;

    public static async Task<Dictionary<string, string>?> GetCurseForgeCategories(
        FileType type = FileType.ModPack)
    {
        if (CurseForgeCategories == null)
        {
            var list6 = await CurseForgeAPI.GetCategories();
            if (list6 == null)
            {
                return null;
            }

            CurseForgeCategories = list6;
        }

        var list7 = from item2 in CurseForgeCategories.data
                    where item2.classId == type switch
                    {
                        FileType.Mod => CurseForgeAPI.ClassMod,
                        FileType.World => CurseForgeAPI.ClassWorld,
                        FileType.Resourcepack => CurseForgeAPI.ClassResourcepack,
                        _ => CurseForgeAPI.ClassModPack
                    }
                    orderby item2.name descending
                    select (item2.name, item2.id);

        return list7.ToDictionary(a => a.id.ToString(), a => a.name);
    }

    private static List<ModrinthCategoriesObj>? ModrinthCategories;

    public static async Task<Dictionary<string, string>?> GetModrinthCategories(
        FileType type = FileType.ModPack)
    {
        if (ModrinthCategories == null)
        {
            var list6 = await ModrinthAPI.GetCategories();
            if (list6 == null)
            {
                return null;
            }

            ModrinthCategories = list6;
        }

        var list7 = from item2 in ModrinthCategories
                    where item2.project_type == type switch
                    {
                        FileType.Shaderpack => ModrinthAPI.ClassShaderpack,
                        FileType.Resourcepack => ModrinthAPI.ClassResourcepack,
                        FileType.ModPack => ModrinthAPI.ClassModPack,
                        _ => ModrinthAPI.ClassMod
                    }
                    && item2.header == "categories"
                    orderby item2.name descending
                    select item2.name;

        return list7.ToDictionary(a => a);
    }

    public static async Task<bool> InstallCurseForge(CurseForgeObjList.Data.LatestFiles data,
        CurseForgeObjList.Data data1, string? name, string? group)
    {
        var res = await InstancesPath.InstallFromCurseForge(data, name, group);
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
            await SetGameIconFromUrl(res.Item2!, data1.icon_url);
        }

        return true;
    }

    public static async Task SetGameIconFromUrl(GameSettingObj obj, string url)
    {
        try
        {
            var data = await BaseClient.GetBytes(url);
            await File.WriteAllBytesAsync(obj.GetIconFile(), data);
        }
        catch (Exception e)
        {
            Logs.Error(App.GetLanguage("GameBinding.Error8"), e);
            App.ShowError(App.GetLanguage("GameBinding.Error8"), e);
        }
    }

    public static async Task SetGameIconFromFile(IBaseWindow win, GameSettingObj obj)
    {
        if (win is TopLevel top)
        {
            await SetGameIconFromFile(top, obj);
        }
    }

    public static async Task SetGameIconFromFile(TopLevel win, GameSettingObj obj)
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
                if (name == null)
                    return;
                var info = await Image.IdentifyAsync(name);
                if (info.Width != info.Height || info.Width > 200 || info.Height > 200)
                {
                    (win as IBaseWindow)?.OkInfo.Show(App.GetLanguage("GameBinding.Error6"));
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

    public static async Task<(bool, string?)> Launch(GameSettingObj? obj)
    {
        if (obj == null)
        {
            return (false, App.GetLanguage("GameBinding.Error1"));
        }

        if (BaseBinding.IsGameRun(obj))
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
                BaseBinding.OpUrl("https://www.minecraft.net/");
                return (false, App.GetLanguage("GameBinding.Error7"));
            }
        }

        if (UserBinding.IsLock(login))
        {
            var res = await App.MainWindow!.Window.OkInfo.ShowWait(App.GetLanguage("GameBinding.Info1"));
            if (!res)
                return (false, App.GetLanguage("GameBinding.Error3"));
        }

        var res1 = await BaseBinding.Launch(obj, login);
        if (res1.Item1)
        {
            ConfigBinding.SetLastLaunch(obj.UUID);
        }
        return res1;
    }

    public static bool AddGameGroup(string name)
    {
        return InstancesPath.AddGroup(name);
    }

    public static void MoveGameGroup(GameSettingObj obj, string? now)
    {
        obj.MoveGameGroup(now);
        App.MainWindow?.LoadMain();
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

    public static void SetGameJvmMemArg(GameSettingObj obj, uint? min, uint? max)
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

    public static async Task<List<ModDisplayModel>> GetGameMods(GameSettingObj obj)
    {
        var list = new List<ModDisplayModel>();
        var list1 = await obj.GetMods();
        if (list1 == null)
            return list;

        list1.ForEach(item =>
        {
            ModDisplayModel obj1;
            if (item.ReadFail)
            {
                obj1 = new ModDisplayModel()
                {
                    Name = App.GetLanguage("GameEditWindow.Tab4.Info5"),
                    Obj = item
                };
            }
            else
            {
                obj1 = new ModDisplayModel()
                {
                    Name = item.name,
                    Obj = item
                };
            }

            var item1 = obj.Mods.Values.FirstOrDefault(a => a.SHA1 == item.Sha1);
            if (item1 != null)
            {
                obj1.Obj1 = item1;
            }

            obj1.Enable = item.Disable;

            list.Add(obj1);
        });
        return list;
    }

    public static bool ModEnDi(ModObj obj)
    {
        try
        {
            if (obj.Disable)
            {
                obj.Enable();
            }
            else
            {
                obj.Disable();
            }

            return true;
        }
        catch (Exception e)
        {
            Logs.Error("Mod error", e);
            return false;
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

    public static bool AddMods(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
    {
        var list = new List<string>();
        foreach (var item in file)
        {
            var item1 = item.GetPath();
            if (item1 != null)
                list.Add(item1);
        }
        return obj.AddMods(list);
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

    public static List<string> GetAllConfig(WorldObj obj)
    {
        var list = new List<string>();
        var dir = obj.Local.Length + 1;

        var list1 = PathC.GetAllFile(obj.Local);
        foreach (var item in list1)
        {
            if (item.Extension is ".mca" or ".png" or ".lock")
                continue;
            list.Add(item.FullName[dir..]);
        }

        return list;
    }

    public static List<string> GetAllTopConfig(GameSettingObj obj)
    {
        var list = new List<string>();
        var dir = obj.GetGamePath();

        var dir1 = new DirectoryInfo(dir);
        var list1 = dir1.GetFileSystemInfos();
        foreach (var item in list1)
        {
            string name = item.Name;
            if (item.Attributes == FileAttributes.Directory)
            {
                if (name == "mods" || name == "resourcepacks")
                    continue;
                name += "/";
            }

            list.Add(name);
        }

        return list;
    }

    public static NbtBase? ReadNbt(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return NbtBase.Read(Path.GetFullPath(dir + "/" + name));
    }

    public static NbtBase? ReadNbt(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return NbtBase.Read(Path.GetFullPath(dir + "/" + name));
    }

    public static string ReadConfigFile(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return File.ReadAllText(Path.GetFullPath(dir + "/" + name));
    }

    public static string ReadConfigFile(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return File.ReadAllText(Path.GetFullPath(dir + "/" + name));
    }

    public static void SaveConfigFile(WorldObj obj, string name, string? text)
    {
        var dir = obj.Local;

        File.WriteAllText(Path.GetFullPath(dir + "/" + name), text);
    }

    public static void SaveConfigFile(GameSettingObj obj, string name, string? text)
    {
        var dir = obj.GetGamePath();

        File.WriteAllText(Path.GetFullPath(dir + "/" + name), text);
    }

    public static void SaveNbtFile(WorldObj obj, string file, NbtBase nbt)
    {
        var dir = obj.Local;

        NbtBase.Write(Path.GetFullPath(dir + "/" + file), nbt);
    }

    public static void SaveNbtFile(GameSettingObj obj, string file, NbtBase nbt)
    {
        var dir = obj.GetGamePath();

        NbtBase.Write(Path.GetFullPath(dir + "/" + file), nbt);
    }

    public static Task<List<string>?> GetForgeVersion(string version)
    {
        return ForgeAPI.GetVersionList(version, BaseClient.Source);
    }

    public static Task<List<string>?> GetFabricVersion(string version)
    {
        return FabricAPI.GetLoaders(version, BaseClient.Source);
    }

    public static Task<List<string>?> GetQuiltVersion(string version)
    {
        return QuiltAPI.GetLoaders(version, BaseClient.Source);
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
                Time = Funtcions.MillisecondsToDataTime(item.LastPlayed).ToString(),
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

    public static async Task<bool> AddWorld(GameSettingObj obj, string? file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return false;
        }
        var res = await obj.AddWorldZip(file);
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

    public static Task ExportWorld(WorldObj world, string? file)
    {
        if (file == null)
            return Task.CompletedTask;

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
                    Pack = item
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

    public static Task<bool> AddResourcepack(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
    {
        var list = new List<string>();
        foreach (var item in file)
        {
            var item1 = item.GetPath();
            if (item1 != null)
                list.Add(item1);
        }
        return obj.AddResourcepack(list);
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

    public static async Task<bool> DeleteGame(GameSettingObj obj)
    {
        App.CloseGameWindow(obj);
        var res = await obj.Remove();

        App.MainWindow?.LoadMain();

        return res;
    }

    public static GameSettingObj? GetGame(string? uuid)
    {
        return InstancesPath.GetGame(uuid);
    }

    public static void OpPath(GameSettingObj obj)
    {
        BaseBinding.OpPath(obj.GetGamePath());
    }

    public static IEnumerable<ServerInfoObj> GetServers(GameSettingObj obj)
    {
        return obj.GetServerInfos();
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

    public static void DeleteServer(GameSettingObj obj, ServerInfoObj server)
    {
        var list = obj.GetServerInfos().ToList();
        var item = list.First(a => a.Name == server.Name && a.IP == server.IP);
        list.Remove(item);
        obj.SaveServer(list);
    }

    public static Task<List<string>?> GetForgeSupportVersion()
    {
        return ForgeAPI.GetSupportVersion(BaseClient.Source);
    }

    public static Task<List<string>?> GetFabricSupportVersion()
    {
        return FabricAPI.GetSupportVersion(BaseClient.Source);
    }
    public static Task<List<string>?> GetQuiltSupportVersion()
    {
        return QuiltAPI.GetSupportVersion(BaseClient.Source);
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

    public static Task<bool> AddShaderpack(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
    {
        var list = new List<string>();
        foreach (var item in file)
        {
            var item1 = item.GetPath();
            if (item1 != null)
                list.Add(item1);
        }

        return obj.AddShaderpack(list);
    }

    public static List<SchematicDisplayObj> GetSchematics(GameSettingObj obj)
    {
        var list = obj.GetSchematics();
        var list1 = new List<SchematicDisplayObj>();
        foreach (var item in list)
        {
            if (item.Broken)
            {
                list1.Add(new()
                {
                    Name = App.GetLanguage("Gui.Error14"),
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
        var list = new List<string>();
        foreach (var item in file)
        {
            var item1 = item.GetPath();
            if (item1 != null)
                list.Add(item1);
        }

        return obj.AddSchematic(list);
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
            FileType.Optifne.GetName()
        };
    }

    public static void SetModInfo(GameSettingObj obj, CurseForgeObjList.Data.LatestFiles? data)
    {
        if (data == null)
            return;

        data.FixDownloadUrl();

        var obj1 = new ModInfoObj()
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

    public static void SetModInfo(GameSettingObj obj, ModrinthVersionObj? data)
    {
        if (data == null)
            return;

        var file = data.files.FirstOrDefault(a => a.primary);
        if (file == null)
        {
            file = data.files[0];
        }

        var obj1 = new ModInfoObj()
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

    public static async Task<bool> BackupWorld(WorldObj world)
    {
        try
        {
            await world.Backup();
            return true;
        }
        catch (Exception e)
        {
            string text = App.GetLanguage("Gui.Error20");
            Logs.Error(text, e);
            App.ShowError(text, e);
            return false;
        }
    }

    public static Task<bool> BackupWorld(GameSettingObj obj, FileInfo item1)
    {
        return obj.UnzipBackupWorld(item1);
    }


    public static void SetGameName(GameSettingObj obj, string data)
    {
        obj.Name = data;
        obj.Save();

        App.MainWindow?.LoadMain();
    }

    public static async Task<bool> CopyGame(GameSettingObj obj, string data)
    {
        if (BaseBinding.IsGameRun(obj))
            return false;

        var res = await obj.Copy(data);
        if (res == null)
            return false;

        App.MainWindow?.LoadMain();

        return true;
    }

    public static void SaveServerPack(ServerPackObj obj1)
    {
        obj1.Save();
    }

    public static ServerPackObj? GetServerPack(GameSettingObj obj)
    {
        return obj.GetServerPack();
    }

    public static Task<bool> GenServerPack(ServerPackObj obj, string local)
    {
        return obj.GenServerPack(local);
    }

    public static async Task<bool?> AddFile(TopLevel? window, GameSettingObj obj, FileType type)
    {
        if (window == null)
            return false;
        switch (type)
        {
            case FileType.Schematic:
                var res = await BaseBinding.OpFile(window,
                      App.GetLanguage("GameEditWindow.Tab12.Info1"),
                      new string[] { "*" + Schematic.Name1, "*" + Schematic.Name2 },
                      App.GetLanguage("GameEditWindow.Tab12.Info2"), true);
                if (res?.Any() == true)
                {
                    return AddSchematic(obj, res);
                }
                return null;
            case FileType.Shaderpack:
                res = await BaseBinding.OpFile(window,
                    App.GetLanguage("GameEditWindow.Tab11.Info1"),
                    new string[] { "*.zip" },
                    App.GetLanguage("GameEditWindow.Tab11.Info2"), true);
                if (res?.Any() == true)
                {
                    return await AddShaderpack(obj, res);
                }
                return null;
            case FileType.Mod:
                res = await BaseBinding.OpFile(window,
                    App.GetLanguage("GameEditWindow.Tab4.Info7"),
                    new string[] { "*.jar" },
                    App.GetLanguage("GameEditWindow.Tab4.Info8"), true);
                if (res?.Any() == true)
                {
                    return AddMods(obj, res);
                }
                return null;
            case FileType.World:
                res = await BaseBinding.OpFile(window!,
                    App.GetLanguage("GameEditWindow.Tab5.Info2"),
                    new string[] { "*.zip" },
                    App.GetLanguage("GameEditWindow.Tab5.Info6"));
                if (res?.Any() == true)
                {
                    return await AddWorld(obj, res[0].GetPath());
                }
                return null;
            case FileType.Resourcepack:
                res = await BaseBinding.OpFile(window,
                    App.GetLanguage("GameEditWindow.Tab8.Info2"),
                    new string[] { "*.zip" },
                    App.GetLanguage("GameEditWindow.Tab8.Info5"), true);
                if (res?.Any() == true)
                {
                    return await AddResourcepack(obj, res);
                }
                return null;
        }

        return null;
    }

    public static async Task<bool> AddFile(GameSettingObj obj, IDataObject data, FileType type)
    {
        if (!data.Contains(DataFormats.Files))
        {
            return false;
        }
        var list = data.GetFiles();
        if (list == null)
            return false;
        switch (type)
        {
            case FileType.Mod:
                var list1 = new List<string>();
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                        continue;
                    if (File.Exists(file) && file.ToLower().EndsWith(".jar"))
                    {
                        list1.Add(file);
                    }
                }

                return obj.AddMods(list1);
            case FileType.World:
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                        continue;
                    if (File.Exists(file) && file.ToLower().EndsWith(".zip"))
                    {
                        return await obj.AddWorldZip(file);
                    }
                }
                return false;
            case FileType.Resourcepack:
                list1 = new List<string>();
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                        continue;
                    if (File.Exists(file) && file.ToLower().EndsWith(".zip"))
                    {
                        list1.Add(file);
                    }
                }
                return await obj.AddResourcepack(list1);
            case FileType.Shaderpack:
                list1 = new List<string>();
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                        continue;
                    if (File.Exists(file) && file.ToLower().EndsWith(".zip"))
                    {
                        list1.Add(file);
                    }
                }
                return await obj.AddShaderpack(list1);
            case FileType.Schematic:
                list1 = new List<string>();
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                        continue;
                    var file1 = file.ToLower();
                    if (File.Exists(file) &&
                        (file1.EndsWith(Schematic.Name1) || file1.EndsWith(Schematic.Name2)))
                    {
                        list1.Add(file);
                    }
                }
                return obj.AddSchematic(list1);
        }

        return false;
    }

    public static async void CopyServer(TopLevel? top, ServerInfoObj obj)
    {
        await BaseBinding.CopyTextClipboard(top, $"{obj.Name}\n{obj.IP}");
    }

    public static Task<bool> ModCheck(List<ModDisplayModel> list)
    {
        return Task.Run(() =>
        {
            List<string> modid = new();
            List<ModDisplayModel> mod = new();
            foreach (var item in list)
            {
                if (item.Obj.modid == null || item.Obj.Disable)
                    continue;
                modid.Add(item.Obj.modid);
                mod.Add(item);
            }
            modid.Add("forge");

            ConcurrentBag<(string, List<string>)> lost = new();

            Parallel.ForEach(mod, new ParallelOptions()
            { 
                 MaxDegreeOfParallelism = 1
            }, item =>
            {
                if (item == null)
                    return;

                var list1 = new List<string>();
                if (item.Obj.requiredMods != null)
                {
                    foreach (var item1 in item.Obj.requiredMods)
                    {
                        var list2 = item1.Split(",");
                        list1.AddRange(list2);
                        foreach (var item2 in list2)
                        {
                            if (modid.Contains(item2))
                            {
                                list1.Remove(item2);
                            }
                        }
                    }
                }
                if (item.Obj.dependencies != null)
                {
                    foreach (var item1 in item.Obj.dependencies)
                    {
                        var list2 = item1.Split(",");
                        list1.AddRange(list2);
                        foreach (var item2 in list2)
                        {
                            if (modid.Contains(item2))
                            {
                                list1.Remove(item2);
                            }
                        }
                    }
                }

                if (list1.Count > 0)
                {
                    lost.Add((item.Name, list1));
                }
            });

            if (lost.Count > 0)
            {
                var str = new StringBuilder();
                foreach (var item in lost)
                {
                    str.Append(string.Format(App.GetLanguage("Gui.Info25"), item.Item1, item.Item2.GetString())).Append(Environment.NewLine);
                }

                App.ShowError(App.GetLanguage("Gui.Info26"), str.ToString());
                return false;
            }

            return true;
        });
    }

    public static List<string> GetLogList(GameSettingObj obj)
    {
        return obj.GetLogFiles();
    }

    public static async Task<string?> ReadLog(GameSettingObj obj, string name)
    {
        if (BaseBinding.IsGameRun(obj))
        {
            if (name.EndsWith("latest.log") || name.EndsWith("debug.log"))
                return null;
        }
        return await Task.Run(() => obj.ReadLog(name));
    }

    public static Task<bool> ModPackUpdate(GameSettingObj obj, CurseForgeObjList.Data.LatestFiles fid)
    {
        return obj.UpdateModPack(fid);
    }

    public static Task<bool> ModPackUpdate(GameSettingObj obj, ModrinthVersionObj fid)
    {
        return obj.UpdateModPack(fid);
    }
}