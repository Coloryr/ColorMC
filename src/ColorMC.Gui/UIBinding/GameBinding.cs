using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ColorMC.Core.Chunk;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
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
using ICSharpCode.SharpZipLib.Zip;
using Image = SixLabors.ImageSharp.Image;

namespace ColorMC.Gui.UIBinding;

public static class GameBinding
{
    public static bool IsNotGame => InstancesPath.IsNotGame;
    public static List<GameSettingObj> GetGames()
    {
        return InstancesPath.Games;
    }

    public static List<string> GetGameVersion(bool? type1, bool? type2, bool? type3)
    {
        var list = new List<string>();
        if (VersionPath.Versions == null)
        {
            return list;
        }

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

    public static async Task<bool> InstallCurseForge(CurseForgeModObj.Data data,
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
        try
        {
            var file = await PathBinding.SelectFile(win, FileType.Icon);
            if (file != null)
            {
                var info = await Image.IdentifyAsync(file);
                if (info.Width != info.Height || info.Width > 200 || info.Height > 200)
                {
                    win.OkInfo.Show(App.GetLanguage("GameBinding.Error6"));
                    return;
                }
                var data = await File.ReadAllBytesAsync(file);
                await File.WriteAllBytesAsync(obj.GetIconFile(), data);
            }
        }
        catch (Exception e)
        {
            Logs.Error(App.GetLanguage("GameBinding.Error5"), e);
            App.ShowError(App.GetLanguage("GameBinding.Error5"), e);
        }
    }

    private static async Task<(LoginObj?, string?)> GetUser()
    {
        var login = UserBinding.GetLastUser();
        if (login == null)
        {
            return (null, App.GetLanguage("GameBinding.Error2"));
        }
        if (login.AuthType == AuthType.Offline)
        {
            var have = AuthDatabase.Auths.Keys.Any(a => a.Item2 == AuthType.OAuth);
            if (!have)
            {
                BaseBinding.OpUrl("https://www.minecraft.net/");
                return (null, App.GetLanguage("GameBinding.Error7"));
            }
        }

        if (UserBinding.IsLock(login))
        {
            var res = await App.MainWindow!.Window.OkInfo.ShowWait(App.GetLanguage("GameBinding.Info1"));
            if (!res)
                return (null, App.GetLanguage("GameBinding.Error3"));
        }

        return (login, null);
    }

    public static async Task<(bool, string?)> Launch(IBaseWindow window, GameSettingObj? obj, WorldObj? world = null)
    {
        if (obj == null)
        {
            return (false, App.GetLanguage("GameBinding.Error1"));
        }

        if (BaseBinding.IsGameRun(obj))
        {
            return (false, App.GetLanguage("GameBinding.Error4"));
        }

        var user = await GetUser();
        if (user.Item1 == null)
        {
            return (false, user.Item2);
        }

        var res1 = await BaseBinding.Launch(window, obj, user.Item1, world);
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
        {
            return list;
        }

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

            obj1.Enable = !item.Disable;

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

    public static Task<bool> AddMods(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
    {
        var list = new List<string>();
        foreach (var item in file)
        {
            var item1 = item.GetPath();
            if (item1 != null)
            {
                list.Add(item1);
            }
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
            list.Add(item.FullName[dir..].Replace("\\", "/"));
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
            if (item.Extension is ".png" or ".lock")
                continue;
            list.Add(item.FullName[dir..].Replace("\\", "/"));
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

    public static async Task<ChunkData> ReadMca(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return await ChunkMca.Read(Path.GetFullPath(dir + "/" + name));
    }

    public static async Task<ChunkData> ReadMca(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return await ChunkMca.Read(Path.GetFullPath(dir + "/" + name));
    }

    public static async Task<NbtBase> ReadNbt(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return await NbtBase.Read(Path.GetFullPath(dir + "/" + name));
    }

    public static async Task<NbtBase> ReadNbt(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return await NbtBase.Read(Path.GetFullPath(dir + "/" + name));
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

        nbt.Save(Path.GetFullPath(dir + "/" + file));
    }

    public static void SaveNbtFile(GameSettingObj obj, string file, NbtBase nbt)
    {
        var dir = obj.GetGamePath();

        nbt.Save(Path.GetFullPath(dir + "/" + file));
    }

    public static void SaveMcaFile(WorldObj obj, string file, ChunkData data)
    {
        var dir = obj.Local;

        data.Save(Path.GetFullPath(dir + "/" + file));
    }

    public static void SaveMcaFile(GameSettingObj obj, string file, ChunkData data)
    {
        var dir = obj.GetGamePath();

        data.Save(Path.GetFullPath(dir + "/" + file));
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

    public static async Task<List<WorldDisplayObj>> GetWorlds(GameSettingObj obj)
    {
        var list = new List<WorldDisplayObj>();
        var res = await obj.GetWorlds();
        foreach (var item in res)
        {
            var obj1 = new WorldDisplayObj()
            {
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
            PathBinding.OpFile(file);
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

    public static async Task<List<ResourcepackDisplayObj>> GetResourcepacks(GameSettingObj obj)
    {
        var list = new List<ResourcepackDisplayObj>();
        var list1 = await obj.GetResourcepacks();
        var path = obj.GetGamePath();

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
                    Local = item.Local.Replace(path, ""),
                    PackFormat = item.pack_format,
                    Description = item.description,
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
        PathBinding.OpPath(obj, PathType.GamePath);
    }

    public static async Task<IEnumerable<ServerInfoObj>> GetServers(GameSettingObj obj)
    {
        return await obj.GetServerInfos();
    }

    public static async Task<List<ShaderpackDisplayObj>> GetShaderpacks(GameSettingObj obj)
    {
        var list = new List<ShaderpackDisplayObj>();
        foreach (var item in await obj.GetShaderpacks())
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

    public static async void DeleteServer(GameSettingObj obj, ServerInfoObj server)
    {
        var list = await obj.GetServerInfos();
        var list1 = list.ToList();
        var item = list1.First(a => a.Name == server.Name && a.IP == server.IP);
        list1.Remove(item);
        obj.SaveServer(list);
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

    public static void SetModInfo(GameSettingObj obj, CurseForgeModObj.Data? data)
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

        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];
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
                if (item.Obj.InJar?.Count > 0)
                {
                    foreach (var item1 in item.Obj.InJar)
                    {
                        modid.Add(item1.modid);
                    }
                }
                mod.Add(item);
            }
            modid.Add("forge");
            modid.Add("fabric");
            modid.Add("minecraft");
            modid.Add("java");
            modid.Add("fabricloader");

            ConcurrentBag<(string, List<string>)> lost = new();

            Parallel.ForEach(mod, item =>
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

            if (!lost.IsEmpty)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private static string GetString(this List<string> list)
    {
        var str = new StringBuilder();
        foreach (var item in list)
        {
            str.Append(item).Append(',');
        }

        return str.ToString()[..^1];
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

    public static Task<bool> ModPackUpdate(GameSettingObj obj, CurseForgeModObj.Data fid)
    {
        return obj.UpdateModPack(fid);
    }

    public static Task<bool> ModPackUpdate(GameSettingObj obj, ModrinthVersionObj fid)
    {
        return obj.UpdateModPack(fid);
    }

    public static List<ModDisplayModel> ModDisable(ModDisplayModel item, List<ModDisplayModel> items)
    {
        var list = new List<ModDisplayModel>();
        foreach (var item1 in items)
        {
            if (!item1.Enable || item1.Obj.modid == item.Obj.modid)
            {
                continue;
            }
            if (item1.Obj.dependencies != null)
            {
                if (item1.Obj.dependencies.Contains(item.Obj.modid))
                {
                    list.Add(item1);
                    continue;
                }
                else if (item.Obj.InJar != null)
                {
                    foreach (var item2 in item.Obj.InJar)
                    {
                        if (item1.Obj.dependencies.Contains(item2.modid))
                        {
                            list.Add(item1);
                            break;
                        }
                    }
                }
            }
            else if (item1.Obj.requiredMods != null)
            {
                if (item1.Obj.requiredMods.Contains(item.Obj.modid))
                {
                    list.Add(item1);
                    continue;
                }
                else if (item.Obj.InJar != null)
                {
                    foreach (var item2 in item.Obj.InJar)
                    {
                        if (item1.Obj.requiredMods.Contains(item2.modid))
                        {
                            list.Add(item1);
                            break;
                        }
                    }
                }
            }
        }

        return list;
    }

    public static Task<List<string>?> GetNeoForgeVersion(string version)
    {
        return ForgeAPI.GetVersionList(true, version, BaseClient.Source);
    }

    public static Task<List<string>?> GetNeoForgeSupportVersion()
    {
        return ForgeAPI.GetSupportVersion(true, BaseClient.Source);
    }

    public static void GameStateUpdate(GameSettingObj obj)
    {
        if (App.GameLogWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Update();
        }
    }

    public static async Task<bool> AddFile(GameSettingObj obj, IDataObject data, FileType type)
    {
        if (!data.Contains(DataFormats.Files))
        {
            return false;
        }
        var list = data.GetFiles();
        if (list == null)
        {
            return false;
        }
        switch (type)
        {
            case FileType.Mod:
                var list1 = new List<string>();
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        continue;
                    }
                    if (File.Exists(file) && file.ToLower().EndsWith(".jar"))
                    {
                        list1.Add(file);
                    }
                }

                return await obj.AddMods(list1);
            case FileType.World:
                foreach (var item in list)
                {
                    var file = item.TryGetLocalPath();
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        continue;
                    }
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
                    {
                        continue;
                    }
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

    public static PackType CheckType(string local)
    {
        if (File.Exists(local))
        {
            if (local.EndsWith(".mrpack"))
            {
                return PackType.Modrinth;
            }
            if (local.EndsWith(".zip"))
            {
                using ZipFile zFile = new(local);
                if (zFile.GetEntry("game.json") != null)
                {
                    return PackType.ColorMC;
                }
                else if (zFile.GetEntry("mcbbs.packmeta") != null)
                {
                    return PackType.HMCL;
                }
                else if (zFile.GetEntry("instance.cfg") != null)
                {
                    return PackType.MMC;
                }
                else if (zFile.GetEntry("manifest.json") != null)
                {
                    return PackType.CurseForge;
                }
            }
        }

        return PackType.ColorMC;
    }
}