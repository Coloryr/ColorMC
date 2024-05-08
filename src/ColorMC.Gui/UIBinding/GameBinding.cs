using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Chunk;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Chunk;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;

namespace ColorMC.Gui.UIBinding;

public static class GameBinding
{
    /// <summary>
    /// 是否不存在游戏
    /// </summary>
    public static bool IsNotGame => InstancesPath.IsNotGame;

    /// <summary>
    /// 获取游戏实例列表
    /// </summary>
    /// <returns></returns>
    public static List<GameSettingObj> GetGames()
    {
        return InstancesPath.Games;
    }

    /// <summary>
    /// 获取游戏版本号
    /// </summary>
    /// <param name="type1"></param>
    /// <param name="type2"></param>
    /// <param name="type3"></param>
    /// <returns></returns>
    public static async Task<List<string>> GetGameVersion(bool? type1, bool? type2, bool? type3)
    {
        var list = new List<string>();
        var ver = await VersionPath.GetVersionsAsync();
        if (ver == null)
        {
            return list;
        }

        foreach (var item in ver.versions)
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

    /// <summary>
    /// 添加游戏实例
    /// </summary>
    /// <param name="game"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>
    public static async Task<bool> AddGame(GameSettingObj game, ColorMCCore.Request request,
        ColorMCCore.GameOverwirte overwirte)
    {
        var game1 = await InstancesPath.CreateGame(game, request, overwirte);
        if (game1 != null)
        {
            ConfigBinding.SetLastLaunch(game1.UUID);
        }

        return game1 != null;
    }

    /// <summary>
    /// 添加文件夹
    /// </summary>
    /// <param name="path"></param>
    /// <param name="group"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>
    public static async Task<bool> AddGame(string path, string? group, ColorMCCore.Request request,
        ColorMCCore.GameOverwirte overwirte)
    {
        var files = Directory.GetFiles(path);
        var game = new GameSettingObj()
        {
            Version = (await GetGameVersion(true, false, false))[0],
            Loader = Loaders.Normal,
            LoaderVersion = null,
            GroupName = group
        };

        foreach (var item in files)
        {
            if (item.EndsWith(".json"))
            {
                var obj = JObject.Parse(PathHelper.ReadText(item)!);
                if (obj.TryGetValue("id", out var value))
                {
                    game.Name = value.ToString();
                }
                if (obj.TryGetValue("patches", out var patch) && patch is JArray array)
                {
                    foreach (var item1 in array)
                    {
                        var id = item1["id"]?.ToString();
                        var version = item1["version"]?.ToString() ?? "";
                        if (id == "game")
                        {
                            game.Version = version;
                        }
                        else if (id == "forge")
                        {
                            game.LoaderVersion = version;
                            game.Loader = Loaders.Forge;
                        }
                        else if (id == "fabric")
                        {
                            game.LoaderVersion = version;
                            game.Loader = Loaders.Fabric;
                        }
                        else if (id == "quilt")
                        {
                            game.LoaderVersion = version;
                            game.Loader = Loaders.Quilt;
                        }
                        else if (id == "neoforge")
                        {
                            game.LoaderVersion = version;
                            game.Loader = Loaders.NeoForge;
                        }
                    }
                    break;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(game.Name))
        {
            var info = new DirectoryInfo(path);
            game.Name = info.Name;
        }

        game = await InstancesPath.CreateGame(game, request, overwirte);
        if (game == null)
        {
            return false;
        }

        var res = await game.CopyFile(path, null);

        if (!res.Item1)
        {
            await game.Remove(request);
            App.ShowError(App.Lang("Gui.Error26"), res.Item2);
        }

        return true;
    }

    /// <summary>
    /// 导入文件夹
    /// </summary>
    /// <param name="name"></param>
    /// <param name="local"></param>
    /// <param name="unselect"></param>
    /// <param name="group"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>
    public static async Task<bool> AddGame(string name, string local, List<string> unselect,
        string? group, ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte)
    {
        var game = new GameSettingObj()
        {
            Name = name,
            Version = (await GetGameVersion(true, false, false))[0],
            Loader = Loaders.Normal,
            LoaderVersion = null,
            GroupName = group
        };

        game = await InstancesPath.CreateGame(game, request, overwirte);
        if (game == null)
        {
            return false;
        }

        var res = await game.CopyFile(local, unselect);

        if (!res.Item1)
        {
            await game.Remove(request);
            App.ShowError(App.Lang("Gui.Error26"), res.Item2);
        }

        var files = Directory.GetFiles(local);
        foreach (var item in files)
        {
            if (item.EndsWith(".json"))
            {
                try
                {
                    var obj = JObject.Parse(PathHelper.ReadText(item)!);
                    if (obj.TryGetValue("patches", out var patch) && patch is JArray array)
                    {
                        foreach (var item1 in array)
                        {
                            var id = item1["id"]?.ToString();
                            var version = item1["version"]?.ToString() ?? "";
                            if (id == "game")
                            {
                                game.Version = version;
                            }
                            else if (id == "forge")
                            {
                                game.LoaderVersion = version;
                                game.Loader = Loaders.Forge;
                            }
                            else if (id == "fabric")
                            {
                                game.LoaderVersion = version;
                                game.Loader = Loaders.Fabric;
                            }
                            else if (id == "quilt")
                            {
                                game.LoaderVersion = version;
                                game.Loader = Loaders.Quilt;
                            }
                            else if (id == "neoforge")
                            {
                                game.LoaderVersion = version;
                                game.Loader = Loaders.NeoForge;
                            }
                        }
                        game.Save();
                        break;
                    }
                }
                catch
                {

                }
            }
        }

        App.ShowGameEdit(game);

        return true;
    }

    /// <summary>
    /// 导入压缩包
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="zip"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <param name="update"></param>
    /// <param name="update2"></param>
    /// <returns></returns>
    public static Task<(bool, GameSettingObj?)> AddPack(string dir, PackType type, string? name,
        string? group, ColorMCCore.ZipUpdate zip, ColorMCCore.Request request,
        ColorMCCore.GameOverwirte overwirte, ColorMCCore.PackUpdate update, ColorMCCore.PackState update2)
    {
        return InstallGameHelper.InstallZip(dir, type, name, group, zip, request, overwirte,
            update, update2);
    }

    /// <summary>
    /// 获取游戏分组
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, List<GameSettingObj>> GetGameGroups()
    {
        return InstancesPath.Groups;
    }

    /// <summary>
    /// 获取CF支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetCurseForgeGameVersions()
    {
        return CurseForgeHelper.GetGameVersions();
    }

    /// <summary>
    /// 获取MO支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static Task<List<string>?> GetModrinthGameVersions()
    {
        return ModrinthHelper.GetGameVersion();
    }

    /// <summary>
    /// 获取CF分组
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Task<Dictionary<string, string>?> GetCurseForgeCategories(
        FileType type = FileType.ModPack)
    {
        return CurseForgeHelper.GetCurseForgeCategories(type);
    }

    /// <summary>
    /// 获取MO分组
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Task<Dictionary<string, string>?> GetModrinthCategories(FileType type = FileType.ModPack)
    {
        return ModrinthHelper.GetModrinthCategories(type);
    }

    /// <summary>
    /// 安装CF整合包
    /// </summary>
    /// <param name="data"></param>
    /// <param name="data1"></param>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="zip"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <param name="update"></param>
    /// <param name="update2"></param>
    /// <returns></returns>
    public static async Task<bool> InstallCurseForge(CurseForgeModObj.Data data,
        CurseForgeObjList.Data data1, string? name, string? group, ColorMCCore.ZipUpdate zip,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        var res = await InstallGameHelper.InstallCurseForge(data, name, group, zip, request, overwirte,
            update, update2);
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

    /// <summary>
    /// 安装MO整合包
    /// </summary>
    /// <param name="data"></param>
    /// <param name="data1"></param>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="zip"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <param name="update"></param>
    /// <param name="update2"></param>
    /// <returns></returns>
    public static async Task<bool> InstallModrinth(ModrinthVersionObj data,
        ModrinthSearchObj.Hit data1, string? name, string? group, ColorMCCore.ZipUpdate zip,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        var res = await InstallGameHelper.InstallModrinth(data, name, group, zip, request, overwirte,
            update, update2);
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

    /// <summary>
    /// 设置游戏实例图标
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task SetGameIconFromUrl(GameSettingObj obj, string url)
    {
        try
        {
            var data = await BaseClient.GetBytesAsync(url);
            if (data.Item1)
            {
                await File.WriteAllBytesAsync(obj.GetIconFile(), data.Item2!);
            }
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("Gui.Error45"), e);
            App.ShowError(App.Lang("Gui.Error45"), e);
        }
    }

    /// <summary>
    /// 设置游戏实例图标
    /// </summary>
    /// <param name="model"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task SetGameIconFromFile(BaseModel model, GameSettingObj obj)
    {
        try
        {
            var file = await PathBinding.SelectFile(FileType.Icon);
            if (file.Item1 != null)
            {
                model.Progress(App.Lang("Gui.Info30"));
                using var info = SKBitmap.Decode(PathHelper.OpenRead(file.Item1)!);
                if (info.Width > 200 || info.Height > 200)
                {
                    using var image = await Task.Run(() =>
                    {
                        return ImageUtils.Resize(info, 200, 200);
                    });
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    PathHelper.WriteBytes(obj.GetIconFile(), data.AsSpan().ToArray());
                }
                else
                {
                    using var data = info.Encode(SKEncodedImageFormat.Png, 100);
                    PathHelper.WriteBytes(obj.GetIconFile(), data.AsSpan().ToArray());
                }

                model.ProgressClose();
                model.Notify(App.Lang("Gui.Info29"));
            }
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("Gui.Error43"), e);
            App.ShowError(App.Lang("Gui.Error43"), e);
        }
    }

    /// <summary>
    /// 获取选中的账户
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private static async Task<(LoginObj?, string?)> GetUser(BaseModel model)
    {
        var login = UserBinding.GetLastUser();
        if (login == null)
        {
            return (null, App.Lang("Gui.Error40"));
        }
        if (login.AuthType == AuthType.Offline)
        {
            var have = AuthDatabase.Auths.Keys.Any(a => a.Item2 == AuthType.OAuth);
            if (!have)
            {
                BaseBinding.OpUrl(UrlHelper.Minecraft);
                return (null, App.Lang("Gui.Error44"));
            }
        }

        if (UserBinding.IsLock(login))
        {
            var res = await model.ShowWait(App.Lang("Gui.Info36"));
            if (!res)
                return (null, App.Lang("Gui.Error41"));
        }

        return (login, null);
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="model"></param>
    /// <param name="obj"></param>
    /// <param name="world"></param>
    /// <param name="hide"></param>
    /// <returns></returns>
    public static async Task<(bool, string?)> Launch(BaseModel model, GameSettingObj? obj,
        WorldObj? world = null, bool hide = false)
    {
        if (obj == null)
        {
            return (false, App.Lang("Gui.Error39"));
        }

        if (BaseBinding.IsGameRun(obj))
        {
            return (false, App.Lang("Gui.Error42"));
        }

        var user = await GetUser(model);
        if (user.Item1 == null)
        {
            return (false, user.Item2);
        }

        var res1 = await BaseBinding.Launch(model, obj, user.Item1, world, hide);
        if (res1.Item1)
        {
            ConfigBinding.SetLastLaunch(obj.UUID);
        }
        return res1;
    }

    /// <summary>
    /// 添加游戏分组
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool AddGameGroup(string name)
    {
        return InstancesPath.AddGroup(name);
    }

    /// <summary>
    /// 移动游戏到分组
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="now"></param>
    public static void MoveGameGroup(GameSettingObj obj, string? now)
    {
        obj.MoveGameGroup(now);
        App.MainWindow?.LoadMain();
    }

    /// <summary>
    /// 重新获取游戏版本
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> ReloadVersion()
    {
        await VersionPath.GetFromWebAsync();

        return await VersionPath.IsHaveVersionInfoAsync();
    }

    //public static async void SaveGame(GameSettingObj obj, string? versi, Loaders loader, string? loadv)
    //{
    //    if (!string.IsNullOrWhiteSpace(versi))
    //    {
    //        obj.Version = versi;
    //        var ver = await VersionPath.GetVersionsAsync();
    //        var version1 = ver!.versions.FirstOrDefault(a => a.id == versi);
    //        if (version1 != null)
    //        {
    //            if (version1.type == "release")
    //            {
    //                obj.GameType = GameType.Release;
    //            }
    //            else if (version1.type == "snapshot")
    //            {
    //                obj.GameType = GameType.Snapshot;
    //            }
    //            else
    //            {
    //                obj.GameType = GameType.Other;
    //            }
    //        }
    //    }
    //    obj.Loader = loader;
    //    if (!string.IsNullOrWhiteSpace(loadv))
    //    {
    //        obj.LoaderVersion = loadv;
    //    }
    //    obj.Save();
    //}

    /// <summary>
    /// 设置游戏Jvm参数
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public static void SetGameJvmMemArg(GameSettingObj obj, uint? min, uint? max)
    {
        obj.JvmArg ??= new();
        obj.JvmArg.MinMemory = min;
        obj.JvmArg.MaxMemory = max;
        obj.Save();
    }

    /// <summary>
    /// 设置游戏Jvm参数
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    public static void SetGameJvmArg(GameSettingObj obj, RunArgObj obj1)
    {
        obj.JvmArg = obj1;
        obj.Save();
    }

    /// <summary>
    /// 设置游戏窗口
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    public static void SetGameWindow(GameSettingObj obj, WindowSettingObj obj1)
    {
        obj.Window = obj1;
        obj.Save();
    }

    /// <summary>
    /// 设置游戏加入的服务器
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    public static void SetGameServer(GameSettingObj obj, ServerObj obj1)
    {
        obj.StartServer = obj1;
        obj.Save();
    }

    /// <summary>
    /// 设置游戏端口代理
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    public static void SetGameProxy(GameSettingObj obj, ProxyHostObj obj1)
    {
        obj.ProxyHost = obj1;
        obj.Save();
    }

    /// <summary>
    /// 获取游戏的模组列表
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="sha256"></param>
    /// <returns></returns>
    public static async Task<List<ModDisplayModel>> GetGameMods(GameSettingObj obj,
        bool sha256 = false)
    {
        var list = new List<ModDisplayModel>();
        var list1 = await obj.GetModsAsync(sha256);
        if (list1 == null)
        {
            return list;
        }

        list1.ForEach(item =>
        {
            var obj1 = new ModDisplayModel()
            {
                Name = item.ReadFail ? App.Lang("GameEditWindow.Tab4.Info5")
                : item.name,
                Obj = item
            };

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

    /// <summary>
    /// 启用/禁用模组
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static (bool, string?) ModEnableDisable(ModObj obj)
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

            return (true, null);
        }
        catch (Exception e)
        {
            string temp = string.Format(App.Lang("GameEditWindow.Tab4.Error3"), obj.Local);
            Logs.Error(temp, e);
            return (false, temp);
        }
    }

    /// <summary>
    /// 删除模组
    /// </summary>
    /// <param name="mod"></param>
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

    /// <summary>
    /// 添加模组
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <returns></returns>
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
        return obj.AddModsAsync(list);
    }

    /// <summary>
    /// 获取所有配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
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

        var list1 = PathHelper.GetAllFile(con);
        foreach (var item in list1)
        {
            list.Add(item.FullName[dir..].Replace("\\", "/"));
        }

        return list;
    }

    /// <summary>
    /// 获取世界所有配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string> GetAllConfig(WorldObj obj)
    {
        var list = new List<string>();
        var dir = obj.Local.Length + 1;

        var list1 = PathHelper.GetAllFile(obj.Local);
        foreach (var item in list1)
        {
            if (item.Extension is ".png" or ".lock")
                continue;
            list.Add(item.FullName[dir..].Replace("\\", "/"));
        }

        return list;
    }

    /// <summary>
    /// 获取根目录所有配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 读取区块信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<ChunkDataObj?> ReadMca(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return await ChunkMca.ReadAsync(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取区块信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<ChunkDataObj?> ReadMca(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return await ChunkMca.ReadAsync(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取Nbt信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<NbtBase?> ReadNbt(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return await NbtBase.Read(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取Nbt信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<NbtBase?> ReadNbt(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return await NbtBase.Read(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string ReadConfigFile(WorldObj obj, string name)
    {
        var dir = obj.Local;

        return File.ReadAllText(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string ReadConfigFile(GameSettingObj obj, string name)
    {
        var dir = obj.GetGamePath();

        return File.ReadAllText(Path.GetFullPath(dir + "/" + name));
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="text"></param>
    public static void SaveConfigFile(WorldObj obj, string name, string? text)
    {
        var dir = obj.Local;

        File.WriteAllText(Path.GetFullPath(dir + "/" + name), text);
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="text"></param>
    public static void SaveConfigFile(GameSettingObj obj, string name, string? text)
    {
        var dir = obj.GetGamePath();

        File.WriteAllText(Path.GetFullPath(dir + "/" + name), text);
    }

    /// <summary>
    /// 保存Nbt文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <param name="nbt"></param>
    public static void SaveNbtFile(WorldObj obj, string file, NbtBase nbt)
    {
        var dir = obj.Local;

        nbt.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 保存Nbt文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <param name="nbt"></param>
    public static void SaveNbtFile(GameSettingObj obj, string file, NbtBase nbt)
    {
        var dir = obj.GetGamePath();

        nbt.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 保存区块文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <param name="data"></param>
    public static void SaveMcaFile(WorldObj obj, string file, ChunkDataObj data)
    {
        var dir = obj.Local;

        data.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 保存区块文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <param name="data"></param>
    public static void SaveMcaFile(GameSettingObj obj, string file, ChunkDataObj data)
    {
        var dir = obj.GetGamePath();

        data.Save(Path.GetFullPath(dir + "/" + file));
    }

    /// <summary>
    /// 获取世界列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Task<List<WorldObj>> GetWorlds(GameSettingObj obj)
    {
        return obj.GetWorldsAsync();
    }

    /// <summary>
    /// 添加世界
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static async Task<bool> AddWorld(GameSettingObj obj, string? file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return false;
        }
        var res = await obj.AddWorldZipAsync(file);
        if (!res)
        {
            PathBinding.OpFile(file);
        }

        return res;
    }

    /// <summary>
    /// 删除世界
    /// </summary>
    /// <param name="world"></param>
    public static void DeleteWorld(WorldObj world)
    {
        world.Remove();
    }

    /// <summary>
    /// 导出世界
    /// </summary>
    /// <param name="world"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Task ExportWorld(WorldObj world, string? file)
    {
        if (file == null)
            return Task.CompletedTask;

        return world.ExportWorldZip(file);
    }

    /// <summary>
    /// 获取资源包列表
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="sha256"></param>
    /// <returns></returns>
    public static Task<List<ResourcepackObj>> GetResourcepacks(GameSettingObj obj,
        bool sha256 = false)
    {
        return obj.GetResourcepacksAsync(sha256);
    }

    /// <summary>
    /// 删除资源包
    /// </summary>
    /// <param name="obj"></param>
    public static void DeleteResourcepack(ResourcepackObj obj)
    {
        obj.Delete();
    }

    /// <summary>
    /// 导入资源包
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Task<bool> AddResourcepack(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
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
        return obj.AddResourcepackAsync(list);
    }

    /// <summary>
    /// 删除截图
    /// </summary>
    /// <param name="file"></param>
    public static void DeleteScreenshot(string file)
    {
        Screenshots.Delete(file);
    }

    /// <summary>
    /// 删除所有截图
    /// </summary>
    /// <param name="obj"></param>
    public static void ClearScreenshots(GameSettingObj obj)
    {
        obj.ClearScreenshots();
    }

    /// <summary>
    /// 获取所有截图
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string> GetScreenshots(GameSettingObj obj)
    {
        return obj.GetScreenshots();
    }

    /// <summary>
    /// 获取游戏实例
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns></returns>
    public static GameSettingObj? GetGame(string? uuid)
    {
        return InstancesPath.GetGame(uuid);
    }
        
    /// <summary>
    /// 获取服务器列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<ServerInfoObj>> GetServers(GameSettingObj obj)
    {
        return await obj.GetServerInfosAsync();
    }

    /// <summary>
    /// 添加服务器
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static Task AddServer(GameSettingObj obj, string name, string ip)
    {
        return obj.AddServerAsync(name, ip);
    }

    /// <summary>
    /// 删除服务器
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="server"></param>
    /// <returns></returns>
    public static Task DeleteServer(GameSettingObj obj, ServerInfoObj server)
    {
        return obj.RemoveServerAsync(server.Name, server.IP);
    }

    /// <summary>
    /// 删除配置文件
    /// </summary>
    /// <param name="obj"></param>
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

    //public static void SetAdvanceJvmArg(GameSettingObj obj, AdvanceJvmObj obj1)
    //{
    //    obj.AdvanceJvm = obj1;

    //    obj.Save();
    //}

    /// <summary>
    /// 获取光影包列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Task<List<ShaderpackObj>> GetShaderpacks(GameSettingObj obj)
    {
        return obj.GetShaderpacksAsync();
    }

    /// <summary>
    /// 添加光影包
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Task<bool> AddShaderpack(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
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

        return obj.AddShaderpackAsync(list);
    }

    /// <summary>
    /// 删除光影包
    /// </summary>
    /// <param name="obj"></param>
    public static void DeleteShaderpack(ShaderpackObj obj)
    {
        obj.Delete();
    }

    /// <summary>
    /// 获取结构文件列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<List<SchematicObj>> GetSchematics(GameSettingObj obj)
    {
        var list = await obj.GetSchematicsAsync();
        var list1 = new List<SchematicObj>();
        foreach (var item in list)
        {
            if (item.Broken)
            {
                list1.Add(new()
                {
                    Name = App.Lang("Gui.Error14"),
                    Local = item.Local,
                });
            }
            else
            {
                list1.Add(item);
            }
        }

        return list1;
    }

    /// <summary>
    /// 添加结构文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool AddSchematic(GameSettingObj obj, IReadOnlyList<IStorageFile> file)
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

        return obj.AddSchematic(list);
    }

    /// <summary>
    /// 删除结构文件
    /// </summary>
    /// <param name="obj"></param>
    public static void DeleteSchematic(SchematicObj obj)
    {
        obj.Delete();
    }

    /// <summary>
    /// 设置模组信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
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
        if (!obj.Mods.TryAdd(obj1.ModId, obj1))
        {
            obj.Mods[obj1.ModId] = obj1;
        }

        obj.SaveModInfo();
    }

    /// <summary>
    /// 设置模组信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    public static void SetModInfo(GameSettingObj obj, ModrinthVersionObj? data)
    {
        if (data == null)
        {
            return;
        }

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
        if (!obj.Mods.TryAdd(obj1.ModId, obj1))
        {
            obj.Mods[obj1.ModId] = obj1;
        }

        obj.SaveModInfo();
    }

    /// <summary>
    /// 备份世界
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    public static async Task<bool> BackupWorld(WorldObj world)
    {
        try
        {
            await world.BackupAsync();
            return true;
        }
        catch (Exception e)
        {
            string text = App.Lang("Gui.Error20");
            Logs.Error(text, e);
            App.ShowError(text, e);
            return false;
        }
    }

    /// <summary>
    /// 还原世界
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="item1"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Task<bool> BackupWorld(GameSettingObj obj, FileInfo item1, ColorMCCore.Request request)
    {
        return obj.UnzipBackupWorldAsync(item1, request);
    }

    /// <summary>
    /// 设置游戏名字
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    public static void SetGameName(GameSettingObj obj, string data)
    {
        obj.Name = data;
        obj.Save();

        App.MainWindow?.LoadMain();
    }

    /// <summary>
    /// 复制游戏实例
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <param name="request"></param>
    /// <param name="overwirte"></param>
    /// <returns></returns>
    public static async Task<bool> CopyGame(GameSettingObj obj, string data,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte)
    {
        if (BaseBinding.IsGameRun(obj))
        {
            return false;
        }

        if (await obj.Copy(data, request, overwirte) == null)
        {
            return false;
        }

        App.MainWindow?.LoadMain();

        return true;
    }

    /// <summary>
    /// 保存服务器包
    /// </summary>
    /// <param name="obj1"></param>
    public static void SaveServerPack(ServerPackObj obj1)
    {
        obj1.Save();
    }

    /// <summary>
    /// 获取服务器包
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static ServerPackObj? GetServerPack(GameSettingObj obj)
    {
        return obj.GetServerPack().Item2;
    }

    /// <summary>
    /// 生成服务器包
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="local"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Task<bool> GenServerPack(ServerPackObj obj, string local,
        ColorMCCore.Request request)
    {
        return obj.GenServerPackAsync(local, request);
    }

    /// <summary>
    /// 复制服务器地址到剪贴板
    /// </summary>
    /// <param name="obj"></param>
    public static async void CopyServer(ServerInfoObj obj)
    {
        await BaseBinding.CopyTextClipboard($"{obj.Name}\n{obj.IP}");
    }

    /// <summary>
    /// 模组检测
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static Task<bool> ModCheck(List<ModDisplayModel> list)
    {
        return Task.Run(() =>
        {
            var modid = new List<string>();
            var mod = new List<ModDisplayModel>();
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

            var lost = new ConcurrentBag<(string, List<string>)>();

            Parallel.ForEach(mod, item =>
            {
                if (item == null)
                {
                    return;
                }

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
                static string GetString(List<string> list)
                {
                    var str = new StringBuilder();
                    foreach (var item in list)
                    {
                        str.Append(item).Append(',');
                    }

                    return str.ToString()[..^1];
                }

                var str = new StringBuilder();
                foreach (var item in lost)
                {
                    str.Append(string.Format(App.Lang("Gui.Info25"), item.Item1,
                        GetString(item.Item2))).Append(Environment.NewLine);
                }

                App.ShowError(App.Lang("Gui.Info26"), str.ToString());
                return false;
            }

            return true;
        });
    }

    /// <summary>
    /// 获取日志文件列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string> GetLogList(GameSettingObj obj)
    {
        return obj.GetLogFiles();
    }

    /// <summary>
    /// 读取日志文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<string?> ReadLog(GameSettingObj obj, string name)
    {
        if (BaseBinding.IsGameRun(obj))
        {
            if (name.EndsWith("latest.log") || name.EndsWith("debug.log"))
                return null;
        }
        return await Task.Run(() => obj.ReadLog(name));
    }

    public static Task<bool> ModPackUpdate(GameSettingObj obj, CurseForgeModObj.Data fid,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        return obj.UpdateModPack(fid, update, update2);
    }

    public static Task<bool> ModPackUpdate(GameSettingObj obj, ModrinthVersionObj fid,
        ColorMCCore.PackUpdate update,
        ColorMCCore.PackState update2)
    {
        return obj.UpdateModPack(fid, update, update2);
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

    /// <summary>
    /// 游戏实例状态发生改变
    /// </summary>
    /// <param name="obj"></param>
    public static void GameStateUpdate(GameSettingObj obj)
    {
        if (App.GameLogWindows.TryGetValue(obj.UUID, out var win1))
        {
            win1.Update();
        }
    }

    /// <summary>
    /// 拖拽添加文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="data"></param>
    /// <param name="type"></param>
    /// <returns></returns>
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

                return await obj.AddModsAsync(list1);
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
                        return await obj.AddWorldZipAsync(file);
                    }
                }
                return false;
            case FileType.Resourcepack:
                list1 = [];
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
                return await obj.AddResourcepackAsync(list1);
            case FileType.Shaderpack:
                list1 = [];
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
                return await obj.AddShaderpackAsync(list1);
            case FileType.Schematic:
                list1 = [];
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
        Stream? stream = PathHelper.OpenRead(local);

        if (stream == null)
        {
            return PackType.ColorMC;
        }

        try
        {
            if (local.EndsWith(".mrpack"))
            {
                return PackType.Modrinth;
            }
            if (local.EndsWith(".zip"))
            {
                using ZipFile zFile = new(stream);
                foreach (ZipEntry item in zFile)
                {
                    if (item.Name.EndsWith("game.json"))
                    {
                        return PackType.ColorMC;
                    }
                    else if (item.Name.EndsWith("mcbbs.packmeta"))
                    {
                        return PackType.HMCL;
                    }
                    else if (item.Name.EndsWith("instance.cfg"))
                    {
                        return PackType.MMC;
                    }
                    else if (item.Name.EndsWith("manifest.json"))
                    {
                        return PackType.CurseForge;
                    }
                    else if (item.Name.Contains(".minecraft/"))
                    {
                        return PackType.ZipPack;
                    }
                }
            }
        }
        finally
        {
            stream.Close();
            stream.Dispose();
        }

        return PackType.ColorMC;
    }

    public static async Task<bool> UnZipCloudConfig(GameSettingObj obj, CloudDataObj data, string local)
    {
        data.Config.Clear();
        return await Task.Run(() =>
        {
            try
            {
                using var s = new ZipInputStream(PathHelper.OpenRead(local));
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string filename = $"{obj.GetBasePath()}/{theEntry.Name}";
                    data.Config.Add(theEntry.Name);
                    var directoryName = Path.GetDirectoryName(filename);
                    string fileName = Path.GetFileName(theEntry.Name);

                    if (directoryName?.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fileName != string.Empty)
                    {
                        using var streamWriter = PathHelper.OpenWrite(filename);

                        s.CopyTo(streamWriter);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("AddGameWindow.Tab1.Error17"), e);
            }
            return false;
        });
    }
    public static async Task<(bool, string?)> DownloadCloud(CloundListObj obj, string? group,
        ColorMCCore.Request request, ColorMCCore.GameOverwirte overwirte)
    {
        var game = await InstancesPath.CreateGame(new()
        {
            Name = obj.Name,
            UUID = obj.UUID,
            GroupName = group
        }, request, overwirte);
        if (game == null)
        {
            return (false, App.Lang("AddGameWindow.Tab1.Error10"));
        }

        var cloud = new CloudDataObj()
        {
            Config = []
        };

        GameCloudUtils.SetCloudData(game, cloud);
        string local = Path.GetFullPath(game.GetBasePath() + "/config.zip");
        var res = await GameCloudUtils.DownloadConfig(obj, local);
        if (res == 100)
        {
            await UnZipCloudConfig(game, cloud, local);
        }
        else
        {
            //return (false, App.Lang("AddGameWindow.Tab1.Error11"));
        }

        var temp = await GameCloudUtils.HaveCloud(game);
        try
        {
            cloud.ConfigTime = DateTime.Parse(temp.Item3!);
        }
        catch
        {

        }
        GameCloudUtils.SetCloudData(game, cloud);

        game = game.Reload();

        if (game.Mods != null)
        {
            var list = new List<DownloadItemObj>();
            foreach (var item in game.Mods.Values)
            {
                list.Add(new()
                {
                    Url = item.Url,
                    Name = item.File,
                    Local = game.GetGamePath() + "/" + item.Path + "/" + item.File,
                    SHA1 = item.SHA1
                });
            }

            if (list.Count > 0)
            {
                var res1 = await DownloadManager.StartAsync(list);
                if (!res1)
                {
                    return (false, App.Lang("AddGameWindow.Tab1.Error12"));
                }
            }
        }

        return (true, null);
    }

    public static GameSettingObj? GetGameByName(string name)
    {
        return InstancesPath.GetGameByName(name);
    }

    public static async void CheckCloudAndOpen(GameSettingObj obj)
    {
        var res = await GameCloudUtils.HaveCloud(obj);
        if (res.Item1 == 100 && res.Item2)
        {
            Dispatcher.UIThread.Post(() =>
            {
                App.ShowGameCloud(obj, true);
            });
        }
    }

    public static async Task<bool> DeleteGame(GameSettingObj obj,
        ColorMCCore.Request request)
    {
        var res = await obj.Remove(request);
        App.MainWindow?.LoadMain();
        if (res)
        {
            Dispatcher.UIThread.Post(() =>
            {
                App.CloseGameWindow(obj);
            });
        }

        return res;
    }

    public static async Task<(bool, string?)> DownloadServerPack(BaseModel model,
        string? name, string? group, string text, ColorMCCore.GameOverwirte overwirte)
    {
        try
        {
            var data = await BaseClient.GetStringAsync(text + "server.json");
            if (!data.Item1)
            {
                return (false, App.Lang("AddGameWindow.Tab1.Error15"));
            }
            var obj = JsonConvert.DeserializeObject<ServerPackObj>(data.Item2!);
            if (obj == null)
            {
                return (false, App.Lang("AddGameWindow.Tab1.Error16"));
            }

            var game = obj.Game;
            if (!string.IsNullOrWhiteSpace(name))
            {
                game.Name = name;
            }
            if (!string.IsNullOrWhiteSpace(group))
            {
                game.GroupName = group;
            }
            game.UUID = null!;
            game.LaunchData = null!;
            game.ServerUrl = text;
            game.ModPackType = SourceType.ColorMC;
            game = await InstancesPath.CreateGame(game, model.ShowWait, overwirte);

            if (game == null)
            {
                return (false, App.Lang("AddGameWindow.Tab1.Error10"));
            }

            model.Progress(App.Lang("AddGameWindow.Tab1.Info15"));

            var res1 = await obj.UpdateAsync(game, (text) =>
            {
                if (text == null)
                {
                    model.ProgressClose();
                }
                else
                {
                    model.Progress(text);
                }
            });
            if (!res1)
            {
                model.ProgressClose();
                model.ShowOk(App.Lang("AddGameWindow.Tab1.Error12"), async () =>
                {
                    await game.Remove(model.ShowWait);
                });

                return (false, null);
            }

            PathHelper.WriteText(game.GetServerPackFile(), data.Item2!);

            return (true, null);
        }
        catch (Exception e)
        {
            string temp = App.Lang("AddGameWindow.Tab1.Error16");
            Logs.Error(temp, e);
            return (false, temp);
        }
    }

    public static bool DataPackDisE(DataPackObj obj)
    {
        if (BaseBinding.IsGameRun(obj.World.Game))
        {
            return false;
        }
        return DataPack.DisEna([obj], obj.World);
    }

    public static bool DataPackDisE(IEnumerable<DataPackModel> pack)
    {
        var list = new List<DataPackObj>();
        foreach (var item in pack)
        {
            list.Add(item.Pack);
        }
        if (BaseBinding.IsGameRun(list[0].World.Game))
        {
            return false;
        }
        return DataPack.DisEna(list, list[0].World);
    }

    public static async Task<bool> DeleteDataPack(DataPackModel item, ColorMCCore.Request request)
    {
        if (BaseBinding.IsGameRun(item.Pack.World.Game))
        {
            return false;
        }
        return await DataPack.DeleteAsync([item.Pack], item.Pack.World, request);
    }

    public static async Task<bool> DeleteDataPack(IEnumerable<DataPackModel> items, ColorMCCore.Request request)
    {
        var list = new List<DataPackObj>();
        foreach (var item in items)
        {
            list.Add(item.Pack);
        }
        if (BaseBinding.IsGameRun(list[0].World.Game))
        {
            return false;
        }
        return await DataPack.DeleteAsync(list, list[0].World, request);
    }

    public static async Task<List<Loaders>> GetSupportLoader(string version)
    {
        var loaders = new List<Loaders>();
        Task[] list =
        [
            Task.Run(async () =>
            {
                var list = await WebBinding.GetForgeSupportVersion();
                if (list != null && list.Contains(version))
                {
                    loaders.Add(Loaders.Forge);
                }
            }),
            Task.Run(async () =>
            {
                var list = await WebBinding.GetFabricSupportVersion();
                if (list != null && list.Contains(version))
                {
                    loaders.Add(Loaders.Fabric);
                }
            }),
            Task.Run(async () =>
            {
                var list = await WebBinding.GetQuiltSupportVersion();
                if (list != null && list.Contains(version))
                {
                    loaders.Add(Loaders.Quilt);
                }
            }),
            Task.Run(async () =>
            {
                var list = await WebBinding.GetNeoForgeSupportVersion();
                if (list != null && list.Contains(version))
                {
                    loaders.Add(Loaders.NeoForge);
                }
            }),
            Task.Run(async () =>
            {
                var list = await WebBinding.GetOptifineSupportVersion();
                if (list != null && list.Contains(version))
                {
                    loaders.Add(Loaders.OptiFine);
                }
            })
        ];

        await Task.WhenAll(list);

        loaders.Sort();

        return loaders;
    }

    private static string MakeString(this List<string> list)
    {
        var str = new StringBuilder();
        foreach (var item in list)
        {
            str.Append(item).Append(", ");
        }

        if (str.Length > 0)
        {
            return str.ToString()[..^1];
        }

        return "";
    }

    public static async Task GenGameInfo(GameSettingObj obj)
    {
        var list = await obj.GetModsAsync(false);
        var info = new StringBuilder();
        await Task.Run(() =>
        {
            info.AppendLine($"ColorMC:{ColorMCCore.Version}")
                .AppendLine($"{App.Lang("Gui.Info44")}{obj.Name}")
                .AppendLine($"{App.Lang("Gui.Info45")}{obj.Version}");
            if (obj.ModPack)
            {
                info.AppendLine(string.Format(App.Lang("Gui.Info51"), obj.ModPackType.GetName(), obj.PID, obj.FID));
            }
            if (obj.Loader != Loaders.Normal)
            {
                if (obj.Loader == Loaders.Custom)
                {
                    info.AppendLine(string.Format(App.Lang("Gui.Info47"), GameBinding.GetGameLoader(obj), obj.CustomLoader?.OffLib));
                }
                else
                {
                    info.AppendLine(string.Format(App.Lang("Gui.Info46"), obj.Loader.GetName(), obj.LoaderVersion));
                }

                if (list.Count != 0)
                {
                    info.AppendLine(App.Lang("Gui.Info48"));

                    foreach (var item in list)
                    {
                        info.AppendLine(string.Format(App.Lang("Gui.Info49"), item.name, item.modid, item.authorList?.MakeString(), Path.GetFileName(item.Local), item.Disable, item.CoreMod, item.Sha1, item.Loader.GetName()));
                        if (obj.Mods.Values.FirstOrDefault(item => item.SHA1 == item.SHA1) is { } item1)
                        {
                            info.AppendLine(string.Format(App.Lang("Gui.Info50"), item1.Type.GetName(), item1.ModId, item1.FileId));
                        }
                    }
                }
            }
        });
        App.ShowError(App.Lang("Gui.Info52"), info.ToString(), false);
    }

    public static async Task<string> GetGameLoader(GameSettingObj obj)
    {
        var res = await obj.GetGameLoaderInfo();
        if (res != null)
        {
            return res;
        }
        else
        {
            return App.Lang("GameEditWindow.Tab1.Error4");
        }
    }

    public static Task<(bool, string?)> SetGameLoader(GameSettingObj obj, string path)
    {
        return obj.SetGameLoader(path);
    }
}