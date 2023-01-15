using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class GameBinding
{
    private readonly static List<string> SortOrder = new()
    {
        Localizer.Instance["GameBinding.SortOrder.Item1"],
        Localizer.Instance["GameBinding.SortOrder.Item2"]
    };
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

    public static Task<bool> AddPack(string dir, PackType type)
    {
        return InstancesPath.InstallFromZip(dir, type);
    }

    public static Dictionary<string, List<GameSettingObj>> GetGameGroups()
    {
        return InstancesPath.Groups;
    }

    public static Task<CurseForgeObj?> GetPackList(string version, int sort, string filter, int page, int sortOrder)
    {
        return CurseForge.GetPackList(version, page, sort, filter, sortOrder: sortOrder);
    }

    public static List<string> GetCurseForgeTypes()
    {
        var list = new List<string>();
        Array values = Enum.GetValues(typeof(SortField));
        foreach (SortField value in values)
        {
            list.Add(value.GetName());
        }

        return list;
    }

    public static List<string> GetSortOrder()
    {
        return SortOrder;
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

    public static Task<bool> InstallCurseForge(CurseForgeObj.Data.LatestFiles data)
    {
        return InstancesPath.InstallFromCurseForge(data);
    }

    public static Task<CurseForgeFileObj?> GetPackFile(long id, int page)
    {
        return CurseForge.GetCurseForgeFiles(id, page);
    }

    public static async Task<(bool, string?)> Launch(GameSettingObj? obj, bool debug)
    {
        if (obj == null)
        {
            return (false, "没有选择游戏实例");
        }

        var login = UserBinding.GetLastUser();
        if (login == null)
        {
            return (false, "没有选择账户");
        }

        if (UserBinding.IsLock(login))
        {
            var res = await App.MainWindow!.Info.ShowWait("用户已被占用，是否继续使用这个账户");
            if (!res)
                return (false, "账户冲突");
        }

        if (debug)
        {
            App.ShowGameEdit(obj, 6);
        }

        return (await BaseBinding.Launch(obj, login), null);
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

    public static Task<bool> ReloadVersion()
    {
        return VersionPath.GetFromWeb();
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

    public static void DeleteMod(ModObj obj)
    {
        obj.Delete();
    }

    public static void AddMods(GameSettingObj obj, string[] file)
    {
        foreach (var item in file)
        {
            var info = new FileInfo(item);
            var info1 = new FileInfo(Path.GetFullPath(obj.GetModsPath() + info.Name));
            if (info1.Exists)
            {
                info1.Delete();
            }

            File.Copy(info.FullName, info1.FullName);
        }
    }

    public static void OpFile(string item)
    {
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                System.Diagnostics.Process.Start("explorer",
                    $@"/select,{item}");
                break;
            case OsType.Linux:
                System.Diagnostics.Process.Start("nautilus",
                    '"' + item + '"');
                break;
            case OsType.MacOS:
                System.Diagnostics.Process.Start("open",
                    '"' + item + '"');
                break;
        }
    }

    public static List<string> GetAllConfig(GameSettingObj obj)
    {
        var list = new List<string>();
        var dir = obj.GetGameDir().Length + 1;

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
        var dir = obj.GetGameDir();

        return File.ReadAllText(Path.GetFullPath(dir + "/" + name));
    }

    public static void SaveConfigFile(GameSettingObj obj, string name, string text)
    {
        var dir = obj.GetGameDir();

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

    public static Task<List<string>?> GetForgeSupportVersion()
    {
        return ForgeHelper.GetSupportVersion();
    }

    public static Task<List<string>?> GetFabricSupportVersion()
    {
        return FabricHelper.GetSupportVersion();
    }

    public static Task<List<string>?> GetQuiltSupportVersion()
    {
        return QuiltHelper.GetSupportVersion();
    }
}
